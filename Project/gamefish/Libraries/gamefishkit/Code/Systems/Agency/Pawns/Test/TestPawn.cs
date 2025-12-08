namespace GameFish;

public partial class TestPawn : Pawn
{
	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		if ( Input.Pressed( "Jump" ) )
			this.Log( "jumped" );
	}
}
