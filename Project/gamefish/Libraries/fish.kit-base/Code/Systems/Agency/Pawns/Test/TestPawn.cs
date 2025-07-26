namespace GameFish;

public partial class TestPawn : BasePawn
{
	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		if ( Input.Pressed( "Jump" ) )
			this.Log( "jumped" );
	}
}
