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

    RenderState( SrgbWriteEnable0, true );
	RenderState( ColorWriteEnable0, RGBA );
	RenderState( FillMode, SOLID );
	RenderState( CullMode, BACK );

	CreateTexture2D( g_tAvatarTexture ) < Attribute( "AvatarTexture" ); SrgbRead( true ); >;

    float fresnel( float2 uv, float scale, float power ) 
    {
        return 1.0f - pow( 1.0f - length( uv * scale ), power );
    }

	float4 MainPs( PixelInput i ) : SV_Target
	{
        float2 uv = i.vTextureCoords.xy;
        float4 avatarColor = g_tAvatarTexture.Sample( g_sPointWrap, uv.xy ).rgba;

        const float3 OUTLINE = float3(0, 0, 0);
        float3 result = avatarColor;
        float dist = distance(uv.xy, float2(0.5f, 0.5f)) * 2;
        result = lerp(result, OUTLINE, ceil(dist - 0.975f));
        result = lerp(result, (result + OUTLINE) * 0.5f, ceil(dist - 0.95f));

		return float4( result, 1.f );
	}
}