#include "lib/patterns.fx"

#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 color_a = float4(1,1,1,1);
float4 color_b = float4(0.5,0.5,0.5,1);

float2 top_left; float2 bottom_right;

int pattern_size = 1;

float4x4 world; 

bool clip_b = false;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 texCoord : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return pattern_select(1, color_a, color_b, input.texCoord.xy, bottom_right - top_left, pattern_size);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};