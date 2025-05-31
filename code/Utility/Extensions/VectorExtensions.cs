namespace Blobs;

public static class VectorExtensions
{
	public static Vector2 Flatten( this Vector3 self )
	{
		return new Vector2( self.x, self.y );
	}

	public static Vector3 Extrude( this Vector2 self )
	{
		return new Vector3( self.x, self.y, 0f );
	}
}
