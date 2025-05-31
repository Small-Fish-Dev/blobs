FEATURES 
{
    #include "vr_common_features.fxc"
}

MODES
{
    Default();
    Forward();
}

COMMON
{
	#include "common/shared.hlsl"
}


struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{        
    #include "common/pixel.hlsl"

    SamplerState g_sSampler < Filter( BILINEAR ); AddressU( REPEAT ); AddressV( REPEAT ); >;

    RenderState( SrgbWriteEnable0, true );
	RenderState( ColorWriteEnable0, RGBA );
	RenderState( FillMode, SOLID );
	RenderState( CullMode, BACK );

	CreateTexture2D( g_tGroundRepeatTexture ) < Attribute( "GroundRepeatTexture" ); SrgbRead( true ); >;
    float2 g_flWorldOffset < Attribute( "WorldOffset" ); >;
    float g_flWorldScale < Attribute( "WorldScale" ); >;
    float g_flScreenAspect < Attribute("ScreenAspect"); >;
 	float4 g_flWorldBounds < Attribute( "WorldBounds" ); >;

	float4 MainPs( PixelInput i ) : SV_Target
	{
		const float3 VOID_COLOR = 0.25f;
        const float2 SCALE = float2(24.f, 24.f);

		float2 mins = float2(g_flWorldBounds.x, g_flWorldBounds.y);
		float2 maxs = float2(g_flWorldBounds.z, g_flWorldBounds.w);

        float2 screenCenter = i.vTextureCoords.xy - 0.5;
        float2 worldSize = float2(g_flWorldScale, g_flWorldScale) * g_flScreenAspect;
        float2 worldPosition = screenCenter * worldSize + g_flWorldOffset;
		
		float oob = (1.f - ceil(mins.x - worldPosition.x)) * (1.f - ceil(mins.y - worldPosition.y)); // mins
		oob = oob * (1.f - ceil(worldPosition.x - maxs.x)) * (1.f - ceil(worldPosition.y - maxs.y)); // maxs
		oob = max(min(oob, 1), 0);

        float4 groundColor = g_tGroundRepeatTexture.Sample( g_sSampler, worldPosition.xy / (1.f / SCALE) ).rgba;
		float3 result = lerp(groundColor.rgb, groundColor.rgb * VOID_COLOR, 1.f - oob);
		return float4( result.rgb, 1.f );
	}
}