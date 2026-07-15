Shader "Learning/Outline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineColor ("OutlineColor" ,Color ) = (1 , 1,1,1)
        _OutlineWidth ("OutlineWidth" ,Float) = 1.
    }
    SubShader
    {
        Tags
        {
             "RenderType"="TransparentCutout"
             "IgnoreProjector"="True"
        }
        LOD 100

        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert(appdata v) {
                v2f o;

                float3 pos = v.vertex.xyz * (1 + _OutlineWidth * 0.1 );
                o.vertex = UnityObjectToClipPos(float4(pos , 1.0));
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = _OutlineColor;
                return col;
            }
            ENDCG
        }
    }
}
