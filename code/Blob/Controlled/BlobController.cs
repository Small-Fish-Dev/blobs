namespace Blobs;

public sealed partial class BlobController
	: Pawn
{
	[Property, Category( "Components" )]
	public CameraComponent Camera { get; set; }

	[Property, Category( "Components" )]
	public MoveBlob Main { get; set; }

	[Property, Category( "Settings" )]
	public RangedFloat ZoomRange { get; set; } = new RangedFloat( 250f, 750f );

	[Property, Category( "Settings" ), Range( 1f, 10f )]
	public float ZoomScale { get; set; } = 1.5f;

	[Property, Category( "Settings" ), Range( 0f, 100f )]
	public float DefaultDistance { get; set; } = 15f;

	public float Zoom { get; set; }
	public float SmoothZoom { get; private set; }
	public Vector2 WishDirection { get; private set; }
	public Vector2 MouseNormal { get; private set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( Camera.IsValid() )
			Camera.Enabled = !IsProxy;

		if ( Main.IsValid() )
			Main.Controller = this;

		if ( !IsProxy )
			return;

		Zoom = ZoomRange.Max - ZoomRange.Min;
		SmoothZoom = Zoom;
	}
	
	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !Main.IsValid() )
			return;

		// Interpolate some values, "zoom out" our camera.
		var before = Main.SmoothSize;
		Zoom = MathX.Clamp( Zoom - Input.MouseWheel.y * 100f, ZoomRange.Min, ZoomRange.Max );

		SmoothZoom = MathX.Lerp( SmoothZoom, Zoom, 10f * RealTime.Delta );

		if ( Camera.IsValid() )
			Camera.OrthographicHeight = Main.SmoothSize * 2f + SmoothZoom / ZoomRange.Max * before * ZoomScale + DefaultDistance;

		// Wish direction is from mouse, centered on screen.
		var dir = (Mouse.Position / Screen.Size * 2f - 1f)
			.Clamp( -1, 1 );
		dir.y = -dir.y;

		MouseNormal = dir.Normal;
		WishDirection = (dir * 8).Clamp( -1, 1 );

		// Split if possible.
		if ( Input.Pressed( "Jump" ) )
			TrySplit( ValidSiblings.ToArray() );
	}
}
