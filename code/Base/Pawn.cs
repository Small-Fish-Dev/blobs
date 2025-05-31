namespace Blobs;

public abstract class Pawn
	: Component
{
	public Client Client => Client.All?.FirstOrDefault( cl => cl.IsValid() && cl.Pawn == this );
}
