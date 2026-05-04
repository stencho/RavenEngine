float4 pattern_select(int pattern, float4 color_a, float4 color_b, float2 UV, float2 size, int dither_resolution) {    
    float2 px = size * UV.xy;
    
    float4 output = float4(1,1,1,1);
    
    if (pattern == 0) {
        // NONE
        output = color_a;
    } 
        
    if (pattern == 1) {    
        // DITHER        
        int2 px_i = int2(px);
    
        int psize = dither_resolution * 2;
        if (psize % 2 != 0) psize += 1;
    
        bool x = px_i.x % psize >= (psize / 2);
        bool y = px_i.y % psize >= (psize / 2);
        
        if ((x && y) || (!x && !y)) {
            output *= color_a;
        } else {		
            output *= color_b;
        }  
    }
    
    if (pattern == 2) {
        // POLKADOT        
    }
    
    if (pattern == 3) {
        // STRIPE              
    }
    
    if (pattern == 4) {
        // GLOW         
    }
    
    return output;
}

