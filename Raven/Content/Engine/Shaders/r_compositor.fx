#include "lib/general.fx"

sampler DiffuseSampler = sampler_state { texture = <Diffuse>; };
sampler LightingSampler = sampler_state { texture = <Lighting>; };
sampler DepthSampler = sampler_state { texture = <Depth>; };
sampler NormalSampler = sampler_state { texture = <Normal>; };
sampler ComposedSampler = sampler_state { texture = <Composed>; };
sampler OverlaySampler = sampler_state { texture = <Overlay>; };
sampler OutputSampler = sampler_state { texture = <Output>; };

struct VSI {
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VSO {
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
    float4 Pos3d : TEXCOORD1;
};

struct ClearPSO {
    float4 Diffuse : COLOR0;
    float4 Normal : COLOR1;
    float Depth : DEPTH0;
    float4 Lighting : COLOR2;
};

int buffer = -1;

// SCREEN DRAW VARS
float2 screen_resolution;
float2 screen_draw_position;
float2 screen_draw_size;

// VERTEX SHADERS
VSO FullscreenVS(VSI input) {
	VSO output = (VSO)0;
    output.Position = input.Position;    
    output.Pos3d = input.Position;
	output.UV = input.UV;
	return output;
}

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

// PIXEL SHADERS
ClearPSO ClearPS() {
	ClearPSO output;
    output.Diffuse = 0.0;
	output.Normal = 0.0;
	output.Depth = 1.0;
    output.Lighting = 0.0;
	return output;
}

float4 Compose(VSO input) : COLOR {
    float4 rgba = tex2D(DiffuseSampler, input.UV);           
    float4 l = tex2D(LightingSampler, input.UV);            
    float4 d = tex2D(DepthSampler, input.UV);
    float4 n = tex2D(NormalSampler, input.UV);
	
    if (buffer == 0) return rgba;
    else if (buffer == 1) return n; //normals
    else if (buffer == 2) return float4(d.x,d.y,d.z, 1) ; //depth
    else if (buffer == 3) return l; //lighting
	else return float4((l.rgb * 0.2) + saturate((rgba.rgb * (l.rgb))), 1);       
}

float4 Finalize(VSO input) : COLOR {
    float4 composed = tex2D(ComposedSampler, input.UV);
    float4 overlay = tex2D(OverlaySampler, input.UV);
    
    float3 rgb = lerp(composed.rgb, overlay.rgb, overlay.a);
    
    return float4(rgb, 1);
}

float4 ToScreen(VSO input) : COLOR {
    float4 composed = tex2D(OutputSampler, input.UV);     
    return float4((composed.rgb), 1);
}

// TECHNIQUES IN ORDER OF USE
technique clear {
	pass P0	{
		VertexShader = compile VS_SHADERMODEL FullscreenVS();
		PixelShader = compile PS_SHADERMODEL ClearPS();
	}
}

technique compose {
	pass P0	{
		VertexShader = compile VS_SHADERMODEL FullscreenVS();
		PixelShader = compile PS_SHADERMODEL Compose();
	}
};

technique finalize {
	pass P0	{
		VertexShader = compile VS_SHADERMODEL FullscreenVS();
		PixelShader = compile PS_SHADERMODEL Finalize();
	}
};

technique draw_to_screen {
	pass P0 {
		VertexShader = compile VS_SHADERMODEL ScreenVS();
		PixelShader = compile PS_SHADERMODEL ToScreen();
	}
};

technique draw_fullscreen {
	pass P0 {
		VertexShader = compile VS_SHADERMODEL FullscreenVS();
		PixelShader = compile PS_SHADERMODEL ToScreen();
	}
};