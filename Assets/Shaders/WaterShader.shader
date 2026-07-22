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
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
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

            float3 GerstnerWave(float3 pos) {
                float w = 6.2831853 / _waveLen;
                float phase = _speed * w;
                float2 d = normalize(_dir.xy);
                float proj = dot(d, pos.xz);
                float angle = w * proj + phase * _Time.x;

                float3 p = float3(0 , 0 , 0) ;
                p.x = pos.x + _steep * _amp * d.x * cos(angle);
                p.z = pos.z + _steep * _amp * d.y * cos(angle);

                p.y = pos.y + _amp * sin(angle);

                return p;
            }

             v2f vert(Attributes val)
            {
                float3 posW = TransformObjectToWorld(val.positionOS);
                posW= GerstnerWave(posW);

                v2f OUT;
                OUT.positionHCS = TransformWorldToHClip(posW);
                OUT.uv = val.uv;
                return OUT;
            }

            half4 frag( v2f val) : SV_Target
            {
                half4 color = _WaterColor;
                return color;
            }
            ENDHLSL
        }
    }
}
