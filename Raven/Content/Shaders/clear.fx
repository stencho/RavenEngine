//Vertex Shader
float4 VS(float3 Position : POSITION0) : POSITION0
{
	return float4(Position, 1);
}
//Pixel Shader Out
struct PSO
{
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Depth : COLOR2;
    float4 Lighting : COLOR3;
};


//Normal Encoding Function
half3 encode(half3 n)
{
	n = normalize(n);
	n.xyz = 0.5f * (n.xyz + 1.0f);
	return n;
}
//Pixel Shader
float4 color;
PSO PS()
{
	//Initialize Output
	PSO output;
	//Clear Albedo to Transperant Black
    output.Diffuse = color; //    float4(0.55, 0.15, 0.7, 1);
	//Clear Normals to 0(encoded value is 0.5 but can't use normalize on 0, compile error)
	output.Normals.xyz = 0.5f;
	output.Normals.w = 0.0f;
	//Clear Depth to 1.0f
	output.Depth = 0.5f;
    output.Lighting.rgba = 0.0f;
    //output.Lighting.a = 0;
	//Return
	return output;
}//Technique
technique Default
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}