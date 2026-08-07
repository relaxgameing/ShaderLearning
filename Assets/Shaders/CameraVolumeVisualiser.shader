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

            float4x4 _GameCamProjMat;

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord  ;
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float rawDepth = SampleSceneDepth(uv);
                // float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                // float linearDepth = Linear01Depth(rawDepth, _ZBufferParams);

                float4 viewPos = float4(uv * 2 -1  , rawDepth, 1.0);
                float4 worldPos = mul(UNITY_MATRIX_I_VP, viewPos);
                worldPos.xyz /= worldPos.w;

                float4 pos = mul(_GameCamProjMat, float4(worldPos.xyz , 1.0));
                pos.xyz /= pos.w;

                float2 isInside = (1 - step(1 , abs(pos.xy)));
                float insideZ=  (1 - step(1 , pos.z)) * (step(0 , pos.z));
                float inside =  (isInside.x * isInside.y * insideZ );

                return inside * col * 0.2 + (1 - inside) * col;
                // return 1 -col;
                // return float4(uv  , 0 , 1);
                // return worldPos;
            }
            ENDHLSL
        }
    }
}
