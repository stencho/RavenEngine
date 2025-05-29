#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;

float3x3 WVIT;

float FarClip = 1000;
float NearClip;
float LightBias;

float3 tint = float3(1,1,1);

sampler DIFFUSE : register(s0);
sampler NORMAL : register(s1);
sampler DEPTH : register(s2);
sampler LIGHTING : register(s3);

texture DiffuseMap;
sampler DiffuseSampler = sampler_state
{
	texture = <DiffuseMap>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;    
    float2 TexCoord : TEXCOORD0;
    float4 Depth : TEXCOORD1;
	float3 WorldPos : TEXCOORD2;
    float3x3 TBN : TEXCOORD3;
	float4 ViewPosition : TEXCOORD6;
};
struct PSO
{
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Depth : COLOR2;
    float4 Lighting : COLOR3;
};

half3 encode(half3 n)
{
    n = normalize(n);
    n.xyz = 0.5f * (n.xyz + 1.0f);
    return n;
}
float3 decode(half3 enc)
{
    return (2.0f * enc.xyz - 1.0f);
}

float3 color_lerp(float3 a, float3 b, float position) {
    return float3(
                a.r + ((a.r - b.r) * position),
                a.g + ((a.g - b.g) * position),
                a.b + ((a.b - b.b) * position));
}
float4 color_lerp(float4 a, float4 b, float position)
{
    return float4(
                a.r + ((b.r - a.r) * position),
                a.g + ((b.g - a.g) * position),
                a.b + ((b.b - a.b) * position),
                a.a + ((b.a - a.a) * position));
}

float3 ambient_light;

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	
	float4x4 wvp = mul(World, mul(View, Projection));
		
	output.Position = mul(input.Position, wvp);
    output.TexCoord = input.TexCoord;
		
    //output.Depth = 1-((output.Position.z / FarClip));

	//output.Depth = output.Position;
    
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = mul(mul(input.Position, World),View).z;
	
	output.ViewPosition = output.Position;

	output.WorldPos = input.Position.xyz;

	output.TBN[0] = normalize(mul(input.Tangent, (float3x3)WVIT));
	output.TBN[1] = normalize(mul(input.BiTangent, (float3x3)WVIT));
	output.TBN[2] = normalize(mul(input.Normal, (float3x3)WVIT));

	return output;
}

float PCF(float depth, float NdotL, float2 shadowmap_UV) {
	
	return 0.5f;
}

float3 camera_pos;
float3 atmosphere_color;
float3 sky_color;
bool fog = false;
bool clip_trans = true;
bool fullbright = false;
bool fulldark = false;
PSO MainPS(VertexShaderOutput input)
{
    PSO output = (PSO)0;

    float4 rgba = tex2D(DiffuseSampler, input.TexCoord);
	if (rgba.a < 1 && clip_trans) { clip(-1); }
	/*
    output.Depth.rgb = input.Depth.z/input.Depth.w;
	output.Depth.a = 1;
	*/
	
	output.Depth.r = input.Depth.x / input.Depth.y;
	output.Depth.gba = 1;
	


    output.Normals.rgb = encode(normalize(input.TBN[2]));
	output.Normals.a = 1;


	//float fog_val = 1;
	//if (fog && exp(input.Depth.x / FarClip) > 0.5) {
	//	fog_val = ((exp(input.Depth.x / FarClip) - 0.5) * 2);
	//}
	//TODO vvvUSE ALPHA CHANNEL OF LIGHTINGvvv FOR KEEPING TRACK OF SCENE ALPHA
	// this will allow for at the very least 1 bit of alpha through obviously Lighting.w.a = 0;
	// but also with a bit of work, well, a float is a lot of bytes, it would be possible to do stuff like storing a set of bytes in a float in the alpha, representing an ID from a list of 255 possible values
	// I think this would allow up to 4 transparencies in a row before it'd break, and would allow individual IDs 0-255
	// 4 8-bit ints packed into a 32-bit float, [AAAA/AAAA][BBBB/BBBB][CCCC/CCCC][DDDD/DDDD]
	//										    0xAA,      0xBB,      0xCC,      0xDD

	float d = 1;
	float dist = (distance(camera_pos, input.WorldPos)) / (FarClip);
	float fog_start = 0.85;
	float fog_end = 1;
	float3 atmos = color_lerp(atmosphere_color.rgb, sky_color.rgb, clamp(input.WorldPos.y*0.3, 0.0, 1));

	output.Lighting = float4(0,0,0,1);
	if (fullbright){
		output.Lighting = float4(1,1,1,1);
	}
	if (fulldark) {
		output.Lighting.a = 0;
	}
	if (fog && dist > fog_start) {
		d = 1 - ((dist - fog_start) * (1/(1-fog_start)));
		output.Lighting.a = d;	
	}

    output.Diffuse = color_lerp(rgba * float4(tint, 1), float4(atmos,d), (1-(d))) ;
	//output.Diffuse.a *= d;
	
	if (dist >= 0.999) { 
		output.Lighting.a = 0;	
		output.Diffuse.a = 0;
	}
	//output.Diffuse.a = 1-fog_val;

	return output;
}
	

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
	
technique just_vs
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
	}
};