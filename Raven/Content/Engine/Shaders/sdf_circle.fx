#include "lib/patterns.fx"

#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

float2 size;
float line_width = 1;
bool fill = false;
int pattern = 0;

float4 color_b = float4(0,0,0,0);

int dither_res = 1;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 UV : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 c = input.Color;
	float2 px = 1/size;

	if (length(input.UV - float2(0.5, 0.5)) > 0.5) {
		clip(-1);
	} 

	if (!fill && length(input.UV - float2(0.5, 0.5)) <= 0.5 - (px.x * line_width)){
		clip(-1);
	} 

	return pattern_select(pattern, c, color_b, input.UV, size, dither_res);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};