Shader "Learning/CameraVolumeVisualiser"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "CameraVolumeVisualiserPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float4x4 _GameCamViewMat;

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float rawDepth = SampleSceneDepth(uv);
                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float linearDepth = Linear01Depth(rawDepth, _ZBufferParams);

                float4 viewPos = float4(uv, rawDepth, 1.0);
                float4 worldPos = mul(UNITY_MATRIX_I_VP, viewPos);
                // worldPos.xyz /= worldPos.w;

                float4 pos = mul(_GameCamViewMat, worldPos);
                pos.xy /= pos.w;

                float isInside = (1 - step(1 , abs(pos.xy)));

                return  isInside * col;
            }
            ENDHLSL
        }
    }
}
