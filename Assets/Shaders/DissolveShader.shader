Shader "Custom/DissolveShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo (RGB)", 2D) = "white" { }
        _Glossiness ("Smoothness", Range(0, 1)) = 0.5
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalRenderPipeline" }
        LOD 200
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
        float4 _Color;
        float _Glossiness;
        float _Metallic;
        CBUFFER_END
        
        ENDHLSL
        
        struct Attributes
        {
            float4 positionOS: POSITION;
            float2 uv: TEXCOORD0;
            float4 color: COLOR;
        };

        struct Varyings
        {
            float4 positionCS: SV_POSITION;
            float2 uv: TEXCOORD0;
            float4 color: COLOR;
        };

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        Pass
        {
            Name "Mainpass"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            Varyings vert(Attributes IN)
            {
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;

                return OUT;
            }
            
            ENDHLSL
            
        }
    }
    FallBack "Diffuse"
}
