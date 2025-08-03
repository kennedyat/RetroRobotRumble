using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScribbleTransitionRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class ScribbleSettings
    {
        public Material scribbleMaterial;
        [Range(0f, 1f)] public float progress = 0f;
    }

    public ScribbleSettings settings = new ScribbleSettings();

    class ScribblePass : ScriptableRenderPass
    {
        private Material material;
        private float progress;

        public ScribblePass(Material mat)
        {
            material = mat;
            // Run after post-processing, so your effect draws last
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public void Setup(float progress)
        {
            this.progress = progress;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
            {
                Debug.LogError("Scribble material is missing.");
                return;
            }

            // Get camera color target here inside Execute - it's valid here
            RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            if (cameraColorTarget == null)
            {
                Debug.LogError("cameraColorTargetHandle is null during Execute.");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("Scribble Effect");

            material.SetFloat("_Progress", progress);

            // Blit fullscreen from camera target to itself using your material
            Blit(cmd, cameraColorTarget, cameraColorTarget, material);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    ScribblePass pass;

    public override void Create()
    {
        if (settings.scribbleMaterial == null)
        {
            Debug.LogError("Scribble material is not assigned.");
            return;
        }

        pass = new ScribblePass(settings.scribbleMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.scribbleMaterial == null)
        {
            Debug.LogError("Scribble material is null in AddRenderPasses.");
            return;
        }

        // Just pass progress, no camera target here!
        pass.Setup(settings.progress);
        renderer.EnqueuePass(pass);
    }
}
