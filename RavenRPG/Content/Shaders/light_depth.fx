float4x4 World;
float4x4 LVP;
float LightClip;
float FarClip;
float3 light_pos;

struct VertexShaderOutput
{
    float4 Position : POSITION;
	float4 depth : TEXCOORD0;
	float4 world_pos : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(float4 position : POSITION)
{

    VertexShaderOutput output;
	output.world_pos = mul(position, World);
    output.Position = mul(position, mul(World, LVP));

	output.depth = (output.Position);
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float d = (input.depth.z / input.depth.w);
    return float4(d,d,d,1);
}

technique Technique1
{
    pass Pass1
    {
#if SM4
		VertexShader = compile vs_4_0_level_9_3 VertexShaderFunction();
		PixelShader = compile ps_4_0_level_9_3 PixelShaderFunction();
#else
        VertexShader = compile vs_3_0 VertexShaderFunction();
		PixelShader = compile ps_3_0 PixelShaderFunction();
#endif
    }
}