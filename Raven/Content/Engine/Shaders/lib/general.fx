#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0

float hash21(float2 p)
{
    p = floor(p);

    float3 p3 = frac(float3(p.x, p.y, p.x) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

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


float3 pomn(float3 line_a, float3 line_b, float3 a, float3 p) {
    
	float3 b = a + (normalize(line_a) * distance(line_a, line_b));
	float3 ab = b - a;

	float t = dot(p-a, ab) / dot(ab,ab);

	if (t <= 0) { t = 0; }
	if (t >= 1) { t = 1; }

	return a + t * ab;
}

float3 pomn(float3 position, float3 direction, float clip, float3 p) {
    
	float3 b = position + (normalize(direction) * clip);
	float3 ab = b - position;

	float t = dot(p-position, ab) / dot(ab,ab);

	if (t <= 0) { t = 0; }
	if (t >= 1) { t = 1; }

	return position + t * ab;
}

float distance(float3 A, float3 B ) {
    float3 C = A - B;
    return sqrt(dot( C, C ));    
}
float distance_squared( float3 A, float3 B ) {
    float3 C = A - B;
    return dot( C, C );    
}