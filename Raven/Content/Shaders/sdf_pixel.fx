float2 offset;

float2 billboard_size;
float2 draw_offset = float2(0,0);
float2 draw_scale = float2(1,1);

float alpha_scissor = 0.5;

float opacity = 1;
float outline_width = 0;

bool invert_map = false;

bool enable_outline = false;

float4 inside_color;
float4 outside_color;
float4 outline_color; 

sampler2D SDFs : register(s0) {	
	texture = <SDFTEX>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

bool enable_outside_overlay = false;
bool enable_inside_overlay = false;
bool enable_outline_overlay = false;

sampler2D OVERLAY_INSIDE : register(s1){	
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
sampler2D OVERLAY_OUTSIDE : register(s2){	
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
sampler2D OVERLAY_OUTLINE : register(s3) {	
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

float2 inside_tile_count = (float2)1;
float2 outside_tile_count = (float2)1;
float2 outline_tile_count = (float2)1;


float4x4 World;
float4x4 View;
float4x4 Projection;


struct VertexShaderInput
{
	float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};
struct PSO
{
    float4 Color : COLOR0;
	float Depth : SV_Depth;
};


//vestigial vs
VertexShaderOutput VS(VertexShaderInput input) {
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 wvp = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, wvp);
    output.TexCoord = input.TexCoord;

	return output;
}

//Pixel Shader
PSO PS(float4 position : SV_Position, float4 color : COLOR0, float2 TexCoords : TEXCOORD0)
{
	PSO output = (PSO)0;

	float a = (tex2D(SDFs, TexCoords).r);	

	float4 rgba_inside_overlay = tex2D(OVERLAY_INSIDE, TexCoords * inside_tile_count).rgba; 
	float4 rgba_outside_overlay = tex2D(OVERLAY_OUTSIDE, TexCoords * outside_tile_count).rgba; 		
	float4 rgba_outline_overlay = tex2D(OVERLAY_OUTLINE, TexCoords * outline_tile_count).rgba; 

	if (!enable_inside_overlay){
		rgba_inside_overlay = float4(1,1,1,1);
	}
	if (!enable_outside_overlay){
		rgba_outside_overlay = float4(1,1,1,1);
	}
	if (!enable_outline_overlay){
		rgba_outline_overlay = float4(1,1,1,1);
	}
	
	if (invert_map)
		a = 1-a;		


	//inside color
	float4 rgba = inside_color * rgba_inside_overlay; 
	
	if (a > clamp(alpha_scissor, 0.001, 1) - outline_width && a > 0 && enable_outline && outline_width > 0) {
		//outline color
		rgba = outline_color * rgba_outline_overlay; 

	} else if (a < clamp(alpha_scissor, 0.001, 1)){
		//outside color
		rgba = outside_color * rgba_outside_overlay;

	} 
	
	if (a > clamp(alpha_scissor, 0.001, 1)) {
		rgba = inside_color * rgba_inside_overlay; 
	}
	
	output.Color = rgba * float4(1,1,1, opacity);
	output.Depth = 0;

	return output;
}


technique Default
{
	pass p0
	{
		PixelShader = compile ps_3_0 PS();
	}
}

technique Full
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}