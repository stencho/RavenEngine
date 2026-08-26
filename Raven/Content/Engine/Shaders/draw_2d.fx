#include "lib/general.fx"
#include "lib/patterns.fx"

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

// SCREEN DRAW VARS
float total_incoming_draws; //if depth fails pull depth from these lol
float current_draw_index;

float2 screen_resolution;
float2 screen_draw_position;
float2 screen_draw_size;

VSO ScreenVS(VSI input) {
    VSO output = (VSO)0;
        
    float2 float_per_screen_pixel = 1.0 / screen_resolution;
    float2 screen_offset = float_per_screen_pixel * screen_draw_position;    
    float2 screen_size = float_per_screen_pixel * screen_draw_size;
    
    output.Position = float4(input.Position + float3(screen_offset, 0), 1);
    output.Position.xy *= screen_size;
    
    output.Pos3d = input.Position;
    
    output.UV = input.UV;
    return output;    
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    return input.Color;
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};