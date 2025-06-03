namespace Blobs;

public sealed partial class EdibleBlob 
	: Blob
{
	public static int Count { get; set; }

	[Property, Hide]
	public Blob Source { get; set; }

	public Color Color { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Color = Color.Random;
		Count++;
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();

		Count--;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( SceneObject.IsValid() && SceneObject.RenderingEnabled )
		{
			SceneObject.Attributes?.Set( "D_ISSOLID", true );
			SceneObject.Attributes?.Set( "BlobColor", Color );
			SceneObject.Attributes?.Set( "BlobOutlineSize", 0.25f );
		}
	}
}
