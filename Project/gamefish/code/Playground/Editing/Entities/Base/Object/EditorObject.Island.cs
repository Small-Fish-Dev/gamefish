namespace Playground;

partial class EditorObject
{
	/// <summary>
	/// The parent entity that this is "snapped" to.
	/// </summary>
	[Property]
	[Title( "Island" )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Order( EDITOR_ORDER )]
	public EditorIsland Island
	{
		get => _objIsland;
		protected set
		{
			if ( _objIsland == value )
				return;

			var old = _objIsland;
			_objIsland = value;

			OnSetIsland( _objIsland, old );
		}
	}

	protected EditorIsland _objIsland = null;

	protected virtual void OnSetIsland( EditorIsland newIsland, EditorIsland oldIsland )
	{
		if ( oldIsland.IsValid() )
			this.Log( $"group new:[{newIsland}] old:[{oldIsland}]" );

		if ( newIsland.IsValid() )
			newIsland.OnObjectAdded( this );
	}

	protected void RefreshIsland( GameObject parent = null )
	{
		parent ??= GameObject;

		if ( !parent.IsValid() )
		{
			Island = null;
			return;
		}

		Island = parent?.Components?.Get<EditorIsland>( FindMode.Enabled | FindMode.InAncestors );

		if ( Island.IsValid() )
			PreventMotion();
	}

	protected void PreventMotion()
	{
		if ( !GameObject.IsValid() )
			return;

		var bodies = Components.GetAll<Rigidbody>( FindMode.EverythingInSelfAndDescendants );

		if ( !bodies.Any() )
			return;

		foreach ( var rb in bodies.ToArray() )
			rb.Enabled = false;
	}
}
