namespace Playground;

partial class Editor
{
	/// <summary>
	/// How close must things be to display their helpers?
	/// </summary>
	[Property]
	[Title( "Helper Radius" )]
	[Feature( EDITOR ), Group( SETTINGS )]
	public float DrawNearbyHelperRadius { get; set; } = 2048f;

	public HighlightOutline Outline => _outline ??= _outline.GetCached( GameObject );
	protected HighlightOutline _outline;

	protected void UpdateTargetOutline()
	{
		if ( !Tool.IsValid() || !Tool.HasTarget )
		{
			SetOutlineTarget( null );
			return;
		}

		SetOutlineTarget( Tool.TargetObject );
	}

	protected void SetOutlineTarget( GameObject target )
	{
		if ( !Outline.IsValid() )
			return;

		var hasTarget = target.IsValid();

		Outline.Enabled = hasTarget;

		if ( !hasTarget )
		{
			Outline.Targets = null;
			return;
		}

		var renderers = target.Components
			.GetAll<Renderer>( FindMode.EnabledInSelfAndDescendants );

		Outline.Targets = [.. renderers];
	}

	protected void RenderNearbyHelpers()
	{
		if ( !IsOpen )
			return;

		if ( !Scene.IsValid() || !Scene.Camera.IsValid() )
			return;

		var origin = Scene.Camera.WorldPosition;
		var radius = DrawNearbyHelperRadius;

		if ( !ITransform.IsValid( origin ) || !ITransform.IsValid( radius ) )
			return;

		foreach ( var ent in Scene.GetAll<EditorObject>() )
			if ( origin.Distance( ent.Center ) < radius )
				ent.RenderHelpers();
	}
}
