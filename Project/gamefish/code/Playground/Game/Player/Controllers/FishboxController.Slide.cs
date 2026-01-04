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
		ValueRange = new( 0f, 5000f )
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
}
