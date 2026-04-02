#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 View;
float4x4 Projection;

sampler DIFFUSE : register(s0);
sampler NORMAL : register(s1);
sampler DEPTH : register(s2);
sampler LIGHTING : register(s3);

int2 particle_texture_size;


texture particle;
sampler particle_sampler = sampler_state
{
	texture = <particle>;
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

struct instancedVSI {
	float4 row1 : TEXCOORD1;
	float4 row2 : TEXCOORD2;
	float4 row3 : TEXCOORD3;
	float4 row4 : TEXCOORD4;

	float3 normal : NORMAL0;

	float4 tint : COLOR0;
};

struct VSO {
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 Depth : TEXCOORD1;
	float3x3 TBN : TEXCOORD2;
	float2 screen_pos : TEXCOORD6;
	float4 tint : COLOR0;
};

struct PSO {
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

float4x4 CreateMatrixFromRows(float4 r0, float4 r1, float4 r2, float4 r3) {
	return float4x4(r0, r1, r2, r3);
}

float4x4 CreateMatrixFromCols(float4 c0, float4 c1, float4 c2, float4 c3) {
	return float4x4(c0.x, c1.x, c2.x, c3.x,
		c0.y, c1.y, c2.y, c3.y,
		c0.z, c1.z, c2.z, c3.z,
		c0.w, c1.w, c2.w, c3.w);
}


VSO mVSInstanced(VSI input, instancedVSI instance) {
	VSO output = (VSO)0;

	float4x4 world_instance = CreateMatrixFromCols(instance.row1, instance.row2, instance.row3, instance.row4);		
	
	float4 pos = mul(input.Position, world_instance);
	float4 viewpos = mul(pos, View);
	float4 wvp = mul(viewpos, Projection);

	output.Position = wvp;
	
	
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = mul(mul(input.Position, world_instance),View).z;
	
	output.TBN[0] = normalize(mul(input.Tangent, (float3x3)world_instance));
	output.TBN[1] = normalize(mul(input.BiTangent, (float3x3)world_instance));
	output.TBN[2] = normalize(mul(input.Normal, (float3x3)world_instance));
	
	//output.normal = normalize(mul(instance.normal, (float3x3)world_instance_IT));

    output.TexCoord = input.TexCoord;

	output.TexCoord = 1-input.TexCoord;

	output.screen_pos.xy = output.Position.xy;

	output.tint = instance.tint;

	return output;
}

PSO mPS(VSO input) {
	PSO output;
	output.Diffuse.rgba = tex2D(particle_sampler, input.TexCoord).rgba ;
	output.Diffuse.rgb *= input.tint;
	if (output.Diffuse.a < 1) {
		clip(-1);
	}

	output.Diffuse.a = 1;
	
	output.Normals.rgb = encode(normalize(input.TBN[2])) +1;
	//output.Normals.a = output.Diffuse.rgba.a;
	//output.Normals.y = 0.5;

	//output.Normals.rgb = encode(normalize(input.normal.rgb));	
	output.Normals.a = 1;

	output.Depth.r = input.Depth.x / input.Depth.y;
	output.Depth.gba = 1;
	output.Depth.a = output.Diffuse.rgba.a;
	
	float4 d = tex2D(DEPTH, input.screen_pos);
	if (output.Depth.r < d.r) {
		clip(-1);
	}


	float4 l = tex2D(LIGHTING, input.screen_pos);

    output.Lighting.rgba = float4(0,0,0,1);

	//output.Diffuse.a = input.opacity;

	return output ;
}

technique Instanced
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL mVSInstanced();
		PixelShader = compile PS_SHADERMODEL mPS();
	}
}