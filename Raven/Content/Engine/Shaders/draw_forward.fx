#include "lib/general.fx"

sampler DiffuseSampler = sampler_state {
	texture = <texture_map>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
sampler DepthSampler = sampler_state {
	texture = <depth_map>;
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


float3 ambient_light;

matrix World;
matrix View;
matrix Projection;

float3x3 WVIT;

float far_clip = 1000;
float near_clip = 1000;

float opacity = 1.0;

float4 tint = float4(1,1,1,1);

VSO MainVS(in VSI input)
{
	VSO output = (VSO)0;
	
	float4x4 wvp = mul(World, mul(View, Projection));
		
	output.Position = mul(input.Position, wvp);
	output.ViewPosition = output.Position;
	
    output.TexCoord = input.TexCoord;
		    
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = mul(mul(input.Position, World),View).z;	
        	    
	output.WorldPos = input.Position.xyz;

	output.TBN[0] = normalize(mul(input.Tangent, (float3x3)WVIT));
	output.TBN[1] = normalize(mul(input.BiTangent, (float3x3)WVIT));
	output.TBN[2] = normalize(mul(input.Normal, (float3x3)WVIT));

	return output;
}


float3 camera_pos;

bool fog = false;
bool fullbright = false;

float2 resolution;

float4x4 inverse_view;

float3 atmosphere_color;
float atmosphere_intensity;

float3 directional_light;
float3 light_color;
float3 light_intensity;

float4 MainPS(VSO input) : SV_Target0 {    	
    float4 rgba = tex2D(DiffuseSampler, input.TexCoord);
    
    //get screen pos for depth clip
	float2 ndc = input.ViewPosition.xy / input.ViewPosition.w;	
	float2 screenUV = ndc.x * 0.5f + 0.5f;	
    screenUV.y = 1.0f - (ndc.y * 0.5f + 0.5f);     
	
	// DEPTH/OCCLUSION CLIP
	float3 depth = tex2D(DepthSampler, screenUV).xyz;		    
    if (input.Depth.x/input.Depth.y > depth.x / depth.y) { clip(-1); }
    
    //set up normals
    float3 Normals = encode(normalize(input.TBN[2]));	
    float4 decoded_normal = mul(decode(Normals), inverse_view);
    float n_dot_l = dot(directional_light, decoded_normal);
    	
    float4 lighting = float4(0,0,0,1);
    lighting.rgb = ((light_color.rgb) * n_dot_l);    
    lighting *= rgba.a * opacity;	
    
	float d = 1;
	float fog_start = 0.85;
	float fog_end = 1;
			
	if (fullbright){
		lighting.rgb = float3(1,1,1);
	} 
			
    // FAR CLIP    
	float dist = (distance(camera_pos, input.WorldPos)) / (far_clip);
	if (dist >= 0.999) clip(-1);	
	    
	float4 tint_pow = tint;
	//tint_pow.rgb = pow(tint.rgb, 2.2);
	float4 Diffuse = rgba * tint_pow;
	
    Diffuse.rgb = (lighting.rgb * 0.2) + saturate(Diffuse.rgb * lighting.rgb);
    
    //Diffuse.rgb = pow(Diffuse.rgb, 1.0/2.2);
    Diffuse.a = rgba.a * opacity;
    
    return Diffuse;
}
	

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
	