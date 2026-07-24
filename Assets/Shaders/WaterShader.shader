Shader "Learning/WaterShader"
{
    Properties
    {
        [MainColor] _WaterColor("Water Color", Color) = (1, 1, 1, 1)
        _amp ("Amplitude " , Float) = 1.
        _waveLen ("Wave length" , Float) = 1.
        _speed("Speed" , Float) = 1
        _steep("Steepness" , Range(0,1)) = 1
        _dir ("Direction",  Vector) = (0.5 , 0.5 , 0 , 0)
        _k ("K" , Float) = 10.
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
            "LightMode" = "UniversalForward"
        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal: TEXCOORD1;
                float3 posW : TEXCOORD2;
            };


            float4 _WaterColor;
            float _amp;
            float _waveLen;
            float _speed;
            float _steep;
            float _k;
            float4 _dir;

            float WaterHeight(float3 pos) {
                float w = 2. / _waveLen;

                float val = sin(dot(_dir.xy  , pos.xz) * w + (_speed * w ) * _Time.y) + 1;
                val = pow(val , _k);
                val /= pow(2 , _k);

                float height = 0.;
                height = 2 * _amp * val;
                return height;
            }

            float3 GerstnerWave(float3 pos , out float3 normal) {
                float w = 6.2831853 / _waveLen;
                float phase = _speed * w * _Time.y;
                float2 d = normalize(_dir.xy);
                float proj = dot(d, pos.xz);
                float angle = w * proj + phase ;

                float3 p = float3(0 , 0 , 0) ;
                p.x = pos.x + _steep * _amp * d.x * cos(angle);
                p.z = pos.z + _steep * _amp * d.y * cos(angle);
                p.y = pos.y + _amp * sin(angle);

                normal.x = - d.x * w * _amp * cos(angle);
                normal.z = - d.y * w * _amp * cos(angle);
                normal.y = 1 - _steep * w * _amp * sin(angle);

                normal = normalize(normal);
                return p;
            }


             v2f vert(Attributes val)
            {
                float3 posW = TransformObjectToWorld(val.positionOS);
                float3 normal;
                posW= GerstnerWave(posW , normal);



                v2f OUT;
                OUT.positionHCS = TransformWorldToHClip(posW);
                OUT.posW = posW;
                OUT.normal = normal;
                OUT.uv = val.uv;
                return OUT;
            }

            half4 frag( v2f val) : SV_Target
            {

                float3 normal= normalize(val.normal);
                Light mainLight = GetMainLight();
                float3 camDir = normalize(GetCameraPositionWS() - val.posW);

                float3 lightDir = mainLight.direction;
                float3 reflectedDir = reflect(-lightDir , normal);


                float ambient = 0.2f;
                float diffuse = saturate(dot(lightDir , normal));
                float specular =saturate(pow( saturate(dot(reflectedDir , camDir)), 100));

                half3 color =_WaterColor.rgb * (ambient + diffuse * mainLight.color ) + specular;
                return half4(color, _WaterColor.a);

            }
            ENDHLSL
        }
    }
}
