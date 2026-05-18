namespace GameFish;

partial class Pawn
{
	public virtual void AddRecoil( in Rotation rRecoil )
		=> View?.AddRecoil( in rRecoil );

	public virtual void ResetRecoil()
		=> View?.ResetRecoil();
}
