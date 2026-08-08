Shader "Learning/CameraVolumeVisualiser"
{
    Properties
    {
        _InsideColor("surface visible inside camera" , Color) = (1,1,1,1)
        _DistanceErrorThreshold("Distance Precision Error threshold" , Range(0,1)) = 0.01
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }
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

            float4 _InsideColor;
            float _DistanceErrorThreshold;

            float4x4 _GameCamProjMat;
            float4x4 _GameCamInvViewProjMat;
            float4   _GameCamPosWs;

            Texture2D _GameCamDepthTex;
            SAMPLER(sampler_GameCamDepthTex);

            float4 frag(Varyings input) : SV_Target {
                float2 uv = input.texcoord;
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float  rawDepth = SampleSceneDepth(uv);
                float3 worldPos = ComputeWorldSpacePosition(uv, rawDepth, UNITY_MATRIX_I_VP);


                float4 posCS = mul(_GameCamProjMat, float4(worldPos.xyz, 1.0));
                float3 ndc = posCS.xyz / posCS.w;

                float2 isInside = (1 - step(1, abs(ndc.xy)));
                float  insideZ = (1 - step(1, ndc.z)) * (step(0, ndc.z));
                float  inside = (isInside.x * isInside.y * insideZ);

                float2 screenUV = ndc * 0.5 + 0.5;
                screenUV.y = 1- screenUV.y;
                float  camDepth = SAMPLE_TEXTURE2D(_GameCamDepthTex, sampler_GameCamDepthTex, screenUV);

                if (inside) {
                    float3 depthPosWS = ComputeWorldSpacePosition(screenUV,
                                                       camDepth,
                                                       _GameCamInvViewProjMat);

                    float screenDepthDist = distance(_GameCamPosWs.xyz, depthPosWS);
                    float worldPosDist = distance(_GameCamPosWs.xyz, worldPos);
                    inside  *= step( worldPosDist , screenDepthDist + _DistanceErrorThreshold);
                }

                return inside * col * _InsideColor * 0.2 + (1 - inside) * col;
            }
            ENDHLSL
        }
    }
}
