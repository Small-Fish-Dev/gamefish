namespace GameFish;

public partial class TestPawn : BasePawn
{
	public override void FrameOperate( in float deltaTime )
	{
		base.FrameOperate( deltaTime );

		if ( Input.Pressed( "Jump" ) )
			this.Log( "jumped" );
	}
}
