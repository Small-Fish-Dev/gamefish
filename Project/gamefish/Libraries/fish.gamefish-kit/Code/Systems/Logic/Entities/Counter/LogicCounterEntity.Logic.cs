namespace GameFish;

partial class LogicCounterEntity : IActivate, ILogicValue
{
	/// <summary>
	/// Execute this logic when the count changes.
	/// <br /> <br />
	/// <b> NOTE: </b> Activations pass this counter's value.
	/// </summary>
	[Property]
	[Title( "On Count" )]
	[Order( COUNT_FUNCTIONS_ORDER )]
	[Feature( COUNT ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> OnCountLogic { get; set; } = [];

	/// <summary>
	/// Execute this logic when the minimum is reached.
	/// <br /> <br />
	/// <b> NOTE: </b> Activations pass this counter's value.
	/// </summary>
	[Property]
	[Title( "On Min" )]
	[Order( COUNT_FUNCTIONS_ORDER )]
	[Feature( COUNT ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> OnMinLogic { get; set; } = [];

	/// <summary>
	/// Execute this logic when the count changes.
	/// <br /> <br />
	/// <b> NOTE: </b> Activations pass this counter's value.
	/// </summary>
	[Property]
	[Title( "On Max" )]
	[Order( COUNT_FUNCTIONS_ORDER )]
	[Feature( COUNT ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> OnMaxLogic { get; set; } = [];

	public float Value => Count;

	public bool TrySetValue( in float value, out float result )
	{
		if ( !TrySetCount( in value ) )
		{
			result = Count;
			return false;
		}

		result = Count;
		return true;
	}

	public virtual bool CanActivate( object source )
		=> !GameObject.IsDestroyed();

	public bool TryActivate( object source = null, object value = null )
	{
		if ( !CanActivate( source ) )
			return false;

		value ??= DefaultModify;

		return value switch
		{
			int iValue => TryModifyCount( iValue ),
			float fValue => TryModifyCount( fValue ),
			double dValue => TryModifyCount( (float)dValue ),
			_ => false
		};
	}
}
