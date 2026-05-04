#include "lib/patterns.fx"

#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 inner_color;
float4 inner_color_secondary;

float4 outer_border_color;
float4 inner_border_color;
float4 outer_border_color_secondary;
float4 inner_border_color_secondary;

float outer_border_width = 0;
float inner_border_width = 0;

int inner_pattern = 0;
int outer_border_pattern = 0;
int inner_border_pattern = 1;

int inner_dither_res = 1;
int outer_border_dither_res = 1;
int inner_border_dither_res = 1;

float2 pomn(float2 a, float2 b, float2 p) {
    float2 ab = b - a;
    float t = dot(p - a, ab) / dot(ab, ab);

    if (t < 0) t = 0;
    if (t > 1) t = 1;

    return a + t * ab;
}

float2 points[32];
int point_count;

float2 resolution;
float2 fill_resolution;
float2 bordered_resolution;
float2 fill_position;

sampler2D fill_sampler : register(s1)
{
    texture = <fill_texture>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

float shortest_line_distance(int2 pixel) {
}

float4 MainPS(VertexShaderOutput input) : COLOR {    
    int2 pixel = input.UV.xy * bordered_resolution;
    float2 uv_per_pixel = 1 / bordered_resolution;
    float2 border = float2(32,32);
    float2 fill_uv_top_left = uv_per_pixel * border;
    float2 fill_uv_bottom_right = uv_per_pixel * (border + fill_resolution);
    float2 fill_uv_total = fill_uv_bottom_right - fill_uv_top_left;
    
    bool inside_fill = (pixel.x >= border.x && pixel.x <= border.x + fill_resolution.x && pixel.y >= border.y && pixel.y <= border.y + fill_resolution.y);
    float2 fill_uv = (input.UV.xy - fill_uv_top_left) / fill_uv_total;        
    float inside = tex2D(fill_sampler, fill_uv).a == 1 && inside_fill;
    
    float sdf = 100000;
    float dist = 0;
    
    for (int i = 1; i < point_count; i++) {
        float2 p = pomn(fill_position + points[i-1], fill_position + points[i], pixel);
        float d = distance(pixel, p) / (bordered_resolution / 2);
        if (d < sdf) {
            sdf = d;
            dist = distance(pixel, p);
        }
    }
    
    float2 p = pomn(fill_position + points[point_count-1], fill_position + points[0], pixel);
    float d = distance(pixel, p) / (bordered_resolution / 2);
    if (d < sdf) {
        sdf = d;
        dist = distance(pixel, p);        
    }
        
    float4 output = float4(0,0,0,0);
    
    if (inside) {
        //draw inner        
        output = pattern_select(inner_pattern, inner_color, inner_color_secondary, input.UV.xy, bordered_resolution, inner_dither_res);
                
        //draw inner border
        if (round(dist) < inner_border_width) {
            float4 p = pattern_select(inner_border_pattern, inner_border_color, inner_border_color_secondary, input.UV.xy, bordered_resolution, inner_border_dither_res);
            output = lerp(output, p, p.a);
        }
        
    } else {
        //draw outer border
        if (round(dist) < outer_border_width) {
            output = pattern_select(outer_border_pattern, outer_border_color, outer_border_color_secondary, input.UV.xy, bordered_resolution, outer_border_dither_res);
        } else {
            output = float4(0,0,0,0); 
        }
    }
    
    return output;        
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};
