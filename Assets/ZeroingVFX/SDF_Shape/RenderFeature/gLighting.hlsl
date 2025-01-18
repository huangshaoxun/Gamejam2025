#ifndef gLighting
#define gLighting


half3 LightingLambert2( Light light, half3 normal){
    half3 attenuatedLightColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
    half NdotL= saturate(dot(normal, light.direction));
    NdotL = max(0.33,NdotL);
    return NdotL * attenuatedLightColor;
}
half3 LightingSpecular2( Light light, half3 normal, half3 viewDir, half4 specular,half smoothness)
{
    half3 attenuatedLightColor = light.color * light.distanceAttenuation * light.shadowAttenuation;
    // specular.a = exp2(10 * specular.a + 1);
    smoothness = exp2(10 * smoothness + 1);
    float3 halfVec = SafeNormalize(float3(light.direction) + float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = pow(NdotH, smoothness);
    modifier *= saturate(dot(normal, light.direction)); //暗部不显示高光
    half3 specularReflection = specular.rgb * modifier;
    return  specularReflection * attenuatedLightColor;
}



#endif