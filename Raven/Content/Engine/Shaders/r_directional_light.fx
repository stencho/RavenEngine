#include "lib/general.fx"
#include "lib/lighting.fx"

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

//Vertex Shader
VSO VS(VSI input)
{
	VSO output = (VSO)0;

	output.Position = input.Position;
	output.pos = input.Position;
	output.TexCoord = input.TexCoord;
		
	return output;
}

float4x4 inverse_view;

float4 light_color = float4(1,1,1,1);

float3 light_direction = float3(0,-1,0);

bool fullbright = false;

float4 PS(VSO input) : SV_Target0
{
    float4 Lighting = float4(0,0,0,1);
    
    float Depth = tex2D(DEPTH,input.TexCoord).r;
    
    // do nothing to lighting at max depth as that's where the skybox lives
    if (Depth == 1) clip(-1);
        
    float3 Normal = tex2D(NORMAL,input.TexCoord).rgb;
    
	if (fullbright){
		Lighting = float4(1,1,1,1);
		
	} else {	                
        Lighting.rgb = Directional(light_color, Normal, light_direction, inverse_view);
	}	
	
    return Lighting;
}

technique Default
{
	pass p0
	{
		VertexShader = compile VS_SHADERMODEL VS();
		PixelShader = compile PS_SHADERMODEL PS();
	}
}