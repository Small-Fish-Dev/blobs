namespace Blobs;

partial class BlobController
{
	public const float SPLIT_FORCE = 2.1f;
	public const int MIN_SIZE = 200;
	public const int MAX_SIBLINGS = 12;
	public const float SPLIT_FRACTION = 0.4f;

	public int TotalSize => ValidSiblings.Sum( sibling => sibling.Size );
	public IEnumerable<MoveBlob> ValidSiblings => Siblings.Where( blob => blob.IsValid() );

	[Sync( SyncFlags.FromHost )]
	public NetList<MoveBlob> Siblings { get; set; } = new();

	public void RemoveSibling( MoveBlob blob )
	{
		if ( !blob.IsValid() || !Siblings.Contains( blob ) ) return;
		Siblings.Remove( blob );
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly | NetFlags.SendImmediate )]
	public void TrySplit( MoveBlob[] children )
	{
		if ( children is not { Length: > 0 } || ValidSiblings.Count() >= MAX_SIBLINGS )
			return;

		void CreateBlob( Blob parent, int size )
		{
			var obj = new GameObject( true, "child blob" );
			
			var blob = obj.Components.Create<MoveBlob>();
			blob.Size = size;
			blob.WorldPosition = parent.WorldPosition + (MouseNormal * (parent.WorldSize - blob.WorldSize)).Extrude();
			blob.Velocity = parent.WorldSize * MouseNormal * SPLIT_FORCE;
			blob.Controller = this;
			
			obj.SetupNetworking( Network.Owner, OwnerTransfer.Fixed, NetworkOrphaned.Destroy );

			Siblings.Add( blob );
		}

		using ( Scene.Push() )
			foreach ( var child in children )
			{
				if ( !child.IsValid() )
					continue;

				if ( child.Controller != this )
					continue;

				if ( child.Size < MIN_SIZE )
					continue;

				if ( ValidSiblings.Count() >= MAX_SIBLINGS )
					break;

				var size = (int)(child.Size * SPLIT_FRACTION + 0.5f);
				child.Size -= size;
				child.LifeTime = 0f;

				CreateBlob( child, size );
			}	
	}
}
