#include "lib/general.fx"

float2 resolution;
float2 position, size;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR {
	float2 xy;

	float2 minimum = position / resolution;
	float2 maximum = (position + size) / resolution;
	float2 diff = maximum - minimum;

	xy.xy = minimum + (input.TextureCoordinates*diff);

	return float4(xy.x, xy.y, 1, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};