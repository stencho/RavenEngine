#include "lib/general.fx"
#include "lib/lighting.fx"

SamplerState DiffuseSampler : register(s0) { 
    texture = <DIFFUSE>; 
};

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

matrix World;
matrix View;
matrix Projection;

float3x3 WVIT;

VSO MainVS(in VSI input) {
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

float4x4 inverse_view;

float4 tint = float4(1,1,1,1);

float3 directional_light_dir;
float3 directional_light_color;

float3 camera_pos;

float2 resolution;

float far_clip = 1000;
float opacity = 1.0;

bool fullbright = false;
bool ignore_depth = false;

float4 MainPS(VSO input) : SV_Target0 {    	
    float4 rgba = tex2D(DiffuseSampler, input.TexCoord);
    
    // apply tint
    rgba *= tint;
    
    // get screen pos for depth clip
	float2 ndc = input.ViewPosition.xy / input.ViewPosition.w;	
	float2 screenUV = ndc.x * 0.5f + 0.5f;	
    screenUV.y = 1.0f - (ndc.y * 0.5f + 0.5f);     
	
	// depth clip (it goes it goes it goes)
	float3 depth = tex2D(DepthSampler, screenUV).xyz;		    
    if (!ignore_depth && (input.Depth.x/input.Depth.y > depth.x / depth.y)) { clip(-1); }
        	
    // build lighting
    float4 lighting = float4(0,0,0,1);
    float3 Normal = encode(normalize(input.TBN[2]));	
    
    // directional lighting
    lighting.rgb = Directional(directional_light_color, Normal, directional_light_dir, inverse_view);
      
    // dim with opacity    
    lighting *= rgba.a * opacity;	
    
    // fullbright mode toggle		
	if (fullbright) { 
        rgba.rgb = saturate(rgba.rgb);	  
           
	} else { // final color + lighting blend
        rgba.rgb = (lighting.rgb * 0.2) + saturate(rgba.rgb * lighting.rgb);
    }
	
    // final opacity blend        
    rgba.a = rgba.a * opacity;
    
    return rgba;
}	



technique render {
	pass P0 {
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
