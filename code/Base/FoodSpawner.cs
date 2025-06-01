namespace Blobs;

public sealed class FoodSpawner
	: Component
{
	[Property]
	public int MaxFood { get; set; } = 500;

	[Property]
	public GameObject FoodPrefab { get; set; }

	[Property]
	public RangedFloat FoodSizeRange { get; set; } = new RangedFloat( 4, 15 );

	[Property]
	public float SpawnCooldown { get; set; } = 1f;

	private TimeSince _lastSpawned;

	public void Spawn()
	{
		if ( !FoodPrefab.IsValid() ) return;
		if ( EdibleBlob.Count > MaxFood ) return;
		var bounds = GameManager.Bounds;
		var position = new Vector2(
			Game.Random.Float( bounds.Mins.x, bounds.Maxs.x ),
			Game.Random.Float( bounds.Mins.y, bounds.Maxs.y )
		);

		var obj = FoodPrefab.Clone();
		var blob = obj.Components.Get<Blob>( FindMode.EverythingInSelf );
		blob.Size = Game.Random.Int( 
			(int)FoodSizeRange.Min,
			(int)FoodSizeRange.Max 
		);
		blob.WorldPosition = position.Extrude();
		blob.LerpStart = true;
		
		obj.SetupNetworking( null, OwnerTransfer.Fixed, NetworkOrphaned.Host );
	}

	protected override void OnStart()
	{
		base.OnStart();

		EdibleBlob.Count = 0;

		if ( IsProxy )
			return;

		for ( int i = 0; i < 100; i++ )
			Spawn();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( _lastSpawned > SpawnCooldown )
		{
			Spawn();
			_lastSpawned = 0f;
		}
	}
}
