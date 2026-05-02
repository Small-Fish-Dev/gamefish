using GameFish;

namespace Fishbox;

/// <summary>
/// A badass player controller. <br />
/// Shout out to <b>Cyber-Ninja: Ascension</b>.
/// </summary>
[Icon( "sports_esports" )]
public partial class ShooterController : FirstPersonController
{
	protected const string BADASS = "😎 Badass";
	protected const int BADASS_ORDER = PAWN_ORDER - 1000;

	/// <summary>
	/// The radius of the cylinder and head.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS )]
	public float Radius { get; set; } = 16f;

	protected float Height => Radius * 2f;

	protected Vector3 Up => WorldRotation.Up;
	protected Vector3 Down => WorldRotation.Down;

	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		base.Simulate( deltaTime, isFixedUpdate );

		if ( ShouldJump() )
			Jump();
	}

	protected override void PostMove( in float deltaTime )
	{
		base.PostMove( deltaTime );

		IsGrounded = Mover?.IsGrounded is true;

		// Gravity.
		if ( !IsGrounded )
			Velocity += Gravity * deltaTime;
	}

	protected override void Move( in float deltaTime )
	{
		PreMove( in deltaTime );

		MoveBy( Velocity * deltaTime );

		PostMove( in deltaTime );
	}

	public override SceneTrace BuildTrace()
	{
		if ( !Scene.IsValid() )
			return default;

		return Scene.Trace
			.Cylinder( Height, Radius )
			.IgnoreGameObjectHierarchy( GameObject );
	}
}
