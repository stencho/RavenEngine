#include "patterns.fx"

#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4 outer_color;
float4 outer_color_secondary;
float4 inner_color;
float4 inner_color_secondary;

float4 outer_border_color;
float4 outer_border_color_secondary;
float4 inner_border_color;
float4 inner_border_color_secondary;

float outer_border_width = 0;
float inner_border_width = 0;

float2 top_left;
float2 bottom_right;

int outer_pattern = 0;
int outer_border_pattern = 0;
int inner_pattern = 0;
int inner_border_pattern = 0;

int outer_dither_res = 0;
int outer_border_dither_res = 0;
int inner_dither_res = 0;
int inner_border_dither_res = 0;

sampler2D sdf_pos_sampler : register(s1)
{
    texture = <sdf_pos>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
sampler2D sdf_neg_sampler : register(s2)
{
    texture = <sdf_neg>;
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
    float2 texCoords : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{    
    float4 positive = tex2D(sdf_pos_sampler, input.texCoords).rgba;
    float4 negative = tex2D(sdf_neg_sampler, input.texCoords).rgba;
    
    float pos = positive.r; 
    float neg = negative.r;    
    
    float2 size = bottom_right - top_left;
    float aspect = 0;
    
    if (size.y < size.x) aspect = size.x / size.y; 
    else aspect = size.y / size.x; 
    
    float2 position = size * input.texCoords;
    
    float4 output = float4(0,0,0,0);
    
    //return positive + negative;
    
    if (negative.a == 0 && positive.a != 0) {
        //outside shape
        if (pos < outer_border_width) {
            output = pattern_select(outer_border_pattern, 
                outer_border_color, outer_border_color_secondary, 
                input.texCoords, top_left, bottom_right, 
                outer_border_dither_res);
        } else {
            output = pattern_select(outer_pattern, 
                outer_color, outer_color_secondary, 
                input.texCoords, top_left, bottom_right, 
                outer_dither_res);
        }        
    } else {
        //inside shape
        if (neg < inner_border_width) {
            output = pattern_select(inner_border_pattern, 
                inner_border_color, inner_border_color_secondary, 
                input.texCoords, top_left, bottom_right, 
                inner_border_dither_res);
        } else {
            output = pattern_select(inner_pattern, 
                inner_color, inner_color_secondary, 
                input.texCoords, top_left, bottom_right, 
                inner_dither_res);
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
