#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


sampler2D cubeS = sampler_state
{
    texture = <cubemap>;
    magfilter = ANISOTROPIC;
    minfilter = ANISOTROPIC;
    mipfilter = ANISOTROPIC;
    MaxAnisotropy = 16;
    ADDRESSU = MIRROR;
    ADDRESSV = MIRROR;
};
sampler2D cubeSE = sampler_state
{
    texture = <cubemap_emissive>;
    magfilter = ANISOTROPIC;
    minfilter = ANISOTROPIC;
    mipfilter = ANISOTROPIC;
    MaxAnisotropy = 16;
    ADDRESSU = MIRROR;
    ADDRESSV = MIRROR;
};

struct VSI
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
};

struct VSO
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 Pos3d : TEXCOORD1;
};

struct PSO
{
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Depth : COLOR2;
    float4 Lighting : COLOR3;
};

bool draw_2d = true;
float2 offset = float2(0, 0);

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 cam_dir;

VSO MainVS(VSI input)
{
    VSO output = (VSO) 0;
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.Pos3d = input.Position.xyz;
    output.UV = input.UV;

    return output;
}

float3 color_lerp(float3 a, float3 b, float position) {
    return float3(
                a.r - ((a.r - b.r) * position),
                a.g - ((a.g - b.g) * position),
                a.b - ((a.b - b.b) * position));
}
float4 color_lerp(float4 a, float4 b, float position)
{
    return float4(
                a.r + ((b.r - a.r) * position),
                a.g + ((b.g - a.g) * position),
                a.b + ((b.b - a.b) * position),
                a.a + ((b.a - a.a) * position));
}

bool fog = false;

float4 sky_color;
float4 atmosphere_color;

PSO clear_sky(VSO input)
{
    PSO output = (PSO) 0;
        
    float4 rgba = tex2D(cubeS, input.UV);

    float4 rgba_final = sky_color;
    
    //draw fade from atmospheric albedo (is this the right term lmaomo) up to full on sky colour
    rgba_final.rgb = color_lerp(atmosphere_color.rgb, sky_color.rgb, clamp(input.Pos3d.y*0.3, 0.0, 1));
    output.Lighting.rgb = 0;
    output.Lighting.a = 1;
    output.Normals = 0;
    output.Diffuse = rgba_final;
    output.Depth.rgb = 1;
    output.Depth.a = 1;
    return output;
};

technique draw
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL clear_sky();
    }
};