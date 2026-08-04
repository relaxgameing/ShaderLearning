Shader "Learning/RayMarching"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _PlanetColor("Planet color" , Color) = (1,1,1,1)
        _DiskColor("Planet Disk color" , Color) = (1,1,1,1)
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
            float4 _PlanetColor;
            float4 _DiskColor;

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
                // OUT.postionWS = (IN.positionOS);
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

            float2x2 rot2D(float angle) {
                angle = DegToRad(angle);
                float c = cos(angle);
                float s = sin(angle);
                return float2x2(c, -s, s, c);
            }

            float3 scale(float3 p  , float3 scaleVal) {
                p.x *= scaleVal.x;
                p.y *= scaleVal.y;
                p.z *= scaleVal.z;
                return p;
            }

            float3 translate(float3 p , float3 dist) {
                return p + dist;
            }

            float sdf(float3 p , out float4 color ) {
                p = translate(p , float3(0 , _CosTime.w , 0));

                float  dist = sdSphere( p, 0.1);
                color = _PlanetColor;
                float3 rotP = p;

                rotP.xy = mul(rot2D( 10 ), p.xy);
                rotP.xz = mul(rot2D(_Time.y  * 10 ), p.xz);
                float diskDist = sdTorus(scale(rotP ,float3(1 , 1 , 1) ), float2(0.5, 0.01));

                if (diskDist < dist) {
                    dist = diskDist;
                    color = _DiskColor;
                }

                return dist;
            }

            float3 getNormal(float3 p) {
                float4 color;
                float2 e = float2(0.01, 0);
                float3 normal = sdf(p ,color ) -
                float3(
                    sdf(p + e.xyy , color),
                    sdf(p + e.yxy , color),
                    sdf(p + e.yyx , color)
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

                float3 ro = (val.camPos);
                float3 rd = normalize(val.postionWS - ro);

                float d = 0;
                int   i = 0;
                float4 color = float4(0, 0, 0, 1);
                for (i = 0; i < MaxStep; ++i) {
                    float3 p = ro + d * rd;
                    float  safeDist = sdf(p ,color);
                    d += safeDist;
                    if (safeDist < ContactDist || d > MaxDist) break;
                }

                if (d >= MaxDist) {
                    float2 screenUv = GetNormalizedScreenSpaceUV(val.positionHCS);
                    return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, screenUv);
                }

                float3 p = ro + d * rd;
                float3 normal = getNormal(p);
                float3 lightDir = normalize(p - lightPosition);
                float  lumen = max(dot(normal, lightDir), 0);

                color = color * lumen;

                return color;
            }
            ENDHLSL
        }
    }
}
