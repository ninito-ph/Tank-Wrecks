﻿Shader "Custom/GrassShader"
{
    Properties
    {
        [MainTexture] _BaseMap ("Grass Texture", 2D) = "white" { }
        _WindMap ("Wind Distortion Map", 2D) = "white" {}
        _WindFrequency ("Wind Frequency", Vector) = (0.5, 0.5, 0, 0)
        _WindStrenght ("Wind Strenght", Float) = 1
        _Height ("Height", Float) = 1.0
        _MinHeight ("Minimum Height", Float) = 0
        _MaxHeight ("Maxmimum Height", Float) = 1
        _Width ("Width", Float) = 1.0
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (1, 1, 1, 1)
        _LightPower ("Light Intensity", Float) = 0.05
        _Translucency ("Translucency", Float) = 0.02
        _AlphaCutoff ("Alpha Cutoff", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Geometry Pass"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            Cull Off
            
            HLSLPROGRAM
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 4.0

            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _SPECULAR_SETUP
            #pragma shader_feature _RECEIVE_SHADOWS_ON
            //#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS

            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            
            #pragma require geometry

            #pragma geometry geom
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

            #include "GrassPass.hlsl"
            
            ENDHLSL
            
        }
    }
}
