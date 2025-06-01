namespace Blobs;

partial class BlobController
{
	public const int MIN_FEED_SIZE = 150;
	public const int FEED_AMOUNT = 25;
	public const float FEED_FORCE = 3.2f;

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly | NetFlags.SendImmediate )]
	public void TryFeed( MoveBlob[] children )
	{
		if ( children is not { Length: > 0 } )
			return;

		void CreateBlob( Blob parent, int size )
		{
			var obj = new GameObject( true, "food" );

			var blob = obj.Components.Create<EdibleBlob>();
			blob.Size = size;
			blob.WorldPosition = parent.WorldPosition + (MouseNormal * (parent.WorldSize - blob.WorldSize)).Extrude();
			blob.Velocity = (parent.WorldSize + 1f) * MouseNormal * FEED_FORCE;
			blob.Source = parent;

			obj.SetupNetworking( null, OwnerTransfer.Fixed, NetworkOrphaned.Destroy );
		}

		foreach ( var child in children )
		{
			if ( !child.IsValid() ) 
				continue;

			if ( child.Controller != this )
				continue;

			if ( child.Size < MIN_FEED_SIZE )
				continue;

			CreateBlob( child, FEED_AMOUNT );
			child.Size -= FEED_AMOUNT;
		}
	}
}
