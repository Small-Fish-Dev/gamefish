namespace GameFish;

partial class LogicMathEntity : IActivate
{
	/// <summary>
	/// If enabled: ignores any value passed in when
	/// activating this and only ever uses the default.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Ignore Input" )]
	[Order( MATH_LOGIC_ORDER )]
	[Feature( MATH ), Group( LOGIC )]
	public virtual bool IgnoreActivationValue { get; set; } = false;

	/// <summary>
	/// Execute this logic before attempting any kind of math.
	/// <br /> <br />
	/// <b> NOTE: </b> Activations pass the CURRENT value of the target BEFORE operation.
	/// </summary>
	[Property]
	[Title( "Before Math" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( MATH ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> PreOperationLogic { get; set; } = [];

	/// <summary>
	/// Execute this logic after trying to operate on the target value.
	/// <br /> <br />
	/// <b> NOTE: </b> Activations pass the RESULTING value of the target AFTER operation.
	/// </summary>
	[Property]
	[Title( "After Math" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( MATH ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> PostOperationLogic { get; set; } = [];

	/// <summary>
	/// Execute this logic upon successfully changing a target's value.
	/// <br /> <br />
	/// <b> NOTE: </b> Activations pass the resulting value.
	/// </summary>
	[Property]
	[Title( "On Success" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( MATH ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> OnSuccessLogic { get; set; } = [];

	/// <summary>
	/// Execute this logic upon failure to change a target's value.
	/// <br /> <br />
	/// <b> NOTE: </b> Activations pass the resulting value.
	/// </summary>
	[Property]
	[Title( "On Failure" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( MATH ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> OnFailureLogic { get; set; } = [];

	public virtual bool CanActivate( object source )
		=> !GameObject.IsDestroyed();

	public bool TryActivate( object source = null, object value = null )
	{
		if ( !CanActivate( source ) )
			return false;

		if ( IgnoreActivationValue )
			return TryOperate( DefaultValue );

		value ??= DefaultValue;

		return value switch
		{
			int iValue => TryOperate( iValue ),
			float fValue => TryOperate( fValue ),
			double dValue => TryOperate( (float)dValue ),
			_ => false
		};
	}
}
