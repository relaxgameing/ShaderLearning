using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class CameraDepthToRenderTexture : ScriptableRendererFeature
{
    CamearDepthToRenderTexturePass m_ScriptablePass;
    [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent
        .AfterRenderingTransparents;

    [SerializeField]  private Material mat;

    public override void Create()
    {
        m_ScriptablePass = new CamearDepthToRenderTexturePass(mat);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = injectionPoint;

        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        var cam = renderingData.cameraData.baseCamera;
        var target = EditorToRenderFeatureBridge._gameCam;

        if (renderingData.cameraData.cameraType != CameraType.Game || !EditorToRenderFeatureBridge
            .isEnabled) {
            // Debug.Log("rejecting cam");
            return;
        }

        renderer.EnqueuePass(m_ScriptablePass);
    }


    class CamearDepthToRenderTexturePass : ScriptableRenderPass
    {
        private Material _mat;
        public CamearDepthToRenderTexturePass(Material mat) {
            _mat = mat;
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Target Camera Depth Texture Copy Pass";
            var resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer) {
                Debug.Log("Camera Depth copy is in back buffer");
                return;
            }

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            resourceData.activeDepthTexture.GetDescriptor(renderGraph);
            desc.colorFormat = RenderTextureFormat.RFloat;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateHandleIfNeeded(ref EditorToRenderFeatureBridge
                ._gameCamDepthTexture,
                desc, FilterMode.Point,
                TextureWrapMode.Clamp, name: "SharedDepthTex");

            var outTex = renderGraph.ImportTexture(EditorToRenderFeatureBridge
                ._gameCamDepthTexture);


            var param = new RenderGraphUtils.BlitMaterialParameters(
                resourceData.activeDepthTexture , outTex , _mat , 0);

            renderGraph.AddBlitPass(param, passName);

            Debug.Log("Depth Texture copied");
        }
    }
}
