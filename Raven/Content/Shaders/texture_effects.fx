float3 tint;
float alpha_scissor = 0.0f;
float2 UV_Offset = float2(0,0);
float2 UV_Scale = float2(1,1);

sampler2D TEXTURE : register(s0);

float4 PS(float4 position : SV_Position, float4 color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
	
	float4 rgba = tex2D(TEXTURE, TexCoords * UV_Scale);
	
	if (rgba.a < clamp(alpha_scissor, 0.001, 1)) {
		clip(-1);
	}
	
	return rgba * float4(tint.r, tint.g, tint.b, 1);
}

technique Default
{
	pass p0
	{
		PixelShader = compile ps_3_0 PS();
	}
}