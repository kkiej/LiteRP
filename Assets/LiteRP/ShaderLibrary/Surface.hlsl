#ifndef LITERP_SURFACE_INCLUDED
#define LITERP_SURFACE_INCLUDED

struct Surface
{
	float3 position;
	half3 normal;
	float3 interpolatedNormal;
	float3 viewDirection;
	float depth;
	half3 color;
	half alpha;
	half metallic;
	float occlusion;
	half smoothness;
	float fresnelStrength;
	half dither;
};

#endif