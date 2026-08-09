Shader "Learning/TextureCopyShader"
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

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord  ;
                float4 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                return col.r;
            }
            ENDHLSL
        }
    }
}
