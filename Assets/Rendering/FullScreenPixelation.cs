using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenPixelation : ScriptableRendererFeature
{
    [System.Serializable]
    public class CelShadingSettings
    {
        public Color BaseColor = new Color(1f, 1f, 1f, 1f);
        public float Shine = 1.0f;
        public float Rim = 1.0f;
        public Color SecondaryColor = new Color(.25f, .5f, .7f, 1f);
        public float PixelSize = 0.01f;
        public LayerMask layerMask = -1; // -1 means "Everything"
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    private class CelShadingPass : ScriptableRenderPass
    {
        private Material material;
        private CelShadingSettings settings;
        private RTHandle tempRT;
        private RTHandle maskRT;
        private FilteringSettings filteringSettings;
        private List<ShaderTagId> shaderTagsList;
        private Material maskMaterial;

        public CelShadingPass(Material mat, CelShadingSettings settings)
        {
            this.material = mat;
            this.settings = settings;
            renderPassEvent = settings.renderPassEvent;

            // Create simple mask material
            Shader maskShader = Shader.Find("Hidden/Universal Render Pipeline/FallbackError");
            if (maskShader == null)
                maskShader = Shader.Find("Unlit/Color");
            
            maskMaterial = new Material(maskShader);
            maskMaterial.color = Color.white;

            // Setup filtering for layer mask
            filteringSettings = new FilteringSettings(RenderQueueRange.all, settings.layerMask);
            
            shaderTagsList = new List<ShaderTagId>
            {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };
        }

        public void UpdateLayerMask(LayerMask mask)
        {
            settings.layerMask = mask;
            filteringSettings = new FilteringSettings(RenderQueueRange.all, mask);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_CelTempRT");
            
            // Mask RT for layer filtering
            var maskDescriptor = descriptor;
            maskDescriptor.colorFormat = RenderTextureFormat.R8;
            maskDescriptor.depthBufferBits = 24;
            RenderingUtils.ReAllocateIfNeeded(ref maskRT, maskDescriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_CelLayerMask");

            ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Depth);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
                return;

            var cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            
            CommandBuffer cmd = CommandBufferPool.Get("CelShading");

            // Only create mask if layer mask is not "Everything"
            if (settings.layerMask != -1)
            {
                // Render mask of objects on selected layers
                cmd.SetRenderTarget(maskRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmd.ClearRenderTarget(true, true, Color.black);
                
                var drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, SortingCriteria.CommonOpaque);
                drawingSettings.overrideMaterial = maskMaterial;
                drawingSettings.overrideMaterialPassIndex = 0;

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
                
                material.SetTexture("_LayerMask", maskRT);
                material.EnableKeyword("_USE_LAYER_MASK");
            }
            else
            {
                material.DisableKeyword("_USE_LAYER_MASK");
            }

            // Update material properties
            material.SetColor("_BaseColor", settings.BaseColor);
            material.SetFloat("_Shine", settings.Shine);
            material.SetColor("_SecondaryColor", settings.SecondaryColor);
            material.SetFloat("_Rim", settings.Rim);
            material.SetFloat("_PixelSize", settings.PixelSize);

            // Blit with cel shading effect
            Blitter.BlitCameraTexture(cmd, cameraTarget, tempRT, material, 0);
            Blitter.BlitCameraTexture(cmd, tempRT, cameraTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempRT?.Release();
            maskRT?.Release();
            if (maskMaterial != null)
                Object.DestroyImmediate(maskMaterial);
        }
    }

    public Shader celShader;
    public CelShadingSettings settings = new CelShadingSettings();

    private Material celMaterial;
    private CelShadingPass celPass;

    public override void Create()
    {
        if (celShader == null)
        {
            Debug.LogError("Cel Shader is not assigned!");
            return;
        }

        celMaterial = CoreUtils.CreateEngineMaterial(celShader);
        celPass = new CelShadingPass(celMaterial, settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (celMaterial == null)
        {
            Debug.LogWarning("Cel material is null!");
            return;
        }
        
        // Update layer mask if it changed
        celPass.UpdateLayerMask(settings.layerMask);
        
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(celPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        celPass?.Dispose();
        CoreUtils.Destroy(celMaterial);
    }
}