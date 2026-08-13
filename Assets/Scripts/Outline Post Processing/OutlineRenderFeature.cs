using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class OutlineRenderFeature : ScriptableRendererFeature {
    [SerializeField] OutlineRenderFeatureSettings settings;
    OutlineRenderFeaturePass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create() {
        m_ScriptablePass = new OutlineRenderFeaturePass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = settings.injectionPoint;

        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color |
                                        ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        //m_ScriptablePass.requiresIntermediateTexture = true;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer,
        ref RenderingData renderingData) {
        if (renderingData.cameraData.renderType == CameraRenderType.Overlay) {
            Debug.Log("outline pass added");
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }

    // Use this class to pass around settings from the feature to the pass
    [Serializable]
    public class OutlineRenderFeatureSettings {
        public Material outlinePostProcessingMat;
        public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
    }

    class OutlineRenderFeaturePass : ScriptableRenderPass {
        readonly OutlineRenderFeatureSettings _settings;

        public OutlineRenderFeaturePass(OutlineRenderFeatureSettings settings) {
            this._settings = settings;
        }


        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void
            RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {
            const string passName = "Outline Render Pass";

            var resourceData = frameData.Get<UniversalResourceData>();

            var dstDesc =
                renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            dstDesc.name = "_OutlineOut";
            dstDesc.clearBuffer = true;
            TextureHandle dstTex = renderGraph.CreateTexture(dstDesc);

            var param = new RenderGraphUtils.BlitMaterialParameters(
                resourceData.activeColorTexture,
                dstTex,
                _settings.outlinePostProcessingMat,
                0);

            renderGraph.AddBlitPass(param, passName);

            resourceData.cameraColor = dstTex;
        }
    }
}
