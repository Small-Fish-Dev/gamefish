using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Stores a value that's kept within a range. Runs logic whenever it hits its min/max.
/// <br /> <br />
/// <b> LOGIC: </b> Passes along its value whenever activating targets.
/// <br /> <br />
/// <b> LOGIC: </b> Activating this with a number specified modifies the count by that value.
/// <br /> <br />
/// <b> LOGIC: </b> Activating this without a number
/// modifies the count by "<b>Modify</b>"(if defined).
/// <code> math_counter </code>
/// </summary>
[Icon( "looks_one" )]
[EditorHandle( Icon = "looks_one" )]
public partial class LogicCounterEntity : LogicEntity
{
	protected const int COUNT_ORDER = LOGIC_ORDER - 1000;

	protected const int COUNT_DEBUG_ORDER = COUNT_ORDER - 50;

	protected const int COUNT_RANGE_ORDER = COUNT_ORDER + 10;
	protected const int COUNT_DEFAULT_ORDER = COUNT_ORDER + 20;
	protected const int COUNT_DISPLAY_ORDER = COUNT_ORDER + 50;
	protected const int COUNT_FUNCTIONS_ORDER = COUNT_ORDER + 100;

	/// <summary>
	/// If enabled: print when the count changes.
	/// </summary>
	[Property]
	[Title( "Logging (count)" )]
	[Order( COUNT_DEBUG_ORDER )]
	[Feature( COUNT ), Group( DEBUG )]
	public bool DebugLogCount { get; set; } = false;

	/// <summary>
	/// The current value.
	/// </summary>
	[Property]
	[Title( "Count" )]
	[JsonIgnore, ReadOnly]
	[Order( COUNT_DEBUG_ORDER )]
	[Feature( COUNT ), Group( DEBUG )]
	protected float InspectorCount => Count;

	/// <summary>
	/// The starting value.
	/// </summary>
	[Property]
	[InlineEditor]
	[Feature( COUNT )]
	[Order( COUNT_ORDER )]
	[Title( "Initial Value" )]
	public float DefaultValue
	{
		get => _defaultValue; set
		{
			_defaultValue = value;

			if ( InEditor )
				OnSetCount( in value );
		}
	}

	protected float _defaultValue = 0f;

	/// <summary>
	/// The number to operate with upon activation if one wasn't specified.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Modify" )]
	[Order( COUNT_DEFAULT_ORDER )]
	[Feature( COUNT ), Group( DEFAULT )]
	public float DefaultModify { get; set; } = 1f;

	/// <summary>
	/// What kind of math to use with the number we get activated with.
	/// </summary>
	[Property]
	[InlineEditor]
	[EnumButtonGroup]
	[Title( "Operation" )]
	[Order( COUNT_DEFAULT_ORDER )]
	[Feature( COUNT ), Group( DEFAULT )]
	public NumberOperation ModifyOperation { get; set; } = NumberOperation.Add;

	/// <summary>
	/// The number rounding operation to use when the value is changed.
	/// </summary>
	[Property]
	[InlineEditor]
	[EnumButtonGroup]
	[Feature( COUNT )]
	[Title( "Rounding" )]
	[Order( COUNT_ORDER )]
	public NumberRounding Rounding { get; set; } = NumberRounding.Floor;

	/// <summary>
	/// Count must be within this range.
	/// </summary>
	[Property]
	[Title( "Range" )]
	[Order( COUNT_RANGE_ORDER )]
	[Feature( COUNT ), Group( RANGE )]
	[InlineEditor( Label = false ), WideMode( HasLabel = false )]
	public FloatRange ValueRange { get; set; } = new( 0, 100 );

	/// <summary>
	/// The text renderer to "write" the count to.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Text" )]
	[Order( COUNT_DISPLAY_ORDER )]
	[Feature( COUNT ), Group( DISPLAY )]
	public TextRenderer TextRenderer { get; set; }

	[Sync]
	public float Count
	{
		get => _count ?? GetDefaultValue();

		protected set
		{
			if ( _count is float fValue )
				if ( fValue == value )
					return;

			_count = value;
			OnSetCount( in value );
		}
	}

	protected float? _count;

	protected virtual void OnSetCount( in float value )
	{
		if ( DebugLogCount )
			this.Log( $"Count: {value}" );

		if ( TextRenderer.IsValid( out var text ) )
			text.Text = value.Round( Rounding ).ToString();
	}

	public virtual float GetDefaultValue()
		=> DefaultValue.Round( Rounding ).Clamp( ValueRange );

	public virtual bool TrySetCount( in float value )
	{
		var prevCount = Count;

		Count = value.Round( Rounding ).Clamp( ValueRange );

		if ( Count == prevCount )
			return false;

		// Let other logic stuff receive the count as input.
		LogicAction.TryExecute( OnCountLogic, this, Count );

		if ( Count <= ValueRange.Min )
			LogicAction.TryExecute( OnMinLogic, this, Count );
		else if ( Count >= ValueRange.Max )
			LogicAction.TryExecute( OnMaxLogic, this, Count );

		return true;
	}

	public virtual bool TryModifyCount( in float value )
	{
		if ( IsProxy )
			return false;

		var newCount = Count.Operate( value, ModifyOperation );

		return TrySetCount( in newCount );
	}
}
