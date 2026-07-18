
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0

//float4 tex2d(Texture2D texture, SamplerState sampler, float2 position) {
    //return texture.Sample(sampler, position);
//}
float3 color_lerp(float3 a, float3 b, float position) {
    return float3(a.r - ((a.r - b.r) * position),
                  a.g - ((a.g - b.g) * position),
                  a.b - ((a.b - b.b) * position));
}
float3 slerp(float3 a, float3 b, float position) {
    float fac = smoothstep(0, 1, position);
    return lerp(a, b, fac);
}
float3 slerp(float3 a, float3 b, float position, float edge_a, float edge_b) {
    float fac = smoothstep(edge_a, edge_b, position);
    return lerp(a, b, fac);
}
float slerp(float a, float b, float position, float edge_a, float edge_b) {
    float fac = smoothstep(edge_a, edge_b, position);
    return lerp(a, b, fac);
}
float4 color_lerp(float4 a, float4 b, float position) {
    return float4(a.r + ((b.r - a.r) * position),
                  a.g + ((b.g - a.g) * position),
                  a.b + ((b.b - a.b) * position),
                  a.a + ((b.a - a.a) * position));
}

//sRGB to linear and vice versa. prolly not needed? at all?
float3 stl(float3 input) { return pow(input, 2.2); }
float3 lts(float3 input) { return pow(input, 1.0/2.2); }

//normal encoding/decoding
half3 encode(half3 n) {
    n = normalize(n);
    n.xyz = 0.5f * (n.xyz + 1.0f);
    return n;
}

float3 decode(half3 enc) { return (2.0f * enc.xyz - 1.0f); }