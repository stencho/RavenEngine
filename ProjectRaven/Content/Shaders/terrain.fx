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
float4x4 World;

float4x4 inv_view_proj;

float3 cam_pos;

float FarClip;
float NearClip;

float4 tint = float4(1.0, 1.0, 1.0, 1.0);


float4x4 inverse(float4x4 m) {
    float n11 = m[0][0], n12 = m[1][0], n13 = m[2][0], n14 = m[3][0];
    float n21 = m[0][1], n22 = m[1][1], n23 = m[2][1], n24 = m[3][1];
    float n31 = m[0][2], n32 = m[1][2], n33 = m[2][2], n34 = m[3][2];
    float n41 = m[0][3], n42 = m[1][3], n43 = m[2][3], n44 = m[3][3];

    float t11 = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
    float t12 = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
    float t13 = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
    float t14 = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;

    float det = n11 * t11 + n21 * t12 + n31 * t13 + n41 * t14;
    float idet = 1.0f / det;

    float4x4 ret;

    ret[0][0] = t11 * idet;
    ret[0][1] = (n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44) * idet;
    ret[0][2] = (n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44) * idet;
    ret[0][3] = (n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43) * idet;

    ret[1][0] = t12 * idet;
    ret[1][1] = (n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44) * idet;
    ret[1][2] = (n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44) * idet;
    ret[1][3] = (n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43) * idet;

    ret[2][0] = t13 * idet;
    ret[2][1] = (n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44) * idet;
    ret[2][2] = (n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44) * idet;
    ret[2][3] = (n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43) * idet;

    ret[3][0] = t14 * idet;
    ret[3][1] = (n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34) * idet;
    ret[3][2] = (n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34) * idet;
    ret[3][3] = (n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33) * idet;

    return ret;
}
struct VSI
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;
};

struct VSO
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float4 Depth : TEXCOORD1;
	float3x3 TBN : TEXCOORD2;
	float3 pos3d : TEXCOORD6;
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

half3 decode(half3 enc)
{
	return (2.0f * enc.xyz - 1.0f);
}

float logzbuf(float z, float w)
{
	return max(1e-6, log(NearClip * z + 1) / log(NearClip * FarClip + 1) * w);
}
float logzbuf(float4 xyzw)
{
    return max(1e-6, log(NearClip * xyzw.z + 1) / log(NearClip * FarClip + 1) * xyzw.w);
}


texture DiffuseMap;

float4 sky_color = float4(1,1, 1, 1);
float sky_brightness = 1;


//float Heightmap_Weights[];
//int Heightmap_Stride;

sampler DiffuseSampler = sampler_state
{
	texture = <DiffuseMap>;
    magfilter = ANISOTROPIC;
    minfilter = ANISOTROPIC;
    mipfilter = ANISOTROPIC;
    MaxAnisotropy = 16;
    ADDRESSU = MIRROR;
    ADDRESSV = MIRROR;
};

texture VisibilityMap;
sampler VisibilitySampler = sampler_state
{
	texture = <VisibilityMap>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    MaxAnisotropy = 16;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

texture OverlayMap;
sampler OverlaySampler = sampler_state
{
	texture = <OverlayMap>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
    MaxAnisotropy = 16;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

texture NormalMap;

sampler NormalMapSampler = sampler_state
{
	texture = <NormalMap>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
    ADDRESSU = MIRROR;
    ADDRESSV = MIRROR;
};
float3 light_dir = float3(0.5, -0.5, 0);

float Phong(float3 N)
{
    float3 R = normalize(reflect(light_dir.rgb, N));
    return dot(N, -light_dir.rgb);
}

VSO Vert(VSI input)
{
    VSO output;
    float4x4 wvp = mul(World, mul(View, Projection));
	
	//if (length(height) )

    output.Position.xyzw = mul(input.Position, wvp);
    output.Depth = 1-((output.Position.z / FarClip) / 1);
	
    output.pos3d.xyz = mul(input.Position, World);
	//output.pos3d.z = input.Position.z;

	//output.pos3d.xyz = mul(output.pos3d.xyz, inv_view_proj);
	//output.pos3d /= output.pos3d.z;

	//output.Depth.x = logzbuf(output.Position.z, output.Position.w);
	//output.Depth.y = output.Position.z;
	//output.Depth.z = output.Position.w;                   
    
	output.Depth.a = 1;	

    output.TBN[0] = normalize(mul(input.Tangent, (float3x3) World));
    output.TBN[1] = normalize(mul(input.BiTangent, (float3x3) World));
    output.TBN[2] = normalize(mul(input.Normal, (float3x3) World));

    output.TexCoord = input.TexCoord;

	//output.Color.a = output.Color.a * opacity;
    return output;
}

float atmosphere = 0.02;
bool fullbright = false;
bool transparent = false;
bool use_visibility_map = false;

const float3 pos_pl = float3(15,3,15);

PSO Diffuse(VSO input)
{
    
    float4 rgba = tex2D(DiffuseSampler, input.TexCoord);
    float4 overlay = tex2D(OverlaySampler, input.TexCoord);
    float4 vis = tex2D(VisibilitySampler, input.TexCoord);
	float visible = ((vis.x + vis.y + vis.z) / 3);
	if (use_visibility_map){
		if (visible < 0.5) clip(-1); 
	} 
    float4 norm = tex2D(NormalMapSampler, input.TexCoord);
    //rgba.rgb *= 0.8;
	//float4 em = tex2D(EmissiveSamplerPoint, input.TexCoord);    
    
	PSO output = (PSO)0;
	
	float3 normal = (norm);
    
	output.Normals.xyz = normal;
	output.Normals.w = 1;

    
	//output.Depth.xyz = input  .Depth.xyz;
    //output.Depth.a = 1;
    
    
    output.Diffuse.rgb = rgba.rgb;
    output.Diffuse.a = 1;
    
	if (overlay.a > 0.9) {
		output.Diffuse.rgb = overlay.rgb;
	}

    //output.Lighting.rgb = 0.3 + clamp(sky_color.rgb * 1.4, 0, 1) * (sky_brightness);
        
    
    //if (!fullbright)
        output.Lighting.rgb  = visible;
        
	//if (distance(pos_pl, input.pos3d.xyz) < 25)
		//output.Lighting.r = 1;

    output.Lighting.a = 1;
    

	//output.Diffuse.rgba *= visible;

    output.Depth.rgb = ( input.Depth.z);
    output.Depth.a = 1;
    
    return output ;
};





technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL Vert();
		PixelShader = compile PS_SHADERMODEL Diffuse();
	}

};