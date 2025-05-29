#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


sampler2D DiffuseSampler = sampler_state
{
	texture = <DiffuseLayer>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};
sampler2D LightSampler = sampler_state
{
	texture = <LightLayer>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};
sampler2D DepthSampler = sampler_state
{
    texture = <DepthLayer>;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};
sampler2D NormalSampler = sampler_state
{
    texture = <NormalLayer>;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

struct VSI
{
	float3 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VSO
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

bool draw_2d = true;
float2 offset = float2(0, 0);

VSO MainVS(VSI input)
{
	VSO output = (VSO)0;
    output.Position = float4(input.Position + float3(offset, 0), 1);
	output.UV = input.UV;
	return output;
}

bool fog = true;

int buffer = 1;
float sky_brightness;
float3 atmosphere_color;

bool fullbright = false;

float cubicPulse( float c, float w, float x )
{
    x = abs(x - c);
    if( x>w ) return 0.0;
    x /= w;
    return 1.0 - x*x*(3.0-2.0*x);
}

float4 MainPS(VSO input) : COLOR
{
    float4 rgba = tex2D(DiffuseSampler, input.UV);
    float4 l = tex2D(LightSampler, input.UV);
    float4 n = tex2D(NormalSampler, input.UV);
    float3 d = tex2D(DepthSampler, input.UV).rgb;
	
    if (buffer == -1)    
        return (l * 0.1) + saturate((rgba.rgba * l) * 1.15);
        //return (rgba.rgba * 0.25) + ((rgba.rgba * l) * 1.15);
    else if (buffer == 0 || fullbright)
        return rgba.rgba;
    else if (buffer == 1)
        return n; //normals
    else if (buffer == 2)
        return float4(d.r, d.r, d.r, 1) ; //depth
    else if (buffer == 3)
        return l; //lighting
	else 
        return (l * 0.1) + saturate((rgba.rgba * l) * 1.15);
    
}

technique draw
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};