// Vertex attributes struct
struct Attributes
{
    // OS stands for object space
    // LM stands for lightmap
    float4 positionOS: POSITION;
    float3 normalOS: NORMAL;
    float4 tangentOS: TANGENT;
    float2 uv: TEXCOORD0;
    float2 uvLM: TEXCOORD1;
    float4 color: COLOR;
    
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// The fragment varyings struct
struct Varyings
{
    // WS stands for world space
    float3 normalOS: NORMAL;
    float2 uv: TEXCOORD0;
    float2 uvLM: TEXCOORD1;
    float4 positionWSAndFogFactor: TEXCOORD2;
    half3 normalWS: TEXCOORD3;
    half3 tangentWS: TEXCOORD4;
    float4 positionOS: TEXCOORD5;
    
    float4 color: COLOR;
    #if _NORMALMAP
        half3 bitangentsWS: TEXCOORD5;
    #endif
    
    #ifdef _MAIN_LIGHT_SHADOWS
        float4 shadowCoord: TEXCOORD6;
    #endif
    float4 positionCS: SV_POSITION;
};

// Properties
float _Height;
float _MinHeight;
float _MaxHeight;
float _Width;
float4 _Color;
float4 _ShadowColor;
float _LightPower;
float _Translucency;
float _AlphaCutoff;
sampler2D _WindMap;
float3 _WindFrequency;
float _WindStrenght;

// Vertex pass
Varyings vert(Attributes IN)
{
    Varyings OUT;
    
    OUT.color = IN.color;
    
    VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS);
    VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
    
    float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    
    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
    OUT.uvLM = IN.uvLM.xy * unity_LightmapST.xy + unity_LightmapST.zw;
    
    OUT.positionWSAndFogFactor = float4(vertexInput.positionWS, fogFactor);
    OUT.positionCS = vertexInput.positionCS;
    OUT.positionOS = IN.positionOS;
    
    OUT.normalWS = vertexNormalInput.normalWS;
    OUT.tangentWS = vertexNormalInput.tangentWS;
    
    #ifdef _NORMALMAP
        OUT.bitangentsWS = vertexNormalInput.bitangentsWS;
    #endif
    
    #ifdef _MAIN_LIGHT_SHADOWS
        OUT.shadowCoord = GetShadowCoord(vertexInput);
    #endif
    
    return OUT;
}

// Rotates on the Y angle with a Matrix
float3x3 YRotationMatrix(float angle)
{
    return float3x3
    (
        cos(angle), 0, sin(angle),
        0, 1, 0,
        - sin(angle), 0, cos(angle)
    );
}

// Rotates on the X angle with a Matrix
float3x3 XRotationMatrix(float angle)
{
    return float3x3
    (
        1, 0, 0,
        0, cos(angle), -sin(angle),
        0, sin(angle), cos(angle)
    );
}

// Rotates on the Z angle with a Matrix
float3x3 ZRotationMatrix(float angle)
{
    return float3x3
    (
        cos(angle), -sin(angle), 0,
        sin(angle), cos(angle), 0,
        0, 0, 1
    );
}

