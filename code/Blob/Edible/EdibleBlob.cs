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

		if ( !SceneObject.IsValid() )
			SceneObject = new Renderer( this, Scene?.SceneWorld );
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();

		Count--;
	}
}
