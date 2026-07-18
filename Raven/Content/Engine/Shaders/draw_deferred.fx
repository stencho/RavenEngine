#include "lib/general.fx"

matrix World;
matrix View;
matrix Projection;

float3x3 WVIT;

float far_clip = 1000;

float4 tint = float4(1,1,1,1);

SamplerState DIFFUSE : register(s0);

//SamplerState NORMAL : register(s1);
//SamplerState SPECULAR : register(s2);
//SamplerState EMISSIVE : register(s3);

Texture2D DEPTH;
SamplerState DepthSampler : register(s6) = sampler_state {
    texture = <DEPTH>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
}; 

struct VSI {
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;
};

struct VSO {
    float4 Position : POSITION;    
    float2 TexCoord : TEXCOORD0;
    float4 Depth : TEXCOORD1;
	float3 WorldPos : TEXCOORD2;
    float3x3 TBN : TEXCOORD3;
	float4 ViewPosition : TEXCOORD6;
};

struct PSO {
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Lighting : COLOR2;
};

float3 ambient_light;

VSO MainVS(in VSI input)
{
	VSO output = (VSO)0;
	
	float4x4 wvp = mul(World, mul(View, Projection));
		
	output.Position = mul(input.Position, wvp);
    output.TexCoord = input.TexCoord;
		
    //output.Depth = 1-((output.Position.z / far_clip));

	//output.Depth = output.Position;
    
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = mul(mul(input.Position, World),View).z;
	        
	output.ViewPosition = output.Position;
    //output.ViewPosition.x = (output.Position.x / output.Position.w) * 0.5f + 0.5f;
    //output.ViewPosition.y = 1.0f - ((output.Position.y / output.Position.w) * 0.5f + 0.5f);
    
	output.WorldPos = input.Position.xyz;

	output.TBN[0] = normalize(mul(input.Tangent, (float3x3)WVIT));
	output.TBN[1] = normalize(mul(input.BiTangent, (float3x3)WVIT));
	output.TBN[2] = normalize(mul(input.Normal, (float3x3)WVIT));

	return output;
}

float PCF(float depth, float NdotL, float2 shadowmap_UV) {	
	return 0.5f;
}

float3 camera_pos;

float3 atmosphere_color;
float3 sky_color;

bool fog = false;

bool fullbright = false;

float2 resolution;

PSO MainPS(VSO input) {
    PSO output = (PSO)0;

    float4 rgba = tex2D(DIFFUSE, input.TexCoord);    
    //rgba.rgb = pow(rgba.rgb, 2.2);
    		
	float2 ndc = input.ViewPosition.xy / input.ViewPosition.w;
	
	float2 screenUV = ndc.x * 0.5f + 0.5f;
	screenUV.y = 1.0f - (ndc.y * 0.5f + 0.5f); 
    	
	float3 depth = tex2D(DepthSampler, screenUV).xyz;
			    
    // depth clip
    if (input.Depth.x/input.Depth.y > depth.x / depth.y) {
        clip(-1);
    }
    
    output.Normals.rgb = encode(normalize(input.TBN[2]));
	output.Normals.a = 1;
	
	output.Lighting = float4(1,1,1,1);	
	
	float d = 1;
	float dist = (distance(camera_pos, input.WorldPos)) / (far_clip);
	float fog_start = 0.85;
	float fog_end = 1;	
	
	output.Lighting = float4(0,0,0,1);		
	
	float4 tint_pow = tint;
	//tint_pow.rgb = pow(tint_pow.rgb, 2.2);
	
    output.Diffuse.rgb = rgba.rgb * tint_pow.rgb;
    
    //output.Diffuse.rgb = pow(output.Diffuse.rgb, 1.0/2.2);
	output.Diffuse.a = 1; 
	
	return output;
}	

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
	