namespace Blobs;

public sealed partial class MoveBlob 
	: Blob
{
	public const float RECONNECTION_COOLDOWN = 30f;

	public const float SPEED_SMALL = 6f;
	public const float SPEED_LARGE = 2f;

	[Sync( SyncFlags.FromHost )]
	public BlobController Controller { get; set; }

	[Sync( SyncFlags.FromHost )]
	public TimeSince LifeTime { get; set; } = 0f;

	public float Speed => MathX.Lerp( SPEED_SMALL, SPEED_LARGE, (Size * 8f) / (float)MAX_SIZE ) + (Controller?.Main != this ? 0.5f : 0f);
	public bool Reconnecting => Controller.Main != this && Controller.Main.IsValid() && LifeTime > RECONNECTION_COOLDOWN;

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Controller.IsValid() )
			Controller.RemoveSibling( this );
	}

	private void TryRepelAndConnect()
	{
		if ( !Controller.Main.IsValid() || Controller.Main == this )
			return;

		const float RECONNECTION_SPEED = 15f;
		const float REPEL_FORCE = 15f;

		var position = WorldPosition.Flatten();
		if ( Reconnecting )
		{
			var mainPosition = Controller.Main.WorldPosition.Flatten();
			var distance = position.Distance( mainPosition );
			var direction = Vector2.Direction( position, mainPosition );
			var force = MathX.Clamp( distance / (Controller.Main.WorldSize * Controller.Main.WorldSize) - 0.5f, 0.5f, 1f );

			Velocity += RECONNECTION_SPEED * force * direction * Time.Delta;
		}
		else
			foreach ( var sibling in Controller.ValidSiblings )
			{
				if ( sibling == this || WorldSize > sibling.WorldSize ) continue;

				var siblingPosition = sibling.WorldPosition.Flatten();
				var distance = position.Distance( siblingPosition );
				var radii = WorldSize + sibling.WorldSize;

				if ( distance < radii )
				{
					var strength = MathF.Max( 0.2f, 1f - distance / radii ) * WorldSize;
					var direction = Vector2.Direction( position, siblingPosition );
					var force = REPEL_FORCE * direction * strength;

					Velocity -= force * Time.Delta;
				}
			}
	}

	private void TryEatBlobs()
	{
		var position = WorldPosition.Flatten();

		foreach ( var blob in Scene.GetAllComponents<Blob>() )
		{
			if ( blob == this ) continue;
			if ( !CanEat( blob ) ) continue;

			TryEat( blob );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( SceneObject.IsValid() && SceneObject.RenderingEnabled )
		{
			var steamId = Controller?.Client?.SteamId ?? default;
			SceneObject.Attributes?.Set( "AvatarTexture", Texture.LoadAvatar( steamId, 128 ) );
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsProxy || !Controller.IsValid() )
			return;

		// Re-connect to main blob and apply repelling forces.
		TryRepelAndConnect();

		// Try to eat other blobs.
		TryEatBlobs();

		// Move the blob.
		WorldPosition += Controller.WishDirection.Extrude() * Speed * Time.Delta;
		ClampToBounds();
	}
}
