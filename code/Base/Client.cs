namespace Blobs;

public sealed class Client
	: Component
{
	public static NetList<Client> All => GameManager.Instance?.Clients;
	public static Client Local => All?.FirstOrDefault( cl => cl.ConnectionId == Connection.Local?.Id );

	public Connection Connection => Connection.Find( _connectionId );

	[Sync( SyncFlags.FromHost )]
	public Guid ConnectionId
	{
		get => _connectionId;
		set 
		{
			_connectionId = value;
		}
	}
	private Guid _connectionId;

	[Sync( SyncFlags.FromHost )]
	public Pawn Pawn { get; set; }

	public SteamId SteamId => Connection?.SteamId ?? default;
	public string DisplayName => Connection?.DisplayName ?? string.Empty;

	public static Client Create( Connection connection )
	{
		var scene = Game.ActiveScene;
		Assert.True( scene.IsValid(), "ActiveScene not valid, something fucked up. " );
		Assert.True( Networking.IsHost, "Tried to create client as non-host." );
		Assert.True( connection is not null, "Tried to create client for non-existing Connection." );

		using ( scene.Push() )
		{
			var obj = new GameObject( true, $"client: {connection.DisplayName}" );
			obj.Flags |= GameObjectFlags.Hidden;

			var client = obj.Components.Create<Client>();
			client.ConnectionId = connection.Id;

			All.Add( client );

			obj.SetupNetworking( connection, OwnerTransfer.Fixed, NetworkOrphaned.Destroy );

			return client;
		}
	}

	public Pawn CreatePawn( string path, bool setupNetworking = true )
	{
		Assert.True( Networking.IsHost, "Tried to create pawn as non-host." );
		Assert.False( string.IsNullOrEmpty( path ), "Cannot create pawn from empty path." );

		var prefab = GameObject.GetPrefab( path );
		Assert.True( prefab is not null, "Path didn't contain valid prefab." );

		var obj = prefab.Clone();
		var pawn = obj.Components.Get<Pawn>( FindMode.EverythingInSelf );

		if ( !pawn.IsValid() )
		{
			obj.Destroy();
			throw new Exception( $"Prefab \"{path}\" did not contain a Pawn component at root." );
		}

		Pawn = pawn;
		pawn.OnHostSpawned();

		if ( setupNetworking )
			obj.SetupNetworking( Connection, OwnerTransfer.Fixed, NetworkOrphaned.Destroy );
		
		return pawn;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( All is not null && All.Contains( this ) )
			All.Remove( this );
	}
}
