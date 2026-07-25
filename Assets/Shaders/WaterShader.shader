Shader "Learning/WaterShader"
{
    Properties
    {
        [MainColor] _WaterColor("Water Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
            "LightMode" = "UniversalForward"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
                float3 posW : TEXCOORD2;
            };

            static const int MAX_WAVES = 16;

            float4 _WaterColor;
            CBUFFER_START(UnityPerMaterial)
                int    _waveCount;
                float4 _waveData[16]; //  (Amp , Wave length , speed , steepness)
                float4 _waveDir[16]; // (dir.x , dir.y , 0,0)
            CBUFFER_END

            // float _k;
            // float WaterHeight(float3 pos) {
            //     float w = 2. / _waveLen;
            //
            //     float val = sin(dot(_dir.xy  , pos.xz) * w + (_speed * w ) * _Time.y) + 1;
            //     val = pow(val , _k);
            //     val /= pow(2 , _k);
            //
            //     float height = 0.;
            //     height = 2 * _amp * val;
            //     return height;
            // }

            float3 CalculateGerstnerWave(int index, float3 worldPos, out float3 normal) {

                float amp = _waveData[index].x;
                float waveLen = _waveData[index].y;
                float speed = _waveData[index].z;
                float steep = _waveData[index].w;

                float2 dir = _waveDir[index].xy;

                float w = 6.2831853 / max(0.0001, waveLen);
                float phase = speed * w * _Time.y;
                float angle = w * dot(dir, worldPos.xz) + phase;

                float sinAngle = sin(angle);
                float cosAngle = cos(angle);

                // Gerstner 3D offset (x, y, z)
                float3 offset;
                offset.x = steep * amp * dir.x * cosAngle;
                offset.y = amp * sinAngle;
                offset.z = steep * amp * dir.y * cosAngle;


                normal.x = -dir.x * w * amp * cos(angle);
                normal.z = -dir.y * w * amp * cos(angle);
                normal.y = 1 - steep * w * amp * sin(angle);

                normal = normalize(normal);
                return offset;
            }

            // Function to accumulate all active waves
            float3 GetTotalWaveDisplacement(float3 worldPos, out float3 finalNormal) {
                float3 totalDisplacement = float3(0, 0, 0);
                finalNormal = float3(0,1,0);

                for (int i = 0; i < _waveCount; i++) {
                    float3 normal;
                    totalDisplacement += CalculateGerstnerWave(i, worldPos, normal);
                    finalNormal += normal;
                }

                return totalDisplacement;
            }


            v2f vert(Attributes val) {
                float3 posW = TransformObjectToWorld(val.positionOS);
                float3 normal;
                posW += GetTotalWaveDisplacement(posW, normal);


                v2f OUT;
                OUT.positionHCS = TransformWorldToHClip(posW);
                OUT.posW = posW;
                OUT.normal = normal;
                OUT.uv = val.uv;
                return OUT;
            }

            half4 frag(v2f val) : SV_Target {
                float3 normal = normalize(val.normal);
                Light  mainLight = GetMainLight();
                float3 camDir = normalize(GetCameraPositionWS() - val.posW);

                float3 lightDir = mainLight.direction;
                float3 reflectedDir = reflect(-lightDir, normal);


                float ambient = 0.2f;
                float diffuse = saturate(dot(lightDir, normal));
                float specular = saturate(pow(saturate(dot(reflectedDir, camDir)), 100));

                half3 color = _WaterColor.rgb * (ambient + diffuse * mainLight.color) + specular;
                return half4(color, _WaterColor.a);
            }
            ENDHLSL
        }
    }
}
