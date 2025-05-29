float4x4 World;
float4x4 View;
float4x4 InverseView;
float4x4 Projection;
float4x4 InverseViewProjection;

float3 CameraPosition;
float4x4 LightViewProjection;
float4x4 LightProjection;
float3 LightPosition;
float4 LightColor;
float LightIntensity;
float3 LightDirection;
float LightAngleCos;
float LightClip;
float2 GBufferTextureSize;
bool Shadows;
float shadowMapSize;
float DepthBias;
float C;

sampler DEPTH : register(s0) = sampler_state {
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;

	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};

sampler NORMAL : register(s1) = sampler_state {
	MINFILTER = ANISOTROPIC;
	MAGFILTER = ANISOTROPIC;
	MIPFILTER = ANISOTROPIC;
	MAXANISOTROPY=2;
	
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};

sampler COOKIE: register(s2)= sampler_state {
	MINFILTER = ANISOTROPIC;
	MAGFILTER = ANISOTROPIC;
	MIPFILTER = ANISOTROPIC;
	MAXANISOTROPY=2;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};

sampler SHADOW : register(s3) = sampler_state {
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};


struct VSI {
	float4 Position : POSITION0;
};

struct VSO {
	float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;
};

VSO VS(VSI input) {
	VSO output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.ScreenPosition = output.Position;
	return output;
}

float4 manualSample(sampler Sampler, float2 UV, float2 textureSize) {
	float2 texelpos = textureSize * UV;
	float2 lerps = frac(texelpos);
	float2 texelSize = 1.0 / textureSize;
	float4 sourcevals[4];

	sourcevals[0] = tex2D(Sampler, UV);
	sourcevals[1] = tex2D(Sampler, UV + float2(texelSize.x, 0));
	sourcevals[2] = tex2D(Sampler, UV + float2(0, texelSize.y));
	sourcevals[3] = tex2D(Sampler, UV + float2(texelSize.x, texelSize.y));

	float4 interpolated = lerp(lerp(sourcevals[0], sourcevals[1], lerps.x),
	lerp(sourcevals[2], sourcevals[3], lerps.x ), lerps.y);
	return interpolated;
}

float3 pomn(float3 a, float3 p) {
	float3 b = a + (LightDirection * LightClip);
	float3 ab = b - a;

	float t = dot(p-a, ab) / dot(ab,ab);

	if (t <= 0) { t = 0; }
	if (t >= 1) { t = 1; }

	return a + t * ab;
}

float4 Phong(float3 Position, float3 N, float radialAttenuation,float SpecularIntensity, float SpecularPower) {
	float3 linpos = pomn(LightPosition, Position);

	float3 L = Position.xyz - LightPosition.xyz;

	float heightAttenuation = 1 - (length(linpos - LightPosition.xyz) / LightClip);
	float Attenuation = (radialAttenuation * heightAttenuation);

	L = normalize(L);

	float SL = dot(L, LightDirection);

	float4 Shading = 0;
	if (SL >= LightAngleCos && length(L) < LightClip + DepthBias) {
		float NL = dot(-N, L);
		float3 Diffuse = NL * LightColor.xyz;
		Shading = float4(Diffuse.rgb, 1) * (Attenuation);
	}

	return Shading;
}


float distSquared( float3 A, float3 B )
{

    float3 C = A - B;
    return dot( C, C );

}
float3 decode(float3 enc) {
	return (2.0f * enc.xyz- 1.0f);
}

float RGBADecode(float4 value) {
	const float4 bits = float4(1.0 / (256.0 * 256.0 * 256.0), 1.0 / (256.0 * 256.0), 1.0 / 256.0, 1);
	return dot(value.xyzw , bits);
}


float4 PS(VSO input) : COLOR0 {
	input.ScreenPosition /= input.ScreenPosition.w;

	float2 UV = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1.0f);
	
	float3 Normal = mul(decode(tex2D(NORMAL,UV).xyz), (float3x3)InverseView).xyz;
		
	float Depth = tex2D(DEPTH,UV).r;

	float4 Position = 1.0f;

	Position.xy = input.ScreenPosition.xy;
	Position.z = Depth;
	Position = mul(Position, InverseViewProjection);
	
	Position /= Position.w;

	float4 LightScreenPos = mul(Position, LightViewProjection);
		
	float3 linpos = pomn(LightPosition, Position);

	float Ll = distance(LightPosition.xyz, linpos);
	float linlen = Ll / LightClip;

	float ndl = dot(Normal, LightDirection);
	
	LightScreenPos /= Ll;
		
	float2 LUV =  0.5 * ((float2(LightScreenPos.x, -LightScreenPos.y)) + 1);

	float lZ = tex2D(SHADOW, LUV).r;

	float cookie = tex2D(COOKIE, LUV.xy).r;

	float ShadowFactor = 1;

	if (Shadows) {
		float loglinlen = (log(C * (Ll) + 1) / log(C * LightClip + 1));
		
		if ((loglinlen) >= lZ + DepthBias){
			ShadowFactor = 0;
		}
	}
	//phong
	float4 phong = 0;

	float3 L = Position.xyz - LightPosition.xyz;

	float lengthAttenuation = 1 - (length(linpos - LightPosition.xyz) / LightClip);
	float attenuation = (cookie * lengthAttenuation);

	L = normalize(L);

	float SL = dot(L, LightDirection);

	float4 Shading = 0;
	if (SL >= LightAngleCos && length(L) < LightClip + DepthBias) {
		float3 Diffuse = dot(-Normal, L) * LightColor.xyz;
		phong = float4(Diffuse.rgb, 1) * (attenuation);
	}
	//end phong

	return phong * saturate(ShadowFactor);
	//return float4(Position.xyz, 1) * s;
}

technique Default {
	pass p0 {
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}