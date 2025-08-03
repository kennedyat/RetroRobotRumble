using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class FullScreenOutline : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        [Range(0, 10)] public float Scale = 1.0f;
        public Color OutlineColor = Color.black;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    private class FullScreenOutlinePass : ScriptableRenderPass
    {

        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("ColorBlit");
        Material m_Material;
        RTHandle m_CameraColorTarget;
        RTHandle tempRT;
        private OutlineSettings settings;

        public FullScreenOutlinePass(Material material,OutlineSettings settings)
        {
            this.settings = settings;
            m_Material = material;
            renderPassEvent = settings.renderPassEvent;
        }

        public void SetTarget(RTHandle colorHandle)
        {
            m_CameraColorTarget = colorHandle;
        }

        // Called before executing the render pass.
        // Used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(m_CameraColorTarget);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            m_Material.SetColor("_Color", settings.OutlineColor);
            m_Material.SetFloat("_Scale", settings.Scale);

            var cameraData = renderingData.cameraData;
            if (cameraData.camera.cameraType != CameraType.Game || m_Material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("FullScreenOutline");

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                // This sets _MainTex in your shader
                cmd.Blit(m_CameraColorTarget, m_CameraColorTarget, m_Material);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (tempRT != null)
                tempRT.Release();
        }
    }
    
     public Shader m_Shader;
    
    [SerializeField]
    public OutlineSettings settings = new (); 

    Material m_Material;

    FullScreenOutlinePass m_RenderPass = null;

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                    ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(m_RenderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                        in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
            // ensures that the opaque texture is available to the Render Pass.
           m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Normal);

            m_RenderPass.SetTarget(renderer.cameraColorTargetHandle);
        }
    }

    public override void Create()
    {
        m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
        m_RenderPass = new FullScreenOutlinePass(m_Material, settings);

        if (!m_Shader || !m_Shader.isSupported) Debug.LogError("Shader is null or not supported!");

    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_Material);
    }
}