// FIXME: This function is slow. consider using a pcg generator instead
// Generates a random number given a seed
float Random(float3 seed)
{
    return frac(sin(dot(seed.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
}

// Geometry pass
// Defines the maximum number of vertices to be dealt with in the
// function
[maxvertexcount(6)]
// Receives a triangle of Varyings (vertices) and returns a triangle stream
void geom(triangle Varyings IN[3], inout TriangleStream < Varyings > outStream)
{   
    // Calculates wind distortion
    float2 uv = (IN[0].positionOS.xy * _Time.xy * _WindFrequency);
    float4 windSample = tex2Dlod(_WindMap, float4(uv, 0, 0)) * _WindStrenght;
    float3 rotatedNormalZ = mul(IN[0].normalWS, ZRotationMatrix(windSample.x));
    float3 rotatedNormal = mul(rotatedNormalZ, XRotationMatrix(windSample.y));

    // Randomizes grass height
    float randomHeight = Random(IN[0].positionWSAndFogFactor.xyz);

    // Calculates the base position of the grass quad
    float3 basePos = (IN[0].positionWSAndFogFactor.xyz + IN[1].positionWSAndFogFactor.xyz + IN[2].positionWSAndFogFactor.xyz) / 3;
    
    // Gets the first vertex in the triangle
    Varyings OUT0 = IN[0];
    
    // Rotates the tangent and stores it for later use
    float3 rotatedTangent = normalize(mul(OUT0.tangentWS, YRotationMatrix(Random(OUT0.positionWSAndFogFactor.xyz) * 90)));
    
    // Calculates the positions of the four vertices composing the quad.
    // Multiplies the normal of the quad by the _Height property to place the
    // upper vertexes, and the _Width property to calculate lateral distance of
    // the quads
    
    // Left vertex
    float3 OUT0pos = (basePos - rotatedTangent * _Width);
    OUT0.positionCS = TransformWorldToHClip(OUT0pos);
    // Right vertex
    Varyings OUT1 = IN[0];
    float3 OUT1pos = (basePos + rotatedTangent * _Width);
    OUT1.positionCS = TransformWorldToHClip(OUT1pos);
    // Top right vertex
    Varyings OUT2 = IN[0];
    float3 OUT2pos = (basePos + rotatedTangent * _Width + rotatedNormal * clamp(_Height * randomHeight, _MinHeight, _MaxHeight));
    OUT2.positionCS = TransformWorldToHClip(OUT2pos);
    // Top left vertex
    Varyings OUT3 = IN[0];
    float3 OUT3pos = (basePos - rotatedTangent * _Width + rotatedNormal * clamp(_Height * randomHeight, _MinHeight, _MaxHeight));
    OUT3.positionCS = TransformWorldToHClip(OUT3pos);
    
    // Calculates the normal of the plane for lighting
    float3 newNormal = mul(rotatedTangent, YRotationMatrix(PI / 2));
    
    // Defines the UV for the quad
    OUT3.uv = TRANSFORM_TEX(float2(0, 1), _BaseMap);
    OUT2.uv = TRANSFORM_TEX(float2(1, 1), _BaseMap);
    OUT1.uv = TRANSFORM_TEX(float2(1, 0), _BaseMap);
    OUT0.uv = TRANSFORM_TEX(float2(0, 0), _BaseMap);
    
    // Defines the normal for the quad
    OUT3.normalWS = newNormal;
    OUT2.normalWS = newNormal;
    OUT1.normalWS = newNormal;
    OUT0.normalWS = newNormal;
    
    // Creates the two triangle strips composing the quad
    outStream.Append(OUT3);
    outStream.Append(OUT2);
    outStream.Append(OUT0);
    // Begins new strip
    outStream.RestartStrip();
    
    outStream.Append(OUT2);
    outStream.Append(OUT1);
    outStream.Append(OUT0);
}

// Transforms world coordinates into shadow coordinates through the cascade index
float4 TransformWorldToShadowCoords(float3 positionWS)
{
    half cascadeIndex = ComputeCascadeIndex(positionWS);
    return mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));
}

// Fragment pass
// Receives a varying (vertex) and checks if the normal is facing the camera
half4 frag(Varyings IN, bool isFacingCamera: SV_IsFrontFace): SV_Target
{
    // Retrieves and normalizes the quad normal
    half3 normalWS = IN.normalWS;
    normalWS = normalize(normalWS);
    
    // FIXME: Another performance loss due to branching/wavefront breaking
    normalWS = isFacingCamera ? normalWS: - normalWS;
    
    // Gets the position in world space
    float3 positionWS = IN.positionWSAndFogFactor.xyz;
    
    // Gets the shadow coordinates of the quad
    float4 shadowCoord = TransformWorldToShadowCoords(positionWS);
    
    // Gets the main light through the shadow coordinates
    Light mainLight;
    mainLight = GetMainLight(shadowCoord);
    
    // Calculates the final color
    half3 color = (0, 0, 0);
    float3 normalLight = LightingLambert(mainLight.color, mainLight.direction, normalWS) * _LightPower;
    float3 inverseNormalLight = LightingLambert(mainLight.color, mainLight.direction, -normalWS) * _Translucency;
    color = _Color + normalLight + inverseNormalLight;
    
    // Fakes self-shadowing by lerping between a lighter and darker color at the
    // top and bottom, respectively
    color = lerp(color, _ShadowColor, 1 - IN.uv.y);
    // Mixes with fog
    color = MixFog(color, IN.positionWSAndFogFactor.w);
    
    // Gets texture alpha and clips alpha
    float alpha = _BaseMap.Sample(sampler_BaseMap, IN.uv).a;
    clip(alpha - _AlphaCutoff);
    
    return half4(color, 1);
}