Shader "Learning/Outline"
{
    Properties
    {
        _OutlineColor("Outline Color" , Color) = (1,1,1,1)
        _EdgeThreshold("Edge Threshold" , Range(0.001 , 5)) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            static float _sobelX[9] = {
                -1., 0., 1.,
                -2., 0, 2.,
                -1., 0., 1.
            };

            static  float _sobelY[9] = {
                -1., -2., -1.,
                0., 0., 0.,
                1., 2., 1.
            };


            float4 _OutlineColor;
            float  _EdgeThreshold;

            half4 frag(Varyings val) : SV_Target {
                float2 uv = val.texcoord;
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

               float2 texelSize = 1.0 / _ScreenParams.xy;

                float gx = 0 , gy = 0;
                int count = 0;
                for (float i = -1; i <= 1; ++i) {
                    for (float j = -1; j <= 1; ++j) {
                        float2 curUv = uv + float2(i, j) * texelSize;
                        float rawDepth = SampleSceneDepth(curUv);
                        float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

                        gx +=  eyeDepth * _sobelX[count];
                        gy +=  eyeDepth * _sobelY[count];
                        count++;
                    }
                }
                float gt = gx * gx + gy * gy;

                float edge = step(_EdgeThreshold  * _EdgeThreshold, gt) ;

                return  edge * _OutlineColor + (1-edge) * col;
            }
            ENDHLSL
        }
    }
}
