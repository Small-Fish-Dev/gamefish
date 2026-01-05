using GameFish;
using ShrimpleCharacterController;
using SCC = ShrimpleCharacterController.ShrimpleCharacterController;

namespace Playground;

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
		if ( !tr.Hit || tr.Surface is not Surface surface )
			return false;

		const float slipFriction = 0.2f;

		return surface.Friction <= slipFriction
			|| surface.ResourceName == SURFACE_WATER
			|| (surface.Tags?.Contains( TAG_SLIPPERY ) ?? false);
	}

	protected virtual void DoSliding( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() )
			return;

		if ( !AllowSliding )
			goto NotSliding;

		var up = WorldRotation.Up;
		var slopeAngle = up.Angle( _c.GroundNormal );

		IsSlipping = _c.IsOnGround && (slopeAngle > SlopeAngle || IsSlippery( GroundTrace ));

		if ( !IsSliding )
		{
			if ( IsSlipping )
			{
				// Always slide when slipping on a slope.
				IsSliding = true;
			}
			else if ( _c.IsOnGround )
			{
				if ( IsDucking && _c.Velocity.Length >= SlideMinSpeed )
				{
					// Start a slide with enough ground speed while crouching.
					IsSliding = true;
				}
			}
		}

		if ( IsSliding )
		{
			_c.MaxGroundAngle = 180f;
			_c.StickToPlatforms = false;
			_c.GroundStickEnabled = false;

			_c.GroundAcceleration = 0f;
			_c.GroundDeceleration = 0f;

			if ( _c.IsOnGround && _c.GroundNormal != Vector3.Zero )
			{
				// Slope Sliding
				var slideSpeed = SlopeSpeed.Evaluate( slopeAngle );
				var slideDir = Vector3.VectorPlaneProject( -up, _c.GroundNormal );

				_c.Velocity += slideDir * slideSpeed * deltaTime;

				// Slide Movement
				var maxSlide = _c.Velocity.Length;
				var slideMove = WishVelocity.ProjectAndScale( _c.GroundNormal );

				// Mitigate sliding in circles.
				var slideDot = slideMove.Normal.Dot( _c.Velocity.Normal ).Abs();
				slideDot = slideDot.Remap( 0f, 0.5f, 0.6f, 1f ); // a bit over 45 degrees
				slideMove *= slideDot;

				if ( slopeAngle < SlopeAngle )
				{
					// Don't move faster than we can walk while slipping.
					var slopeMoveSpeed = (SlideAcceleration * slopeAngle.Remap( SlopeAngle, 0f ))
						.Min( MoveSpeed );

					maxSlide = maxSlide.Max( slopeMoveSpeed );
				}

				_c.Velocity = (_c.Velocity + (slideMove * deltaTime))
					.ClampLength( maxSlide );

				// Slope Friction
				var frictionScale = SurfaceFriction;

				if ( IsSlipping )
					frictionScale = frictionScale.Min( SlippingFriction );

				var slideFriction = SlopeFriction.Evaluate( slopeAngle ) * frictionScale;

				var finalSpeed = _c.Velocity.Length;
				finalSpeed -= (slideFriction * deltaTime).Positive();

				_c.Velocity = _c.Velocity.Normal * finalSpeed.Positive();

				// Stop sliding if going too slow.
				if ( !IsSlipping && slopeAngle <= SlopeAngle )
					if ( _c.Velocity.Length <= SlideStopSpeed )
						goto NotSliding;
			}

			return;
		}

		NotSliding:

		if ( IsSliding )
			IsSliding = false;

		_c.MaxGroundAngle = SlopeAngle;
		_c.StickToPlatforms = true;
		_c.GroundStickEnabled = true;

		_c.GroundAcceleration = Acceleration * 420f;
		_c.GroundDeceleration = Friction.Value * 420f;
	}
}
