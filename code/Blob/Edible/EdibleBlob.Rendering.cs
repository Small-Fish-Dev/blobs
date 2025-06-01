namespace Blobs;

partial class EdibleBlob
{
	private class Renderer
		: SceneCustomObject
	{
		const int POINTS = 12;

		const int VERTICES = POINTS + 1;
		const int INDICES = VERTICES * 3;

		private EdibleBlob Blob { get; }
		private TimeSince TimeOffset { get; }

		private Vertex[] Vertices { get; }
		private ushort[] Indices { get; }

		public Renderer( EdibleBlob blob, SceneWorld sceneWorld ) : base( sceneWorld )
		{
			Blob = blob;
			TimeOffset = Game.Random.Float( -999f, 999f );

			Vertices = new Vertex[VERTICES];
			Indices = new ushort[INDICES];
		}

		public override void RenderSceneObject()
		{
			if ( !Blob.IsValid() || !Blob.VisibleToSelf() )
				return;

			// Draw the blob circle.
			var center = Blob.WorldPosition + Vector3.Up * Blob.WorldSize;
			var bounds = GameManager.Bounds;
			var radius = Blob.SmoothSize;
			var time = TimeOffset / (radius * 0.01f + 1f);

			Vertices[0] = new Vertex()
			{
				Position = center,
				TexCoord0 = new Vector4( 0.5f ),
				Color = Color.White
			};

			for ( int i = 1; i <= POINTS; i++ )
			{
				var input = 2f * MathF.PI * i / POINTS;
				var circle = new Vector3( MathF.Cos( input ), MathF.Sin( input ), 0f );

				var wave = new Vector3(
					radius * 0.02f * MathF.Sin( i * 4 + time * 2f ),
					radius * 0.02f * MathF.Cos( i * 2 + time * 1.2f ),
					0f
				);

				var position = (center - radius * circle);
				var uv = new Vector2(
					0.5f + (position.x - center.x) / (2f * radius),
					0.5f - (position.y - center.y) / (2f * radius)
				);

				Vertices[i] = new Vertex()
				{
					Position = (position + wave).Clamp( bounds.Mins, bounds.Maxs ) + Vector3.Up * Blob.WorldSize,
					TexCoord0 = new Vector4( uv ),
					Color = Color.White
				};

				var j = (i - 1) * 3;
				Indices[j] = 0;
				Indices[j + 1] = (ushort)(i - 1);
				Indices[j + 2] = (ushort)i;
			}

			Indices[INDICES - 3] = 0;
			Indices[INDICES - 2] = (ushort)(VERTICES - 1);
			Indices[INDICES - 1] = 1;
			
			Attributes.Set( "D_ISSOLID", true );
			Attributes.Set( "BlobColor", Blob.Color );
			Attributes.Set( "BlobOutlineSize", 0.25f );

			Graphics.Draw( Vertices, VERTICES, Indices, INDICES, Material.FromShader( "shaders/blob.shader" ), Attributes );
		}
	}
}
