namespace GameFish;

partial class LogicMathEntity : IActivate
{
	/// <summary>
	/// Execute this logic upon successfully changing a target's value.
	/// <br /> <br />
	/// <b> NOTE: </b> Activations pass the resulting value.
	/// </summary>
	[Property]
	[Feature( LOGIC )]
	[Order( LOGIC_ORDER )]
	[Title( "On Operate" )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> OnOperateLogic { get; set; } = [];

	public virtual bool CanActivate( object source )
		=> !GameObject.IsDestroyed();

	public bool TryActivate( object source = null, object value = null )
	{
		if ( !CanActivate( source ) )
			return false;

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
