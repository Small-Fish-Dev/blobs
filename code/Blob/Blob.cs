namespace Blobs;

public partial class Blob
	: Component
{
	protected const int MAX_SIZE = 50000;

	[Property, Sync( SyncFlags.FromHost ), Category( "Settings" )]
	public int Size
	{
		get => _size;
		set
		{
			_size = int.Clamp( value, 1, MAX_SIZE );
		}
	}
	private int _size = 100;

	[Property]
	public bool LerpStart { get; set; } = false;

	public float WorldSize => MathF.Sqrt( Size * 0.1f );
	public float SmoothSize { get; set; }
	public SceneObject SceneObject { get; protected set; }
	public Vector2 Velocity { get; set; }

	[Property, Hide]
	public TimeSince LifeTime { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		LifeTime = 0f;

		if ( !LerpStart )
			SmoothSize = WorldSize;
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();

		if ( SceneObject.IsValid() )
			SceneObject.Delete();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		SmoothSize = MathX.Lerp( SmoothSize, WorldSize, 10f * RealTime.Delta );
	}

	public void Kill()
	{
		Assert.True( Networking.IsHost, "Tried to kill blob on non-host." );

		DestroyGameObject();
	}

	public bool VisibleToSelf()
	{
		var camera = Scene.Camera;
		if ( !camera.IsValid() ) return false;

		var circleRect = new Rect( WorldPosition.Flatten() - WorldSize, WorldSize * 2f );
		var cameraPosition = camera.WorldPosition.Flatten();
		var cameraSize = new Vector2( camera.OrthographicHeight * Screen.Aspect, camera.OrthographicHeight );
		var cameraRect = new Rect( cameraPosition - cameraSize * 0.5f, cameraSize );

		return cameraRect.IsInside( circleRect );
	}

	public void ClampToBounds()
	{
		var bounds = GameManager.Bounds;
		var offset = WorldSize * 0.25f;
		WorldPosition = WorldPosition.Clamp( bounds.Mins + offset, bounds.Maxs - offset );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsProxy )
			return;

		WorldPosition += Velocity.Extrude() * Time.Delta;
		ClampToBounds();

		// Velocity fall-off..
		Velocity *= 0.975f;
		if ( Velocity.IsNearZeroLength )
			Velocity = Vector2.Zero;

		// Bounce off of bounds.
		var bounds = GameManager.Bounds;
		var velocity = Velocity;
		var nextPosition = WorldPosition + Velocity.Extrude() * Time.Delta;
		if ( nextPosition.x <= bounds.Mins.x || nextPosition.x >= bounds.Maxs.x )
			velocity.x *= -0.75f;

		if ( nextPosition.y <= bounds.Mins.y || nextPosition.y >= bounds.Maxs.y )
			velocity.y *= -0.75f;

		Velocity = velocity;
	}
}
