using System;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
#endif
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;

namespace CompoundRendererFeature.PostProcess {
[Serializable, VolumeComponentMenu("Quibli/Stylized Detail")]
public class StylizedDetail : VolumeComponent {
    [Tooltip("Controls the amount of contrast added to the image details.")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0f, 0f, 3f, true);

    [Tooltip("Controls smoothing amount.")]
    public ClampedFloatParameter blur = new ClampedFloatParameter(1f, 0, 2, true);

    [Tooltip("Controls structure within the image.")]
    public ClampedFloatParameter edgePreserve = new ClampedFloatParameter(1.25f, 0, 2, true);

    [Tooltip("The distance from the camera at which the effect starts."), Space]
    public MinFloatParameter rangeStart = new MinFloatParameter(10f, 0f);

    [Tooltip("The distance from the camera at which the effect reaches its maximum radius.")]
    public MinFloatParameter rangeEnd = new MinFloatParameter(30f, 0f);
}

[CompoundRendererFeature("Stylized Detail", InjectionPoint.BeforePostProcess)]
public class StylizedDetailRenderer : CompoundRenderer {
    private StylizedDetail _volumeComponent;

    private Material _effectMaterial;

#if UNITY_6000_0_OR_NEWER
    // Pass data for the final combine in RenderGraph
    private class CombinePassData {
        public TextureHandle src;
        public TextureHandle blur1;
        public TextureHandle blur2;
        public Material mat;
    }
#endif

    static class PropertyIDs {
        internal static readonly int Input = Shader.PropertyToID("_MainTex");
        internal static readonly int PingTexture = Shader.PropertyToID("_PingTexture");
        internal static readonly int BlurStrength = Shader.PropertyToID("_BlurStrength");
        internal static readonly int Blur1 = Shader.PropertyToID("_BlurTex1");
        internal static readonly int Blur2 = Shader.PropertyToID("_BlurTex2");
        internal static readonly int Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int DownSampleScaleFactor = Shader.PropertyToID("_DownSampleScaleFactor");
        public static readonly int CoCParams = Shader.PropertyToID("_CoCParams");
    }

    public override ScriptableRenderPassInput input =>
        ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth;

    // Called only once before the first render call.
    public override void Initialize() {
        base.Initialize();
        _effectMaterial = CoreUtils.CreateEngineMaterial("Hidden/CompoundRendererFeature/StylizedDetail");
    }

    // Called for each camera/injection point pair on each frame.
    // Return true if the effect should be rendered for this camera.
    public override bool Setup(in RenderingData renderingData, InjectionPoint injectionPoint) {
        base.Setup(in renderingData, injectionPoint);
        var stack = VolumeManager.instance.stack;
        _volumeComponent = stack.GetComponent<StylizedDetail>();
        bool shouldRenderEffect = _volumeComponent.intensity.value > 0;
        return shouldRenderEffect;
    }

