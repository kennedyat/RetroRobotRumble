// FILE: DepthNormalsPass.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthNormalsPass : ScriptableRenderPass
{
    private RenderTargetHandle depthNormalsHandle;
    private RenderTargetIdentifier cameraColorTarget;
    private Material depthNormalsMaterial;
    private FilteringSettings filteringSettings;
    private ShaderTagId shaderTagId = new ShaderTagId("DepthNormals");

    public DepthNormalsPass(RenderQueueRange renderQueueRange, LayerMask layerMask, Material material)
    {
        filteringSettings = new FilteringSettings(renderQueueRange, layerMask);
        depthNormalsMaterial = material;
        depthNormalsHandle.Init("_CustomDepthNormalsTexture");
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void SetTarget(RenderTargetIdentifier colorTarget)
    {
        cameraColorTarget = colorTarget;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        var descriptor = cameraTextureDescriptor;
        descriptor.colorFormat = RenderTextureFormat.ARGB32;
        descriptor.depthBufferBits = 0;
        cmd.GetTemporaryRT(depthNormalsHandle.id, descriptor, FilterMode.Point);
        ConfigureTarget(depthNormalsHandle.Identifier());
        ConfigureClear(ClearFlag.All, Color.black);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get("DepthNormalsPass");

        using (new ProfilingScope(cmd, new ProfilingSampler("DepthNormalsPass")))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            var drawSettings = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.CommonOpaque);
            drawSettings.overrideMaterial = depthNormalsMaterial;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        if (cmd == null) return;
        cmd.ReleaseTemporaryRT(depthNormalsHandle.id);
    }

    public RenderTargetHandle GetHandle()
    {
        return depthNormalsHandle;
    }
}
