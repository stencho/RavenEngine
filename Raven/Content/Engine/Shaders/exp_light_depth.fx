float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightPosition;
float LightClip;
float3 LightDirection;
float C;

texture DiffuseMap;
sampler DiffuseSampler = sampler_state
{
	texture = <DiffuseMap>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

struct VSI {
	float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VSO {
	float4 Position : POSITION0;
	float4 WorldPosition : TEXCOORD1;
	float4 ViewPosition : TEXCOORD2;
    float2 TexCoord : TEXCOORD3;
};

VSO VS(VSI input) {
	VSO output;
	float4 worldPosition = mul(input.Position, World);
	
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	//output.Position.z = log((output.Position.w * 0.01) + 1)/log((LightClip*0.01) + 1);
	//output.Position.z *= output.Position.w; 
	output.WorldPosition = worldPosition;
	output.ViewPosition = output.Position;
	output.TexCoord = input.TexCoord;
	return output;
}
float distSquared( float3 A, float3 B )
{

    float3 C = A - B;
    return dot( C, C );

}

float3 pomn(float3 a, float3 p) {
	float3 b = a + (LightDirection * LightClip);
	float3 ab = b - a;

	float t = dot(p-a, ab) / dot(ab,ab);

	if (t <= 0) { t = 0; }
	if (t >= 1) { t = 1; }

	return a + t * ab;
}


float4 PS(VSO input) : COLOR0 {
	if (tex2D(DiffuseSampler, input.TexCoord).a < 1) {clip(-1);}
	
	input.WorldPosition /= input.WorldPosition.w;
	//float depth = distance(input.WorldPosition.xyz, LightPosition.xyz) / LightClip;
	
	float3 linpos = pomn(LightPosition, input.WorldPosition.xyz);
	float depth = (distance(LightPosition.xyz, linpos) / LightClip);

	//float depth = (input.ViewPosition.z / (LightClip));
	return (log(C * (distance(LightPosition.xyz, linpos)) + 1) / log(C * LightClip + 1));
}

technique Default {
	pass p0 {
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}