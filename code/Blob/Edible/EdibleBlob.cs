namespace Blobs;

public sealed partial class EdibleBlob 
	: Blob
{
	[Property, Hide]
	public Blob Source { get; set; }

	public Color Color { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Color = Color.Random;

		if ( !SceneObject.IsValid() )
			SceneObject = new Renderer( this, Scene?.SceneWorld );
	}
}
