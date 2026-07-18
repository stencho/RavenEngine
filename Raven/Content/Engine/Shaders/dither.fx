#include "lib/general.fx"
#include "lib/patterns.fx"

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

float4 MainPS(VertexShaderOutput input) : COLOR0 {
    return pattern_select(1, color_a, color_b, input.texCoord.xy, bottom_right - top_left, pattern_size);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};