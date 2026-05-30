namespace GameFish;

partial class LogicCaseEntity : IActivate
{
	/// <summary>
	/// Execute this logic after any condition was satisfied.
	/// </summary>
	[Property]
	[Title( "On Success" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> OnSuccessLogic { get; set; } = [];

	/// <summary>
	/// Execute this logic when no conditions were satisfied by an input.
	/// </summary>
	[Property]
	[Title( "On Failure" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	public virtual List<LogicAction> OnFailureLogic { get; set; } = [];

	public virtual bool CanActivate( object source )
		=> !GameObject.IsDestroyed();

	public bool TryActivate( object source = null, object value = null )
	{
		if ( !CanActivate( source ) )
			return false;

		return TryExecute( value );
	}
}
