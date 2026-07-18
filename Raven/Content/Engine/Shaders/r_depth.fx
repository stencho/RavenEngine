#include "lib/general.fx"

float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput {
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;
};

struct VertexShaderOutput {
    float4 Position : POSITION;    
    float2 TexCoord : TEXCOORD0;
    float4 Depth : TEXCOORD1;
	float3 WorldPos : TEXCOORD2;
	float4 ViewPosition : TEXCOORD6;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input) {    
	VertexShaderOutput output = (VertexShaderOutput)0;
	
	float4x4 wvp = mul(World, mul(View, Projection));
		
	output.Position = mul(input.Position, wvp);
    output.TexCoord = input.TexCoord;
		    
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = mul(mul(input.Position, World),View).z;
	
	output.ViewPosition = output.Position;

	output.WorldPos = input.Position.xyz;

	return output;
}

float4 PixelShaderFunction(in VertexShaderOutput input) : SV_Target0 {
    return float4(input.Depth.x,input.Depth.y,input.Depth.z,1);
}

technique z_prepass {
    pass Pass {
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}