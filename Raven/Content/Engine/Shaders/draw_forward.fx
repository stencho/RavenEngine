#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix world;
matrix view;
matrix projection;

texture depth;
sampler depth_sampler { texture = <depth>; };

texture diffuse;
sampler diffuse_sampler {
    texture = <diffuse>;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;    
};

float opacity = 1.0;
float4 color = float4(1,1,1,1); 

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 UV : TEXCOORD0;
    float4 color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;    
    float4 UV : TEXCOORD0;
    float4 color : COLOR0;
};

VertexShaderOutput BasicVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    matrix wvp = mul(world, mul(view, projection));
    
    output.Position = mul(input.Position, wvp);
    output.color = input.color;
    output.UV = input.UV;

    return output;
}

float4 UnlitPS(VertexShaderOutput input) : COLOR
{
    float4 rgba = tex2D(diffuse_sampler, input.UV);
    return rgba;
}

technique fullbright {
    pass draw {
        VertexShader = compile VS_SHADERMODEL BasicVS();
        PixelShader = compile PS_SHADERMODEL UnlitPS();
    }
};