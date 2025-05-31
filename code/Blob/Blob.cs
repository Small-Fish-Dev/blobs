namespace Blobs;

public partial class Blob
	: Component
{
	protected const int MAX_SIZE = 10000;

	[Property, Sync, Category( "Settings" )]
	public int Size
	{
		get => _size;
		set
		{
			_size = int.Clamp( value, 1, MAX_SIZE );
		}
	}
	private int _size = 100;

	public float WorldSize => Size * Ground.SCALE;
	public float SmoothSize { get; private set; }
	public SceneObject SceneObject { get; protected set; }


	protected override void OnStart()
	{
		base.OnStart();
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
}
