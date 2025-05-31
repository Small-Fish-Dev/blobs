namespace Blobs;

public sealed class Ground
	: Component
{
	public const float SCALE = 0.01f;

	[Property, Category( "Settings" )]
	public Texture Texture { get; set; }

	private class Renderer
		: SceneCustomObject
	{
		private Ground Ground { get; }

		public Renderer( Ground ground, SceneWorld sceneWorld ) : base( sceneWorld )
		{
			Ground = ground;
		}

		public override void RenderSceneObject()
		{
			var scene = Game.ActiveScene;
			if ( !scene.IsValid() )
				return;

			var camera = scene.Camera;
			if ( !camera.IsValid() )
				return;

			var height = camera.OrthographicHeight;
			var width = height * Screen.Aspect;
			var size = new Vector2( width, width );
			var position = camera.WorldPosition.Flatten();
			var bounds = GameManager.Bounds;

			Attributes.Set( "GroundRepeatTexture", Ground?.Texture );
			Attributes.Set( "WorldOffset", position * SCALE );
			Attributes.Set( "WorldScale", camera.OrthographicHeight * SCALE );
			Attributes.Set( "ScreenAspect", Screen.Aspect );
			Attributes.Set( "WorldBounds", new Vector4( bounds.Mins.x, bounds.Mins.y, bounds.Maxs.x, bounds.Maxs.y ) * SCALE );

			Graphics.DrawQuad( new Rect( -size * 0.5f + position, size ), Material.FromShader( "shaders/ground.shader" ), Color.White, Attributes );
		}
	}

	public SceneObject SceneObject { get; private set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( !SceneObject.IsValid() )
			SceneObject = new Renderer( this, Scene?.SceneWorld );
	}
	
	protected override void OnDisabled()
	{
		base.OnDisabled();

		if ( SceneObject.IsValid() )
			SceneObject.Delete();
	}
}