    public override void Render(CommandBuffer cmd, RTHandle source, RTHandle destination,
                                ref RenderingData renderingData, InjectionPoint injectionPoint) {
        const int downSample = 1;

        RenderTextureDescriptor descriptor = GetTempRTDescriptor(renderingData);
        int wh = descriptor.width / downSample;
        int hh = descriptor.height / downSample;

        // Assumes a radius of 1 is 1 at 1080p. Past a certain radius our gaussian kernel will look very bad so we'll
        // clamp it for very high resolutions (4K+).
        float blurRadius = _volumeComponent.blur.value * (wh / 1080f);
        blurRadius = Mathf.Min(blurRadius, 2f);
        float edgePreserve = _volumeComponent.edgePreserve.value * (wh / 1080f);
        edgePreserve = Mathf.Min(edgePreserve, 2f);

        var rangeStart = _volumeComponent.rangeStart.overrideState ? _volumeComponent.rangeStart.value : 0;
        var rangeEnd = _volumeComponent.rangeEnd.overrideState ? _volumeComponent.rangeEnd.value : -1;
        _effectMaterial.SetVector(PropertyIDs.CoCParams, new Vector2(rangeStart, rangeEnd));

        _effectMaterial.SetFloat(PropertyIDs.Intensity, _volumeComponent.intensity.value);
        SetSourceSize(cmd, descriptor);

        var tempRtDescriptor = GetTempRTDescriptor(renderingData, wh, hh, _defaultHDRFormat);
        cmd.GetTemporaryRT(PropertyIDs.PingTexture, tempRtDescriptor, FilterMode.Bilinear);
        cmd.GetTemporaryRT(PropertyIDs.Blur1, tempRtDescriptor, FilterMode.Bilinear);
        cmd.GetTemporaryRT(PropertyIDs.Blur2, tempRtDescriptor, FilterMode.Bilinear);

        cmd.SetGlobalVector(PropertyIDs.DownSampleScaleFactor,
                            new Vector4(1.0f / downSample, 1.0f / downSample, downSample, downSample));

        cmd.SetGlobalFloat(PropertyIDs.BlurStrength, edgePreserve);
        cmd.SetGlobalTexture(PropertyIDs.Input, source);
        CoreUtils.DrawFullScreen(cmd, _effectMaterial, PropertyIDs.PingTexture, properties: null, shaderPassId: 1);
        cmd.SetGlobalTexture(PropertyIDs.Input, PropertyIDs.PingTexture);
        CoreUtils.DrawFullScreen(cmd, _effectMaterial, PropertyIDs.Blur1, properties: null, shaderPassId: 2);

        cmd.SetGlobalFloat(PropertyIDs.BlurStrength, blurRadius);
        cmd.SetGlobalTexture(PropertyIDs.Input, PropertyIDs.Blur1);
        CoreUtils.DrawFullScreen(cmd, _effectMaterial, PropertyIDs.PingTexture, properties: null, shaderPassId: 1);
        cmd.SetGlobalTexture(PropertyIDs.Input, PropertyIDs.PingTexture);
        CoreUtils.DrawFullScreen(cmd, _effectMaterial, PropertyIDs.Blur2, properties: null, shaderPassId: 2);

        cmd.SetGlobalTexture(PropertyIDs.Input, source);
        CoreUtils.DrawFullScreen(cmd, _effectMaterial, destination, properties: null, shaderPassId: 0);

        cmd.ReleaseTemporaryRT(PropertyIDs.PingTexture);
        cmd.ReleaseTemporaryRT(PropertyIDs.Blur1);
        cmd.ReleaseTemporaryRT(PropertyIDs.Blur2);
    }

#if UNITY_6000_0_OR_NEWER
    public override void RenderWithGraph(RenderGraph renderGraph, TextureHandle source, TextureHandle destination,
                                         RenderTextureDescriptor intermediateDescriptor) {
        if (!source.IsValid() || !destination.IsValid()) return;
        if (_effectMaterial == null) return;
        const int downSample = 1;

        int wh = intermediateDescriptor.width / downSample;
        int hh = intermediateDescriptor.height / downSample;

        // Provide _SourceSize for shaders (parity with non-RenderGraph path)
        float w = intermediateDescriptor.width;
        float h = intermediateDescriptor.height;
        if (intermediateDescriptor.useDynamicScale) {
            w *= ScalableBufferManager.widthScaleFactor;
            h *= ScalableBufferManager.heightScaleFactor;
        }

        Shader.SetGlobalVector(Shader.PropertyToID("_SourceSize"), new Vector4(w, h, 1.0f / w, 1.0f / h));

        // Assumes a radius of 1 is 1 at 1080p. Past a certain radius our gaussian kernel will look very bad so we'll
        // clamp it for very high resolutions (4K+).
        float blurRadius = _volumeComponent.blur.value * (wh / 1080f);
        blurRadius = Mathf.Min(blurRadius, 2f);
        float edgePreserve = _volumeComponent.edgePreserve.value * (wh / 1080f);
        edgePreserve = Mathf.Min(edgePreserve, 2f);

        var rangeStart = _volumeComponent.rangeStart.overrideState ? _volumeComponent.rangeStart.value : 0;
        var rangeEnd = _volumeComponent.rangeEnd.overrideState ? _volumeComponent.rangeEnd.value : -1;
        _effectMaterial.SetVector(PropertyIDs.CoCParams, new Vector2(rangeStart, rangeEnd));

        _effectMaterial.SetFloat(PropertyIDs.Intensity, _volumeComponent.intensity.value);

        _effectMaterial.SetVector(PropertyIDs.DownSampleScaleFactor,
                                  new Vector4(1.0f / downSample, 1.0f / downSample, downSample, downSample));

        // Allocate downsampled blur intermediates (parity with non-RG path intent)
        var smallDesc = intermediateDescriptor;
        smallDesc.width = wh;
        smallDesc.height = hh;
        smallDesc.msaaSamples = 1;
        smallDesc.depthBufferBits = 0;

        TextureHandle blur1 =
            UniversalRenderer.CreateRenderGraphTexture(renderGraph, smallDesc, "_BlurTex1", clear: true);
        TextureHandle blur2 =
            UniversalRenderer.CreateRenderGraphTexture(renderGraph, smallDesc, "_BlurTex2", clear: true);

        RenderGraphUtils.BlitMaterialParameters blit;
        {
            _effectMaterial.SetFloat(PropertyIDs.BlurStrength, edgePreserve);

            blit = new(source, destination, _effectMaterial, shaderPass: 1) {
                sourceTexturePropertyID = PropertyIDs.Input,
                geometry = RenderGraphUtils.FullScreenGeometryType.ProceduralTriangle
            };
            renderGraph.AddBlitPass(blit, passName: $"{_effectMaterial.name}_Pass{blit.shaderPass}");

            blit.source = destination;
            blit.destination = blur1;
            blit.shaderPass = 2;
            blit.sourceTexturePropertyID = PropertyIDs.Input;
            blit.geometry = RenderGraphUtils.FullScreenGeometryType.ProceduralTriangle;
            renderGraph.AddBlitPass(blit, passName: $"{_effectMaterial.name}_Pass{blit.shaderPass}");
        }

        {
            _effectMaterial.SetFloat(PropertyIDs.BlurStrength, blurRadius);

            blit.source = blur1;
            blit.destination = destination;
            blit.shaderPass = 1;
            blit.geometry = RenderGraphUtils.FullScreenGeometryType.ProceduralTriangle;
            renderGraph.AddBlitPass(blit, passName: $"{_effectMaterial.name}_Pass{blit.shaderPass}");

            blit.source = destination;
            blit.destination = blur2;
            blit.shaderPass = 2;
            blit.sourceTexturePropertyID = PropertyIDs.Input;
            blit.geometry = RenderGraphUtils.FullScreenGeometryType.ProceduralTriangle;
            renderGraph.AddBlitPass(blit, passName: $"{_effectMaterial.name}_Pass{blit.shaderPass}");
        }

        {
            // Final combine needs multiple textures (source, blur1, blur2). Use a raster pass.
            // Example: MrtRendererFeature in URP Samples for RenderGraph.
            using (var builder =
                   renderGraph.AddRasterRenderPass<CombinePassData>("StylizedDetail_Combine", out var passData)) {
                passData.src = source;
                passData.blur1 = blur1;
                passData.blur2 = blur2;
                passData.mat = _effectMaterial;

                builder.UseTexture(passData.src);
                builder.UseTexture(passData.blur1);
                builder.UseTexture(passData.blur2);
                builder.SetRenderAttachment(destination, 0);

                builder.SetRenderFunc((CombinePassData data, RasterGraphContext ctx) => {
                    data.mat.SetTexture(PropertyIDs.Input, data.src);
                    data.mat.SetTexture(PropertyIDs.Blur1, data.blur1);
                    data.mat.SetTexture(PropertyIDs.Blur2, data.blur2);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, 0, MeshTopology.Triangles, 3);
                });
            }
        }
    }
#endif

    public override void Dispose(bool disposing) { }
}
}
