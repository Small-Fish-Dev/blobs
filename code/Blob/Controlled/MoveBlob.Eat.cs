namespace Blobs;

partial class MoveBlob
{
	const float EAT_MIN = 50f; // A minimum size threshold for eating another blob.
	const float EAT_THRESHOLD = 0.15f; // How much bigger we need to be to eat a blob.
	const float EAT_RADIUS = 0.9f; // How much we need to be inside of the blob to eat it.
	
	public bool CanEat( Blob blob )
	{
		if ( blob is not MoveBlob other )
			return false;

		// Other blob isn't valid.
		if ( !other.IsValid() ) return false;

		// Other blob is related to this one.
		var reconnecting = false;
		if ( other == this || other.Controller == Controller )
		{
			if ( !other.Reconnecting )
				return false;

			reconnecting = true;
		}

		// Blob isn't big enough to eat the other.
		if ( !reconnecting && other.Size * (1f + EAT_THRESHOLD) + EAT_MIN >= Size ) return false;

		// Blob isn't within eating radius.
		var otherPosition = other.WorldPosition.Flatten();
		var position = WorldPosition.Flatten();
		var distance = otherPosition.Distance( position );

		if ( distance > WorldSize * EAT_RADIUS ) return false;

		// We can eat the blob.
		return true;
	}

	public void Kill()
	{
		Assert.True( Networking.IsHost, "Tried to kill blob on non-host." );
		GameObject.Destroy();
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly | NetFlags.SendImmediate )]
	public void TryEat( Blob blob )
	{
		if ( blob is not MoveBlob other )
			return;

		if ( !other.IsValid() ) return;
		if ( !CanEat( other ) ) return;

		Size += other.Size;
		other.Kill();
	}
}
