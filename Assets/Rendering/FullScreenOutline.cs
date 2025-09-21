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

        public LayerMask layerMask;
    }

    private class FullScreenOutlinePass : ScriptableRenderPass
    {

        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("ColorBlit");
        Material m_Material;
        RTHandle m_CameraColorTarget;
        RTHandle tempRT;
        private FilteringSettings filteringSettings;
        private OutlineSettings settings;



        public FullScreenOutlinePass(Material material, OutlineSettings settings)
        {
            this.settings = settings;
            m_Material = material;
            renderPassEvent = settings.renderPassEvent;

            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.layerMask);
        }


        public void SetTarget(RTHandle colorHandle)
        {
            m_CameraColorTarget = colorHandle;
        }


        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor textureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            textureDescriptor.colorFormat = RenderTextureFormat.Default;
            textureDescriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, textureDescriptor, FilterMode.Bilinear);

            ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Depth);

            ConfigureTarget(m_CameraColorTarget);

        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {

            if (m_Material == null || m_CameraColorTarget == null || tempRT == null)
                return;

            var cmd = CommandBufferPool.Get("FullScreenOutline");

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                m_Material.SetColor("_Color", settings.OutlineColor);
                m_Material.SetFloat("_Scale", settings.Scale);

                cmd.Blit(m_CameraColorTarget, tempRT, m_Material);
                cmd.Blit(tempRT, m_CameraColorTarget);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Release()
        {
            CoreUtils.Destroy(m_Material);

            tempRT?.Release();
        }

        /*public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (tempRT != null)
                tempRT.Release();
        }*/


    }

    public Shader m_Shader;

    [SerializeField]
    public OutlineSettings settings = new();

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

            m_RenderPass.SetTarget(renderer.cameraColorTargetHandle);
        }
    }

    public override void Create()
    {
        m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
        m_RenderPass = new FullScreenOutlinePass(m_Material, settings);

        if (!m_Shader || !m_Shader.isSupported)
            Debug.LogError("Shader is null or not supported!");

    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(m_Material);


    }
}


