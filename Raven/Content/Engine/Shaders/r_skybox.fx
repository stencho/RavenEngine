#include "lib/general.fx"

static const float PI = acos(-1.0);

sampler2D SkyboxLerpSampler = sampler_state { texture = <SkyboxLerp>; };

struct VSI {
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};
struct VSO {
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
    float4 Pos3d : TEXCOORD1;
    float4 view_pos : TEXCOORD2;
};
struct SkyboxPSO {
    float4 Diffuse : COLOR0;
    float4 Lighting : COLOR1;
};

float4 atmosphere_color;
float4 sky_color;
float4 max_sky_darkness;

float3 camera_pos;

float skybox_height;
float day_position;

float4x4 skybox_world;
float4x4 skybox_view;
float4x4 skybox_projection;

float4x4 inverse_view;

VSO SkyboxVS(VSI input) {
    VSO output = (VSO) 0;    
    float4x4 wvp = mul(skybox_world, mul(skybox_view, skybox_projection));
    
    float3 scaled_pos = mul(input.Position, wvp).xyz;
    scaled_pos *= 1.0 / skybox_height;
     
    output.Position = mul(input.Position, wvp);
    output.view_pos = output.Position;
    output.Pos3d = input.Position;
    output.UV = input.UV;
    
    return output;
}

float3 self_multi(float3 col, float m) {
    return saturate(col * m);
}

float3 Hash33(float3 p)
{
    p = frac(p * float3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yzx + 33.33);

    return frac((p.xxy + p.yzz) * p.zyx);
}

SkyboxPSO Skybox(VSO input) {
    SkyboxPSO output = (SkyboxPSO)0;
        
    float open_sky = 0.3;    
    float horizon_point = 0.55;
    float dark_point = 1;
              
              float scale = 200.0;      
              
              
    float3 pos = normalize(input.Pos3d);
    
    float y = pos.y;
    y = (y + (skybox_height / 2)) / (skybox_height);
        
    float y_all_pos = y; 
    y = (y * 2) - 1;    
        
                
    float3 atmos = (atmosphere_color.rgb);
    float3 dark = (max_sky_darkness.rgb);
        
    float4 rgba = float4(1,1,1,1); // = tex2D(cubeS, input.UV);
        
    float midday_dist = distance(day_position, 0.5) * 2;
                
    // DRAW STARS       
    
    float3 grid = pos * 400.0;
    
    float3 cell = floor(grid);
    float3 local = frac(grid);

    float3 rnd = Hash33(cell);

    // Only some cells contain stars
    float starChance = step(0.98, rnd.z);

    // Random star location inside cell
    float3 starPos = rnd;

    float dist = length(local - starPos);

    float brightness = lerp(0.1, 3.0, pow(rnd.y, 12.0) );
    // Random star size
    float radius = lerp(0, 4, pow(rnd.x, 8.0) );
    radius = step(1, radius);
    // Anti-aliased edge
    float aa = fwidth(dist) * 2;
    float star = 1.0 - smoothstep(radius - aa, radius + aa, dist);

    // Brightness distribution

    star = star * starChance * step(0.4, brightness);
        
    float distance_from_tip = 1 - y_all_pos;
      
    rgba.rgb = slerp(sky_color, atmos * (0.5 + (midday_dist * 0.5)), distance_from_tip, open_sky, 1);
    //rgba.rgb = slerp(atmos, dark, distance_from_tip, horizon_point, dark_point);   
     
    output.Lighting.rgb = saturate(rgba.rgb + (star * clamp(midday_dist - 0.6, 0, 1)));
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