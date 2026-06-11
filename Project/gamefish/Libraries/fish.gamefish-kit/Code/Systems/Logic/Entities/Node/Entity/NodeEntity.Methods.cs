namespace GameFish.Nodes;

partial class NodeEntity
{
	protected const string CUSTOM = "📝 Custom";

	protected const string METHOD_INFO = "These methods can be ran using their names.";

	/// <summary>
	/// Logical functions that can be ran by other entities
	/// using string pattern matching with their names.
	/// </summary>
	[Property]
	[Group( CUSTOM )]
	[Title( "Methods" )]
	[Order( METHODS_CUSTOM_ORDER )]
	[InfoBox( METHOD_INFO, Tint = EditorTint.Blue )]
	[Feature( METHODS, Description = METHODS_DESCRIPTION )]
	[InlineEditor( Label = false ), WideMode( HasLabel = false )]
	public virtual List<NodeMethod> Methods { get; set; } = [];

	public virtual bool TryRunMethod( StringMatch sm )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		if ( Methods is null || Methods.Count <= 0 )
			return false;

		if ( !sm.IsValid )
			return false;

		int runCount = 0;

		foreach ( var m in Methods )
		{
			if ( m.Name.IsBlank() )
				continue;

			if ( sm.Matches( m.Name ) )
				if ( m.TryRun( this ) )
					runCount++;
		}

		return runCount > 0;
	}
}
