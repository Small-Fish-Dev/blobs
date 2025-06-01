namespace Blobs;

public sealed partial class BlobController
	: Pawn
{
	[Property, Category( "Components" )]
	public CameraComponent Camera { get; set; }

	public MoveBlob Main => ValidSiblings.MaxBy( blob => blob.SmoothSize ) ?? Base;

	[Property, Category( "Components" )]
	public MoveBlob Base { get; set; }

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

	protected override void OnStart()
	{
		base.OnStart();

		if ( Camera.IsValid() )
			Camera.Enabled = !IsProxy;

		if ( Base.IsValid() )
		{
			Base.Controller = this;
			Siblings.Add( Base );
		}

		if ( !IsProxy )
			return;

		Zoom = ZoomRange.Max - ZoomRange.Min;
		SmoothZoom = Zoom;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !Main.IsValid() || IsProxy )
			return;

		// Interpolate some values, "zoom out" our camera.
		var before = Main.SmoothSize;
		Zoom = MathX.Clamp( Zoom - Input.MouseWheel.y * 100f, ZoomRange.Min, ZoomRange.Max );

		SmoothZoom = MathX.Lerp( SmoothZoom, Zoom, 10f * RealTime.Delta );

		if ( Camera.IsValid() )
		{
			var pos = Camera.WorldPosition.z;
			Camera.OrthographicHeight = Main.SmoothSize * 2f + SmoothZoom / ZoomRange.Max * before * ZoomScale + DefaultDistance;
			Camera.WorldPosition = Vector3.Lerp( Camera.WorldPosition, Main.WorldPosition.WithZ( pos ), 10f * Time.Delta );
		}

		// Wish direction is from mouse, centered on screen.
		var center = Screen.Size * 0.5f;
		var mouse = Mouse.Position;
		var dir = mouse - center;
		dir.y = -dir.y;

		MouseDirection = dir;
		WishDirection = (dir / center * 8).Clamp( -1f, 1f );

		// Split if possible.
		if ( Input.Pressed( "Split" ) )
			TrySplit( ValidSiblings.ToArray() );

		// Feed if possible.
		if ( Input.Pressed( "Feed" ) )
			TryFeed( ValidSiblings.ToArray() );
	}
}
