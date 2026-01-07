using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController
{
	public const string SURFACE_WATER = "water";
	public const string TAG_SLIPPERY = "slippery";

	/// <summary>
	/// The angle where a surface is a slope(when not sliding).
	/// </summary>
	[Property]
	[Feature( PLAYER ), Group( SLIDING ), Order( DEFAULTS_ORDER )]
	[Range( 0f, 90f, clamped: false ), Step( 1f )]
	public float SlopeAngle { get; set; } = 35f;

	[Property]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	[ToggleGroup( nameof( AllowSliding ), Label = SLIDING )]
	public bool AllowSliding { get; set; } = true;

	/// <summary>
	/// Move speed while sliding. Can't accelerate beyond the current velocity.
	/// </summary>
	[Property]
	[Title( "Acceleration (Sliding)" )]
	[Range( 0f, 2000f, clamped: false )]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public float SlideAcceleration { get; set; } = 1200f;

	/// <summary>
	/// Must be going this speed to start sliding while ducked on the ground.
	/// </summary>
	[Property]
	[Title( "Minimum Speed" )]
	[Range( 0f, 1000f, clamped: false )]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public float SlideMinSpeed { get; set; } = 400f;

	/// <summary>
	/// Stop an active slide while under this speed.
	/// </summary>
	[Property]
	[Title( "Stop Speed" )]
	[Range( 0f, 500f, clamped: false )]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public float SlideStopSpeed { get; set; } = 200f;

	/// <summary>
	/// Limit of friction while on a tall slope or slippery surface.
	/// </summary>
	[Property]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	[Range( 0f, 1f, clamped: false ), Step( 0.01f )]
	public float SlippingFriction { get; set; } = 0.15f;

	[Property]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public Curve SlopeSpeed { get; set; } = new( new( 0f, 0f ), new( 1f, 1f ) )
	{
		TimeRange = new( 0f, 90f ),
		ValueRange = new( 0f, 700f )
	};

	[Property]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public Curve SlopeFriction { get; set; } = new( new( 0f, 1f ), new( 1f, 0f ) )
	{
		TimeRange = new( 0f, 90f ),
		ValueRange = new( 0f, 900f )
	};

	[Sync]
	[Property]
	[ShowIf( nameof( InGame ), true )]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public bool IsSliding { get; set; }

	/// <summary>
	/// Is the player on too steep a slope?
	/// </summary>
	[Sync] public bool IsSlipping { get; set; }

	protected virtual bool IsSlippery( in SceneTraceResult tr )
	{
		if ( tr.StartedSolid )
			return false;

		if ( !tr.Hit || tr.Surface is not Surface surface )
			return false;

		const float slipFriction = 0.2f;

		return surface.Friction <= slipFriction
			|| surface.ResourceName == SURFACE_WATER
			|| (surface.Tags?.Contains( TAG_SLIPPERY ) ?? false);
	}

	protected virtual void DoSliding( in float deltaTime )
	{
		if ( !AllowSliding )
			goto NotSliding;

		var up = WorldRotation.Up;
		var slopeAngle = up.Angle( GroundNormal );

		IsSlipping = IsGrounded && (slopeAngle > SlopeAngle || IsSlippery( GroundTrace ));

		if ( !IsSliding )
		{
			if ( IsSlipping )
			{
				// Always slide when slipping on a slope.
				IsSliding = true;
			}
			else if ( IsGrounded )
			{
				if ( IsDucking && Velocity.Length >= SlideMinSpeed )
				{
					// Start a slide with enough ground speed while crouching.
					IsSliding = true;
				}
			}
		}

		if ( IsSliding )
		{
			if ( IsGrounded && GroundNormal != Vector3.Zero )
			{
				// Slope Sliding
				var slideSpeed = SlopeSpeed.Evaluate( slopeAngle );
				var slideDir = Vector3.VectorPlaneProject( -up, GroundNormal );

				Velocity += slideDir * slideSpeed * deltaTime;

				// Slide Movement
				var maxSlide = Velocity.Length;
				var slideMove = WishVelocity.ProjectAndScale( GroundNormal );

				// Mitigate sliding in circles.
				var slideDot = slideMove.Normal.Dot( Velocity.Normal ).Abs();
				slideDot = slideDot.Remap( 0f, 0.5f, 0.6f, 1f ); // a bit over 45 degrees
				slideMove *= slideDot;

				if ( slopeAngle < SlopeAngle )
				{
					// Don't move faster than we can walk while slipping.
					var slopeMoveSpeed = (SlideAcceleration * slopeAngle.Remap( SlopeAngle, 0f ))
						.Min( MoveSpeed );

					maxSlide = maxSlide.Max( slopeMoveSpeed );
				}

				Velocity = (Velocity + (slideMove * deltaTime))
					.ClampLength( maxSlide );

				// Slope Friction
				var frictionScale = GroundCollider?.Friction ?? 1f;

				if ( IsSlipping )
					frictionScale = frictionScale.Min( SlippingFriction );

				var slideFriction = SlopeFriction.Evaluate( slopeAngle ) * frictionScale;

				var finalSpeed = Velocity.Length;
				finalSpeed -= (slideFriction * deltaTime).Positive();

				Velocity = Velocity.Normal * finalSpeed.Positive();

				// Stop sliding if going too slow.
				if ( !IsSlipping && slopeAngle <= SlopeAngle )
					if ( Velocity.Length <= SlideStopSpeed )
						goto NotSliding;
			}

			return;
		}

		NotSliding:

		if ( IsSliding )
			IsSliding = false;

		// MaxGroundAngle = SlopeAngle;
		// StickToPlatforms = true;
		// GroundStickEnabled = true;

		// GroundAcceleration = Acceleration * 420f;
		// GroundDeceleration = Friction.Value * 420f;
	}
}
