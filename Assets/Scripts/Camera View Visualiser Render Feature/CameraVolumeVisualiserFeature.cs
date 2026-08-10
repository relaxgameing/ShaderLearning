using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class CameraVolumeVisualiserFeature : ScriptableRendererFeature {
    [SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
    [SerializeField]
    private Material _mat;
    CameraVolumeVisualiserFeaturePass m_ScriptablePass;
    private static int MatGameCamViewProjMatrix = Shader.PropertyToID("_GameCamProjMat");
    private static int MatGameCamWorldPos = Shader.PropertyToID("_GameCamPosWs");
    private static int MatGameCamInvViewProjMatrix = Shader.PropertyToID("_GameCamInvViewProjMat");
    private static int matGameCamDepthTexProperty = Shader.PropertyToID("_GameCamDepthTex");

    public override void Create()
    {
        m_ScriptablePass = new CameraVolumeVisualiserFeaturePass(_mat);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = injectionPoint;

        // You can request URP color texture and depth buffer as inputs by uncommenting the line below,
        // URP will ensure copies of these resources are available for sampling before executing the render pass.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        m_ScriptablePass.ConfigureInput(ScriptableRenderPassInput.Color | ScriptableRenderPassInput.Depth);

        // You can request URP to render to an intermediate texture by uncommenting the line below.
        // Use this option for passes that do not support rendering directly to the backbuffer.
        // Only uncomment it if necessary, it will have a performance impact, especially on mobiles and other TBDR GPUs where it will break render passes.
        // m_ScriptablePass.requiresIntermediateTexture = true;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (renderingData.cameraData.cameraType != CameraType.SceneView) {
            return;
        }

        if (!EditorToRenderFeatureBridge.hasData) {
            return;
        }

        renderer.EnqueuePass(m_ScriptablePass);
    }


    class CameraVolumeVisualiserFeaturePass : ScriptableRenderPass {

        private Material _mat;
        public CameraVolumeVisualiserFeaturePass(Material mat) {
            _mat = mat;
        }

        // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
        // FrameData is a context container through which URP resources can be accessed and managed.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Camera Volume Visualiser Pass";
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            var src = resourceData.activeColorTexture;

            if (!EditorToRenderFeatureBridge.hasData || !EditorToRenderFeatureBridge.isEnabled) {
                resourceData.cameraColor = src;
                return;
            }

            var dstDesc =
                renderGraph.GetTextureDesc(src);
            dstDesc.name = "_CamearVisualOut";
            dstDesc.clearBuffer = true;
            TextureHandle dst = renderGraph.CreateTexture(dstDesc);

            var cam = EditorToRenderFeatureBridge._gameCam;
            // var camRT = EditorToRenderFeatureBridge._gameCamRenderTexture;
            Matrix4x4 gpuProjection =
                GL.GetGPUProjectionMatrix(
                    cam.projectionMatrix,
                    renderIntoTexture: true
                );

            var projectionMat = gpuProjection* cam.worldToCameraMatrix;

            _mat.SetMatrix(MatGameCamViewProjMatrix, projectionMat);
            _mat.SetMatrix(MatGameCamInvViewProjMatrix, projectionMat.inverse);
            _mat.SetVector(MatGameCamWorldPos, cam.transform.position);

            if (!EditorToRenderFeatureBridge._gameCamDepthTexture.IsUnityNull()) {
                _mat.SetTexture(matGameCamDepthTexProperty , EditorToRenderFeatureBridge._gameCamDepthTexture);
            }

            renderGraph.AddCopyPass(src, dst);

            var bitParam =  new RenderGraphUtils.BlitMaterialParameters(dst ,src, _mat, 0);
            renderGraph.AddBlitPass(bitParam , passName);

            resourceData.cameraColor = src;
            Debug.Log("full pass ");
        }
    }
}
