
float3 Directional(float3 color, float3 normal, float3 light_direction, float4x4 inverse_view) {               
        float NdotL = dot(light_direction,  mul((2.0f * normal.xyz - 1.0f), inverse_view));        
        return(color.rgb * NdotL);        
}
