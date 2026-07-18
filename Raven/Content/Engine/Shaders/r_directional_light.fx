#include "lib/general.fx"

sampler NORMAL : register(s0) = sampler_state {
	MINFILTER = POINT; MAGFILTER = POINT; MIPFILTER = POINT;	
	ADDRESSU = CLAMP; ADDRESSV = CLAMP;
};
sampler DEPTH : register(s1) = sampler_state {
	MINFILTER = POINT; MAGFILTER = POINT; MIPFILTER = POINT;	
	ADDRESSU = CLAMP; ADDRESSV = CLAMP;
};

struct VSI {
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
};
struct VSO {
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float3 pos : TEXCOORD1;
};

//Pixel Shader Out
struct PSO
{
    float4 Lighting : COLOR0;
};

//Vertex Shader
VSO VS(VSI input)
{
	VSO output = (VSO)0;

	output.Position = input.Position;
	output.pos = input.Position;
	output.TexCoord = input.TexCoord;
		
	return output;
}

float3 light_direction = float3(0,-1,0);
float4 light_color = float4(1,1,1,1);

float4x4 inverse_view;

PSO PS(VSO input)
{
	PSO output = (PSO)0;
	
	float Depth = tex2D(DEPTH,input.TexCoord).r;
	float3 Normal = tex2D(NORMAL,input.TexCoord).rgb;
	
	if (Depth == 1) clip(-1);
	
	float4 decodedNormal = mul(decode(Normal), inverse_view);
	
	float NdotL = dot(light_direction, decodedNormal);
    
	output.Lighting.rgb = ((light_color.rgb) * NdotL);
	
	return output;	
}

technique Default
{
	pass p0
	{
		VertexShader = compile VS_SHADERMODEL VS();
		PixelShader = compile PS_SHADERMODEL PS();
	}
}