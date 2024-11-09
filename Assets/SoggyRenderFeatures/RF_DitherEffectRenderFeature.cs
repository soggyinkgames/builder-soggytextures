using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;


public class RF_DitherEffectRenderFeature : ScriptableRendererFeature
{
    class DitherEffectPass : ScriptableRenderPass
    {
        const string m_PassName = "DitherEffectPass";
        public Material m_BlitMaterial;

        public void Setup(Material material)
        {
            m_BlitMaterial = material;
            requiresIntermediateTexture = true;
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var stack = VolumeManager.instance.stack;
            var customEffect = stack.GetComponent<SphereVolumeComponent>();

            if(!customEffect.IsActive())
            {
                return;
            }

            var resourceData = frameData.Get<UniversalResourceData>();

            if(resourceData.isActiveTargetBackBuffer)
            {
                Debug.LogError("Skipping render pass. RF_DitherEffectRenderFeature requires an intermediate ColorTexture, we cant use the back buffer as a texture input");
                return;
            }

            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{m_PassName}";
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters para = new(source, destination, m_BlitMaterial, 0);
            renderGraph.AddBlitPass(para, passName: m_PassName);

            resourceData.cameraColor = destination;
        }
    }

    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
    public Material material;

    DitherEffectPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new DitherEffectPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = injectionPoint;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(material == null)
        {
            Debug.LogWarning("RF_DitherEffectRenderFeature material is null and will be skipped.");
            return;
        }
        
        m_ScriptablePass.Setup(material);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}

