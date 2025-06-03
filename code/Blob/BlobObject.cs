namespace Blobs;

public sealed class BlobObject
	: SceneObject
{
	private static Model GeneratedModel { get; } = GenerateModel();

	public BlobObject( SceneWorld sceneWorld ) : base( sceneWorld, GeneratedModel ) 
	{
		Batchable = false;
	}

	private static Model GenerateModel()
	{
		const int POINTS = 48;

		const int VERTICES = POINTS + 1;
		const int INDICES = VERTICES * 3;

		var material = Material.FromShader( "shaders/blob.shader" );
		var mesh = new Mesh( material );

		var vertices = new Vertex[VERTICES];
		var indices = new int[INDICES];

		var center = Vector3.Zero;
		var radius = 1f;

		vertices[0] = new Vertex()
		{
			Position = center,
			TexCoord0 = new Vector4( 0.5f ),
			Color = Color.White
		};

		for ( int i = 1; i <= POINTS; i++ )
		{
			var input = 2f * MathF.PI * i / POINTS;
			var circle = new Vector3( MathF.Cos( input ), MathF.Sin( input ), 0f );

			var position = (center - radius * circle);
			var uv = new Vector2(
				0.5f + (position.x - center.x) / (2f * radius),
				0.5f - (position.y - center.y) / (2f * radius)
			);

			vertices[i] = new Vertex()
			{
				Position = position,
				TexCoord0 = new Vector4( uv ),
				Color = Color.White
			};

			var j = (i - 1) * 3;
			indices[j] = 0;
			indices[j + 1] = i - 1;
			indices[j + 2] = i;
		}

		indices[INDICES - 3] = 0;
		indices[INDICES - 2] = VERTICES - 1;
		indices[INDICES - 1] = 1;

		mesh.CreateVertexBuffer<Vertex>( VERTICES, Vertex.Layout, vertices );
		mesh.CreateIndexBuffer( INDICES, indices );

		return Model.Builder
			.AddMesh( mesh )
			.Create();
	}
}
