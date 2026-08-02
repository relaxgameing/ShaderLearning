Shader "Learning/RayMarching"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION; // object space
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 camPos: TEXCOORD1;
                float3 postionWS:TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4  _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            v2f vert(Attributes IN) {
                v2f OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.camPos = _WorldSpaceCameraPos;
                OUT.postionWS = TransformObjectToWorld(IN.positionOS);
                return OUT;
            }

            float sdSphere(float3 p, float r) {
                return length(p) - r;
            }

            // t -> (x,y) , where x -> center radius , y -> ring radius
            float sdTorus(float3 p, float2 t) {
                float2 q = float2(length(p.xz) - t.x, p.y);
                return length(q) - t.y;
            }

            float sdf(float3 p) {
                float dist = min(sdSphere(p, 0.1) , sdTorus(p , float2(0.5 , 0.01)));

                return dist;
            }

            float3 getNormal(float3 p) {
                float2 e = float2(0.01, 0);
                float3 normal = sdf(p) -
                float3(
                    sdf(p + e.xyy),
                    sdf(p + e.yxy),
                    sdf(p + e.yyx)
                );

                return normalize(normal);
            }

            half4 frag(v2f val) : SV_Target {
                int    MaxStep = 80;
                float  MaxDist = 100;
                float  ContactDist = 0.01;
                float3 lightPosition = float3(
                    -10 * cos(_Time.y),
                    10,
                    -10 * sin(_Time.y)
                );

                float2 uv = val.uv - 0.5;

                float3 ro = val.camPos;
                float3 rd = normalize(val.postionWS - ro);

                float d = 0;
                int   i = 0;
                for (i = 0; i < MaxStep; ++i) {
                    float3 p = ro + d * rd;
                    float  safeDist = sdf(p);
                    d += safeDist;
                    if (safeDist < ContactDist || d > MaxDist) break;
                }

                float4 color = float4(0, 0, 0, 1);
                if (d >= MaxDist) {
                    discard ;
                }
                float3 p = ro + d * rd;
                float3 normal = getNormal(p);
                float3 lightDir = normalize(p - lightPosition);
                float  lumen = max(dot(normal, lightDir), 0);

                color = float4(1, 1, 1, 1) * lumen;

                return color;
            }
            ENDHLSL
        }
    }
}
