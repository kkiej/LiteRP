#ifndef LITERP_UNLIT_PASS_INCLUDED
#define LITERP_UNLIT_PASS_INCLUDED

struct Attributes
{
    float3 positionOS : POSITION;
    float4 color : COLOR;
#if defined(_FLIPBOOK_BLENDING)
    float4 uv : TEXCOORD0;
    float flipbookBlend : TEXCOORD1;
#else
    float2 uv : TEXCOORD0;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS_SS : SV_POSITION;
#if defined(_VERTEX_COLORS)
    float4 color : VAR_COLOR;
#endif
    float2 uv : VAR_BASE_UV;
#if defined(_FLIPBOOK_BLENDING)
    float3 flipbookUVB : VAR_FLIPBOOK;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings UnlitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    output.positionCS_SS = TransformWorldToHClip(positionWS);

#if defined(_VERTEX_COLORS)
    output.color = input.color;
#endif
    output.uv.xy = TransformBaseUV(input.uv.xy);
#if defined(_FLIPBOOK_BLENDING)
    output.flipbookUVB.xy = TransformBaseUV(input.uv.zw);
    output.flipbookUVB.z = input.flipbookBlend;
#endif
    return output;
}

float4 UnlitPassFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    InputConfig config = GetInputConfig(input.positionCS_SS, input.uv);
    
#if defined(_VERTEX_COLORS)
    config.color = input.color;
#endif
    
#if defined(_FLIPBOOK_BLENDING)
    config.flipbookUVB = input.flipbookUVB;
    config.flipbookBlending = true;
#endif
    
#if defined(_NEAR_FADE)
    config.nearFade = true;
#endif
    
#if defined(_SOFT_PARTICLES)
    config.softParticles = true;
#endif
    
    half4 color = GetBase(config);
    
#if defined(_CLIPPING)
    clip(color.a - GetCutoff(config));
#endif
    
#if defined(_DISTORTION)
    float2 distortion = GetDistortion(config) * color.a;
    color.rgb = lerp(GetBufferColor(config.fragment, distortion).rgb, color.rgb, saturate(color.a - GetDistortionBlend(config)));
#endif
    
    return float4(color.rgb, GetFinalAlpha(color.a));
}
#endif