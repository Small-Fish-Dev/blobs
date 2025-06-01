namespace Blobs;

public sealed class FoodSpawner
	: Component
{
	[Property]
	public int MaxFood { get; set; } = 500;

	[Property]
	public RangedFloat FoodSizeRange { get; set; } = new RangedFloat( 4, 15 );

	[Property]
	public float SpawnCooldown { get; set; } = 1f;

	private TimeSince _lastSpawned;

	public void Spawn()
	{
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
