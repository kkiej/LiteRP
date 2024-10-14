#ifndef LITERP_BRDF_INCLUDED
#define LITERP_BRDF_INCLUDED

struct BRDF
{
	half3 diffuse;
	half3 specular;
	half roughness;
	float perceptualRoughness;
	float fresnel;
};

#define MIN_REFLECTIVITY 0.04

half OneMinusReflectivity(half metallic)
{
	half range = 1.0 - MIN_REFLECTIVITY;
	return range - metallic * range;
}

BRDF GetBRDF(inout Surface surface, bool applyAlphaToDiffuse = false)
{
	BRDF brdf;

	half oneMinusReflectivity = OneMinusReflectivity(surface.metallic);
	
	brdf.diffuse = surface.color * oneMinusReflectivity;
	if(applyAlphaToDiffuse)
	{
		brdf.diffuse *= surface.alpha;
	}
	
	brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);

	brdf.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
	brdf.roughness = PerceptualRoughnessToRoughness(brdf.perceptualRoughness);
	brdf.fresnel = saturate(surface.smoothness + 1.0 - oneMinusReflectivity);
	return brdf;
}

half SpecularStrength(Surface surface, BRDF brdf, Light light)
{
	float3 h = SafeNormalize(light.direction + surface.viewDirection);
	half nh2 = Square(saturate(dot(surface.normal, h)));
	half lh2 = Square(saturate(dot(light.direction, h)));
	half r2 = Square(brdf.roughness);
	half d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
	half normalization = brdf.roughness * 4.0 + 2.0;
	return r2 / (d2 * max(0.1, lh2) * normalization);
}

half3 DirectBRDF(Surface surface, BRDF brdf, Light light)
{
	return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

half3 IndirectBRDF(Surface surface, BRDF brdf, float3 diffuse, float3 specular)
{
	float fresnelStrength = surface.fresnelStrength * Pow4(1.0 - saturate(dot(surface.normal, surface.viewDirection)));
	float3 reflection = specular * lerp(brdf.specular, brdf.fresnel, fresnelStrength);
	reflection /= brdf.roughness * brdf.roughness + 1.0;
	return (diffuse * brdf.diffuse + reflection) * surface.occlusion;
}

#endif