// Every shader must have a name. You can specify a directory in the shader
// selector by typing Directory/Name, replacing Directory by the directory's
// name and the Name by the shader's name
Shader "Custom/Unlit"
{
    // A shader may have a Property Block. Those are essentially the serialized
    // variables of the shader. They can be edited in the editor to alter the
    // shader's looks
    Properties
    {
        // It is common practice to use underlines before shader variable names
        _MainTex ("Albedo (RGB)", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        //_Glossiness ("Smoothness", Range(0, 1)) = 0.5
        //_Metallic ("Metallic", Range(0, 1)) = 0.0
    }

    // Every shader has a subshader. A shader may have more than one subshader.
    // Unity will pick and use the first platform-compatible subshader it finds
    // and render the material using it
    SubShader
    {
        // Subshaders may have a number of tags. Consult Unity documentation to
        // see all available tags and their effects. The tags used below
        // indicate, respectively:
        // That the rendered material is opaque
        // That the render queue is set to geometry. Check the Unity docs to understand how the render queue affects rendering
        // That the current shader is URP-compatible. If Unity is using the SRP or the HDRP, it will skip
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalRenderPipeline" }
        
        // This is the HLSLINCLUDE block. This will make the shader include
        // certain code during compilation. Think of it as a different using
        // directive
        HLSLINCLUDE

        // This is the core HLSL functions file for Unity. Most of the time, you
        // will include this file in a shader
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        
        // This is the CBUFFER. It affects how unity's scriptable rendering
        // pipeline makes its batches. You should include all serialized
        // material properties here, barring textures. You may choose to ignore
        // this, but it will cause considerable performance loss. Take a look at
        // the SRP Batcher more in depth over here:
        // https://blogs.unity3d.com/2019/02/28/srp-batcher-speed-up-your-rendering/
        CBUFFER_START(UnityPerMaterial)
        // Appending ST to a texture variable name is how unity handles tiling
        // and offset for a texture, so it should be present for every texture
        // property in the shader
        float4 _MainTex_ST;
        float4 _Color;
        //float _Glossiness;
        //float _Metallic;
        CBUFFER_END 

        ENDHLSL

        // Every subshader must have one or more passes. They indicate how the
        // material is actually rendered     
        Pass
        {
            // The name of the pass
            Name "Main"
            // The tag of the pass. Note that a pass should always have a tag,
            // or it will break the SRP batcher. Furthermore, if you tag
            // multiple passes with the same tag, only the first pass with the
            // given tag will be used to render. Consult the documentation to
            // see the tags and their effects. Universal forward is used to
            // render the objects using the forward renderer in the URP.
            Tags { "LightMode" = "UniversalForward" }
            
            // This marks the beggining of the HLSL program. This is where the
            // actual shader code lies.
            HLSLPROGRAM
            
            // Every shader in the SRP must have a fragment and vertex function.
            // It is common practice to name those frag and vert, respectively
            #pragma vertex vert
            #pragma fragment frag
            
            // The attributes struct is the input to the vertex function. It
            // gives us access to the per-vertex data. Variables are followed by
            // semicolons and a keyword. These keywords are known as semantics.
            struct Attributes
            {
                // Vertex position
                float4 positionOS: POSITION;
                // Vertex UV texture coordinates
                float2 uv: TEXCOORD0;
                // Vertex color
                float4 color: COLOR;
            };

            // Varyings is the output of the vertex shader and input of the
            // fragment shader.
            struct Varyings
            {
                // The position in clip space
                float4 positionCS: SV_POSITION;
                float2 uv: TEXCOORD0;
                float4 color: COLOR;
            };
            
            // These are the material properties in the HLSL code. We had
            // previously declared them to Unity, but not the shader code. Here,
            // we do exactly that. We use a TEXTURE2D to receive our texture and
            // a sampler to sample said texture. Note that we gave them the
            // exact same name we had declared them under in properties. This is
            // intentional. The name in properties and here MUST be the same.
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            // Here is our vertex function. It returns our Varyings struct
            Varyings vert(Attributes IN)
            {
                // We declare a Varyings to return, named OUT
                Varyings OUT;
                // We use transform object space to clip space function from ShaderVariableFunctions.hlsl
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                // We use TRANSFORM_TEX to generate our UV for the fragment function
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                // We give the color as our input color
                OUT.color = IN.color;
                // We return our varyings OUT
                return OUT;
            }

            // Here is our fragment function. It decides the color of each pixel
            // (fragment) in the image. SV_TARGET tells Unity that we are using
            // half4 for color outputs
            half4 frag(Varyings IN): SV_Target
            {
                // Here we sample the texture and get a pixel color
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                // We calculate the final pixel color by multiplying the main
                // texture sample by the color by the vertex color
                return mainTex * _Color * IN.color;
            }
            ENDHLSL
            
        }
    }
}
