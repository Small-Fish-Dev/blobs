namespace Blobs;

public sealed partial class BlobController
	: Pawn
{
	[Property, Category( "Components" )]
	public CameraComponent Camera { get; set; }

	public MoveBlob Main => ValidSiblings.MaxBy( blob => blob?.SmoothSize ?? 0 );

	[Property, Category( "Settings" )]
	public RangedFloat ZoomRange { get; set; } = new RangedFloat( 250f, 750f );

	[Property, Category( "Settings" ), Range( 1f, 10f )]
	public float ZoomScale { get; set; } = 1.5f;

	[Property, Category( "Settings" ), Range( 0f, 100f )]
	public float DefaultDistance { get; set; } = 15f;

	public float Zoom { get; set; }
	public float SmoothZoom { get; private set; }
	public Vector2 WishDirection { get; private set; }

	[Sync]
	public Vector2 MouseDirection { get; set; }
	public Vector2 MouseNormal => MouseDirection.Normal;

	private TimeSince _sinceFed;

	protected override void OnStart()
	{
		base.OnStart();

		if ( Camera.IsValid() )
			Camera.Enabled = !IsProxy;

		if ( !IsProxy )
			return;

		Zoom = ZoomRange.Max - ZoomRange.Min;
		SmoothZoom = Zoom;
	}

	public override void OnHostSpawned()
	{
		if ( !Networking.IsHost )
			return;

		using ( Scene.Push() )
		{
			var bounds = GameManager.Bounds;
			var obj = new GameObject( true, "child blob" );

			var blob = obj.Components.Create<MoveBlob>();
			blob.Size = 100;
			blob.WorldPosition = new Vector3(
				Game.Random.Float( bounds.Mins.x, bounds.Maxs.x ),
				Game.Random.Float( bounds.Mins.y, bounds.Maxs.y ),
				0f
			);
			blob.Controller = this;
			blob.ClampToBounds();

			obj.SetupNetworking( Client.Connection, OwnerTransfer.Fixed, NetworkOrphaned.Destroy );

			Siblings.Add( blob );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		if ( !Main.IsValid() )
		{
			DestroyGameObject();
			return;
		}

		// Interpolate some values, "zoom out" our camera.
		var before = Main.SmoothSize;
		Zoom = MathX.Clamp( Zoom - Input.MouseWheel.y * 250f, ZoomRange.Min, ZoomRange.Max );

		SmoothZoom = MathX.Lerp( SmoothZoom, Zoom, 10f * RealTime.Delta );

		if ( Camera.IsValid() )
		{
			var pos = Camera.WorldPosition.z;
			var center = Vector3.Zero;
			var count = 0;
			foreach ( var sibling in ValidSiblings )
			{
				center += sibling.WorldPosition;
				count++;
			}

			var targetHeight = Main.SmoothSize * 2f + SmoothZoom / ZoomRange.Max * before * ZoomScale + DefaultDistance * (count * 0.5f);
			Camera.OrthographicHeight = MathX.Lerp( Camera.OrthographicHeight, targetHeight, 10f * Time.Delta );
			Camera.WorldPosition = Vector3.Lerp( Camera.WorldPosition, (center / Math.Max( count, 1 )).WithZ( pos ), 10f * Time.Delta );
		}

		// Wish direction is from mouse, centered on screen.
		var screenCenter = Screen.Size * 0.5f;
		var mouse = Mouse.Position;
		var dir = mouse - screenCenter;
		dir.y = -dir.y;

		MouseDirection = dir;

		var len = (dir / screenCenter * 8).Clamp( -1f, 1f );
		WishDirection = dir.Normal.Abs() * len;

		// Split if possible.
		if ( Input.Pressed( "Split" ) && _sinceFed > 0.1f )
		{
			TrySplit( ValidSiblings.ToList() );
			_sinceFed = 0f;
		}

		// Feed if possible.
		if ( Input.Pressed( "Feed" ) && _sinceFed > 0.1f )
		{
			TryFeed( ValidSiblings.ToList() );
			_sinceFed = 0f;
		}
	}

	[Rpc.Host]
	private void GiveFeed( int size )
	{
		if ( !Main.IsValid() )
			return;

		Main.Size += size;
	}

	[ConCmd( "give_feed" )]
	private static void DebugGiveFeedCommand( int size )
	{
		if ( Client.Local?.Pawn is not BlobController controller )
			return;

		controller.GiveFeed( size );
	}
}
