#include "lib/general.fx"

sampler2D SkyboxLerpSampler = sampler_state { texture = <SkyboxLerp>; };

struct VSI {
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};
struct VSO {
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
    float4 Pos3d : TEXCOORD1;
};
struct SkyboxPSO {
    float4 Diffuse : COLOR0;
    float4 Lighting : COLOR1;
};

float4 atmosphere_color;
float4 max_sky_darkness;

float skybox_height;
float day_position;

float4x4 skybox_world;
float4x4 skybox_view;
float4x4 skybox_projection;

VSO SkyboxVS(VSI input) {
    VSO output = (VSO) 0;    
    float4x4 wvp = mul(skybox_world, mul(skybox_view, skybox_projection));
    
    float3 scaled_pos = mul(input.Position, wvp).xyz;
    scaled_pos *= 1.0 / skybox_height;
     
    output.Position = mul(input.Position, wvp);
    output.Pos3d = input.Position;
    output.UV = input.UV;
    
    return output;
}

SkyboxPSO Skybox(VSO input) {
    SkyboxPSO output = (SkyboxPSO)0;
        
    float open_sky = 0.25;    
    float horizon_point = 0.55;
    float dark_point = 1;
                
    float y = input.Pos3d.y;
    y = (y + (skybox_height / 2)) / (skybox_height);
    float y_all_pos = y;
    y = (y * 2) - 1;
    
    float x = input.Pos3d.x;
    x = (x + (skybox_height / 2)) / (skybox_height);
                
    float3 atmos = (atmosphere_color.rgb * 0.8);
    float3 dark = (max_sky_darkness.rgb);
        
    float4 rgba = float4(1,1,1,1); // = tex2D(cubeS, input.UV);
    
    float atmos_glow = slerp(0, 1, x, 0, 0.5);
    atmos_glow = slerp(0, 1, x, 0.5, 1);
    
    float sky_glow = day_position;
    if (sky_glow > 0.5) {
        sky_glow = 0.5 - (sky_glow-0.5);
    }
    sky_glow *= 2;
    
    float atmos_glow_vertical_offset = 0;
    
    // sky color
    float distance_from_tip = 1 - y_all_pos;
      
    atmos = slerp(float3(0.3,0.2,0.7) * sky_glow, atmos * atmos_glow, distance_from_tip, open_sky, 1);
    rgba.rgb = slerp(atmos, dark, distance_from_tip, horizon_point, dark_point);   
     
    output.Lighting.rgb = (rgba.rgb);
    output.Lighting.a = 1;
    
    output.Diffuse.rgb = 1;
    output.Diffuse.a = 1;
    
    return output;
}

technique skybox {
	pass P0	{
		VertexShader = compile VS_SHADERMODEL SkyboxVS();
		PixelShader = compile PS_SHADERMODEL Skybox();
	}
}