namespace GameFish;

/// <summary>
/// Lets you paint colorables randomly.
/// </summary>
[Icon( "format_paint" )]
public class Painter : Entity, IActivate
{
	protected const int ORDER_COLOR = DEFAULT_ORDER - 1000;
	protected const int ORDER_COLOR_TARGETS = ORDER_COLOR - 10;

	protected const int ORDER_HSV = ORDER_COLOR + 2;
	protected const int ORDER_LINEAR = ORDER_COLOR + 1;
	protected const int ORDER_PALETTE = ORDER_COLOR + 3;

	[DefaultValue( HSV )]
	public enum PickMethod
	{
		/// <summary>
		/// Between one color to another.
		/// </summary>
		[Icon( "trending_flat" )]
		Linear,

		/// <summary>
		/// Hue, saturation and value ranges.
		/// </summary>
		[Icon( "looks" )]
		HSV,

		/// <summary>
		/// From a set of colors.
		/// </summary>
		[Icon( "palette" )]
		Palette,
	}

	/// <summary>
	/// The components implementing <see cref="Component.ITintable"/> to be colored.
	/// </summary>
	[Property]
	[WideMode( HasLabel = false )]
	[Order( ORDER_COLOR_TARGETS )]
	[Feature( COLOR ), Group( TARGETS )]
	public List<ITintable> Targets { get; set; } = [null];

	/// <summary>
	/// If true: color will be selected automatically.
	/// </summary>
	[Property]
	[Title( "Auto-Paint" )]
	[Feature( COLOR ), Order( ORDER_COLOR )]
	public bool AutoPaint { get; set; } = true;

	[Property]
	[Feature( COLOR ), Order( ORDER_COLOR )]
	public PickMethod PickingMethod { get; set; } = PickMethod.HSV;

	[Property]
	[Feature( COLOR ), Order( ORDER_LINEAR )]
	[ToggleGroup( nameof( UsingLinear ), Label = LINEAR )]
	public bool UsingLinear => PickingMethod is PickMethod.Linear;

	[Property]
	[Title( "A" )]
	[ToggleGroup( nameof( UsingLinear ) )]
	[Feature( COLOR ), Order( ORDER_LINEAR )]
	public Color LinearColorA { get; set; } = Color.White;

	[Property]
	[Title( "B" )]
	[ToggleGroup( nameof( UsingLinear ) )]
	[Feature( COLOR ), Order( ORDER_LINEAR )]
	public Color LinearColorB { get; set; } = Color.Black;

	[Property]
	[Feature( COLOR ), Order( ORDER_HSV )]
	[ToggleGroup( nameof( UsingHSV ), Label = HSV )]
	public bool UsingHSV => PickingMethod is PickMethod.HSV;

	/// <summary>
	/// The min/max hue. <br />
	/// <b> RANGE: </b> <c>0</c> to <c>360</c>.
	/// </summary>
	[Property]
	[Title( "Hue" )]
	[ToggleGroup( nameof( UsingHSV ) )]
	[Feature( COLOR ), Order( ORDER_HSV )]
	public FloatRange HueRange { get; set; } = new( 0f, 360f );

	/// <summary>
	/// The min/max saturation. <br />
	/// <b> RANGE: </b> <c>0</c> to <c>1</c>.
	/// </summary>
	[Property]
	[Title( "Saturation" )]
	[ToggleGroup( nameof( UsingHSV ) )]
	[Feature( COLOR ), Order( ORDER_HSV )]
	public FloatRange SaturationRange { get; set; } = new( 0.65f, 0.9f );

	/// <summary>
	/// The min/max value. <br />
	/// <b> RANGE: </b> <c>0</c> to <c>1</c>.
	/// </summary>
	[Property]
	[Title( "Value" )]
	[ToggleGroup( nameof( UsingHSV ) )]
	[Feature( COLOR ), Order( ORDER_HSV )]
	public FloatRange ValueRange { get; set; } = new( 0.7f, 0.8f );

	[Property]
	[Feature( COLOR ), Order( ORDER_PALETTE )]
	[ToggleGroup( nameof( UsingPalette ), Label = PALETTE )]
	public bool UsingPalette => PickingMethod is PickMethod.Palette;

	[Property]
	[WideMode( HasLabel = false )]
	[ToggleGroup( nameof( UsingPalette ) )]
	[Feature( COLOR ), Order( ORDER_PALETTE )]
	public List<Color> Palette { get; set; } =
	[
		Color.White, Color.Gray,
		Color.Blue, Color.Cyan, Color.Green,
		Color.Magenta, Color.Orange, Color.Red, Color.Yellow
	];

	/// <summary>
	/// The color we're painting with. Automatically applies to targets when set.
	/// </summary>
	[Sync]
	public Color? PaintColor
	{
		get => _color;
		set
		{
			_color = value;
			OnSetColor( value );
		}
	}

	protected Color? _color;

	protected virtual void OnSetColor( in Color? color )
	{
		if ( color is Color c )
			TryColor( in c );
	}

	protected override void OnAwake()
	{
		base.OnAwake();

		if ( AutoPaint )
			PickRandomColor();
	}

	[Button( "Paint" )]
	[Order( ORDER_COLOR_TARGETS + 1 )]
	[Feature( COLOR ), Group( TARGETS )]
	protected void PickRandomColor()
	{
		if ( IsProxy )
			return;

		if ( GetRandomColor() is Color c )
			PaintColor = c;
	}

	public Color? GetRandomColor()
	{
		if ( PickingMethod is PickMethod.Linear )
			return LinearColorA.LerpTo( LinearColorB, Random.Float() );

		if ( PickingMethod is PickMethod.HSV )
		{
			var hue = Random.From( HueRange );
			var sat = Random.From( SaturationRange ).Clamp( 0f, 1f );
			var val = Random.From( ValueRange ).Clamp( 0f, 1f );

			return new ColorHsv( hue, sat, val );
		}

		if ( PickingMethod is PickMethod.Palette )
			return Palette?.PickRandom();

		return null;
	}

	/// <summary>
	/// Tries to color all listed targets.
	/// </summary>
	/// <returns> If any of the targets were colored. </returns>
	public virtual bool TryColor( in Color color )
	{
		if ( Targets is null )
			return false;

		var hasColored = false;

		foreach ( var tgt in Targets )
			if ( TryColorTarget( tgt, color ) )
				hasColored = true;

		return hasColored;
	}

	/// <summary>
	/// Tries to color a specific target.
	/// </summary>
	/// <returns> If that target was colored. </returns>
	protected virtual bool TryColorTarget( ITintable target, in Color color )
	{
		if ( target is not Component c || !c.IsValid() )
			return false;

		target.Color = color;
		return true;
	}

	public bool TryActivate( object source = null, object value = null )
	{
		if ( (PaintColor ?? GetRandomColor()) is not Color color )
			return false;

		if ( value is ITintable target )
			return TryColorTarget( target, color );

		return TryColor( color );
	}
}
