namespace Blobs;

public sealed class GameManager 
	: Component, Component.INetworkListener
{
	public static GameManager Instance { get; private set; }
	public static (Vector2 Mins, Vector2 Maxs) Bounds => (Instance?.Mins ?? default, Instance?.Maxs ?? default);

	[Property, Category( "Settings" )]
	public PrefabFile DefaultPawn { get; set; }

	[Property, Category( "Settings" )]
	public ScreenPanel ScreenPanel { get; set; }

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

		connection.CanSpawnObjects = false;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		var camera = Scene.Camera;
		if ( camera.IsValid() && ScreenPanel.IsValid() )
			ScreenPanel.TargetCamera = camera.IsMainCamera ? null : camera;
	}

	protected override void DrawGizmos()
	{
		var bbox = new BBox( new Vector3( Mins ), new Vector3( Maxs ) );
		Gizmo.Draw.LineBBox( bbox );
	}

	[Rpc.Host( NetFlags.SendImmediate | NetFlags.Reliable )]
	public static void Respawn()
	{
		var callerId = Rpc.CallerId;
		var client = Client.All.FirstOrDefault( client => client.ConnectionId == callerId );
		if ( !client.IsValid() )
			return;

		if ( client.Pawn.IsValid() )
			return;

		var pawn = client.CreatePawn( Instance?.DefaultPawn?.ResourcePath );
	}
}
