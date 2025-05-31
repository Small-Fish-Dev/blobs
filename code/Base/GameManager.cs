namespace Blobs;

public sealed class GameManager 
	: Component, Component.INetworkListener
{
	public static GameManager Instance { get; private set; }
	public static (Vector2 Mins, Vector2 Maxs) Bounds => (Instance?.Mins ?? default, Instance?.Maxs ?? default);

	[Property, Category( "Settings" )]
	public PrefabFile DefaultPawn { get; set; }

	[Property, Category( "Settings" )]
	public Vector2 Mins { get; set; } = new Vector2( -100, -100 );

	[Property, Category( "Settings" )]
	public Vector2 Maxs { get; set; } = new Vector2( 100, 100 );

	[Sync]
	public NetList<Client> Clients { get; private set; } = new();

	protected override void OnStart()
	{
		base.OnStart();

		Instance = this;
	
		// Create a lobby if networking not active.
		if ( !Networking.IsActive )
		{
			var lobbySettings = new LobbyConfig()
			{
				Name = $"blobs lobby : -)",
				DestroyWhenHostLeaves = false,
				AutoSwitchToBestHost = false,
				Privacy = LobbyPrivacy.Public
			};

			Networking.CreateLobby( lobbySettings );
		}
	}

	void INetworkListener.OnActive( Connection connection )
	{
		Assert.True( connection is not null, "Something fucked up? Connection was null OnActive. " );

		var client = Client.Create( connection );
		var pawn = client.CreatePawn( DefaultPawn?.ResourcePath );
	}

	protected override void DrawGizmos()
	{
		var bbox = new BBox( new Vector3( Mins ), new Vector3( Maxs ) );
		Gizmo.Draw.LineBBox( bbox );
	}
}
