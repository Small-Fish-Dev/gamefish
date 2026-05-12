namespace GameFish;

partial class Pawn
{
	public virtual void AddRecoil( in Rotation rRecoil )
		=> View?.AddRecoil( in rRecoil );

	public virtual void ResetRecoil()
		=> View?.ResetRecoil();

	public void RpcHostAddRecoil( Rotation rRecoil )
		=> View?.RpcHostAddRecoil( rRecoil );

	public void RpcHostResetRecoil()
		=> View?.RpcHostResetRecoil();
}
