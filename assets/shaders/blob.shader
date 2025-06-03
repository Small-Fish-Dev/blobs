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

	float g_flBlobRadius < Attribute( "BlobRadius" ); Default( 1.f ); >;
 	float4 g_flWorldBounds < Attribute( "WorldBounds" ); >;

	PixelInput MainVs( VertexInput i )
	{
		float offset = (i.vPositionOs.x + i.vPositionOs.y) * 32.f;	
		float scale = g_flBlobRadius * 0.005f;
		float2 wave = float2(
			(0.02f + scale) * sin( offset * 4.f + g_flTime * 5.f + g_flBlobRadius * 32.f), 
			(0.02f + scale) * cos( offset * 2.f + g_flTime * 5.f + g_flBlobRadius * 32.f)
		);

		float2 mins = g_flWorldBounds.xy;
		float2 maxs = g_flWorldBounds.zw;

		i.vPositionOs.xy *= g_flBlobRadius;
		i.vPositionOs.xy += wave.xy;

		PixelInput o = ProcessVertex( i );

		o.vPositionWs.xy = min(max(o.vPositionWs.xy, mins.xy), maxs.xy);
		o.vPositionPs.xyzw = Position3WsToPs( o.vPositionWs.xyz );

		return FinalizeVertex( o );
	}
}

PS
{        
    #include "common/pixel.hlsl"

	DynamicCombo( D_ISSOLID, 0..1, Sys( ALL ) );

    RenderState( SrgbWriteEnable0, true );
	RenderState( ColorWriteEnable0, RGBA );
	RenderState( FillMode, SOLID );
	RenderState( CullMode, BACK );

	CreateTexture2D( g_tAvatarTexture ) < Attribute( "AvatarTexture" ); SrgbRead( true ); >;

	float3 g_flBlobColor < Attribute( "BlobColor" ); Default3( 1, 1, 1 ); >;
	float g_flBlobOutlineSize < Attribute( "BlobOutlineSize" ); Default( 0.1f ); >;

	float4 MainPs( PixelInput i ) : SV_Target
	{
        float2 uv = i.vTextureCoords.xy;
        float4 avatarColor = g_tAvatarTexture.Sample( g_sPointWrap, uv.xy ).rgba;
		float dist = distance(uv.xy, float2(0.5f, 0.5f)) * 2;

		float outlineSize = 1.f - g_flBlobOutlineSize;
		float outlineSizeInner = 1.f - g_flBlobOutlineSize * 1.5f;

		#if( D_ISSOLID )
			float3 outline = g_flBlobColor * 0.6f;
			float3 result = g_flBlobColor;
			result = lerp(result, outline, ceil(dist - outlineSize));
			result = lerp(result, (result + outline) * 0.5f, ceil(dist - outlineSizeInner));
			
			return float4( result, 1.f );
		#else
			const float3 OUTLINE = float3(0, 0, 0);
			float3 result = avatarColor.rgb;
			result = lerp(result, OUTLINE, ceil(dist - outlineSize));
			result = lerp(result, (result + OUTLINE) * 0.5f, ceil(dist - outlineSizeInner));

			return float4( result, 1.f );
		#endif
	}
}