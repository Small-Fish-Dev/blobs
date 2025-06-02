namespace Blobs;

partial class MoveBlob
{
	const float EAT_MIN = 50f; // A minimum size threshold for eating another blob.
	const float EAT_THRESHOLD = 0.15f; // How much bigger we need to be to eat a blob.
	const float EAT_RADIUS = 0.7f; // How much we need to be inside of the blob to eat it.

	const float FEED_EAT_DELAY = 4f;

	public bool CanEat( Blob blob )
	{
		// Other blob isn't valid.
		if ( !blob.IsValid() ) return false;

		// Blob is another move blob.
		if ( blob is MoveBlob other )
		{
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
		}

		// We shoot out a edible blob from this blob.
		else if ( blob is EdibleBlob edible )
		{
			if ( edible.Source == this && edible.Life < FEED_EAT_DELAY )
				return false;
		}

		// Blob isn't within eating radius.
		var otherPosition = blob.WorldPosition.Flatten();
		var position = WorldPosition.Flatten();
		var distance = otherPosition.Distance( position );

		if ( distance > WorldSize - blob.WorldSize * EAT_RADIUS ) return false;

		// We can eat the blob.
		return true;
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly | NetFlags.SendImmediate )]
	public void TryEat( Blob blob )
	{
		if ( !blob.IsValid() ) return;
		if ( !CanEat( blob ) ) return;
		if ( !Networking.IsHost ) return;

		Size += blob.Size;
		blob.Kill();
	}
}
