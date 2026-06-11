using System;

namespace GameFish.Nodes;

partial class NodeEntity
{
	/// <summary>
	/// If enabled: print node/link happenings to the console.
	/// </summary>
	[Property]
	[Title( "Logging (node)" )]
	[Order( NODE_DEBUG_ORDER )]
	[Feature( NODE ), Group( DEBUG )]
	public bool DebugLogNode { get; set; } = false;

	/// <summary>
	/// If enabled: render node/link visuals in play mode.
	/// </summary>
	[Property]
	[Title( "Render (ingame)" )]
	[Order( NODE_DEBUG_ORDER )]
	[Feature( NODE ), Group( DEBUG )]
	public bool DebugRenderInGame { get; set; } = false;

	/// <summary>
	/// The radius of the node's debug rendering sphere.
	/// </summary>
	[Property]
	[Title( "Radius" )]
	[Order( NODE_DEBUG_ORDER )]
	[Feature( NODE ), Group( DEBUG )]
	[Range( 2f, 32f, clamped: false ), Step( 0.5f )]
	public virtual float DebugNodeRadius { get; set; } = 8f;

	/// <summary>
	/// The color to visualize the node with.
	/// </summary>
	[Property]
	[Title( "Color" )]
	[Order( NODE_DEBUG_ORDER )]
	[Feature( NODE ), Group( DEBUG )]
	protected virtual Color DebugNodeColor { get; set; } = new ColorHsv( Random.Float( 360f ), 0.7f, 0.9f );

	protected virtual float NodeAlpha => 0.2f;
	protected virtual Color LineColorDefault => Color.Gray.WithAlpha( 0.2f );

	protected float _debugOffset = Random.Float( 2f );
	protected float _debugTime = Random.Float( 2.5f, 4f );

	protected Vector3 _debugDirection = Vector3.Random.Normal;

	protected virtual Color GetNodeColor( in bool isSelected )
	{
		return DebugNodeColor.WithAlphaMultiplied( NodeAlpha );
	}

	protected virtual Color GetLineColor( in bool isSelected )
	{
		return DebugNodeColor.WithAlphaMultiplied( NodeAlpha );
	}

	protected virtual void RenderNode( in bool isGizmoPass )
	{
		if ( !isGizmoPass && !DebugRenderInGame )
			return;

		// Since no editor handles ingame. Also allows resizing.
		var color = GetNodeColor( IsOn );

		if ( IsOn )
			this.DrawSphere( DebugNodeRadius, default, Color.Transparent, color );
		else
			this.DrawSphere( DebugNodeRadius, default, color, Color.Transparent );

		if ( InGame )
		{
			// Draw active ingame connections.
			if ( Links is not null )
				foreach ( var (node, p) in Links )
					RenderNodeLink( in node, in p, isEditor: false );
		}
		else if ( InEditor )
		{
			// Connections aren't defined yet in editor mode.
			if ( DefaultLinks is not null )
				foreach ( var nl in DefaultLinks )
					RenderNodeLink( nl.Node, nl.Protocol, isEditor: true );
		}
	}

	protected virtual void RenderNodeLink( in NodeEntity node, in LinkProtocol protocol, in bool isEditor = false )
	{
		if ( !node.IsValid() || !node.Active )
			return;

		var origin = Center;
		var dest = node.Center;

		var dir = origin.Direction( dest );
		var sphere = new Sphere( dest, DebugNodeRadius * .5f );

		if ( sphere.Trace( new( origin, dir ), float.MaxValue, out var dist ) )
			dest = origin + (dir * dist);

		var up = node.WorldRotation * _debugDirection;
		var offset = up.PlaneProject( dir, 3f );

		var delta = (dest + offset) - origin;

		this.DrawArrow(
			from: origin,
			to: origin + delta,
			c: LineColorDefault,
			th: 1.5f, len: 6f, w: 1.2f,
			tWorld: global::Transform.Zero
		);

		if ( InGame )
		{
			// Don't preview disabled links in play mode.
			if ( !IsLinkActive( node ) )
				return;
		}
		else
		{
			// Preview would-be valid links in the editor.
			if ( !IsOn || !node.IsOn )
				return;
		}

		var frac = (RealTime.Now + _debugOffset) % _debugTime / _debugTime;

		if ( frac <= 0f || frac >= 1f )
			return;

		delta *= frac.Clamp( 0f, 1f );

		this.DrawArrow(
			from: origin,
			to: origin + delta,
			c: GetLineColor( IsOn ),
			th: 3.5f, len: 0f, w: 0f,
			tWorld: global::Transform.Zero
		);
	}

	/// <summary>
	/// Removes invalid and duplicate references.
	/// </summary>
	[Button( "Validate" )]
	[Order( NODE_LINKS_ORDER + 1 )]
	[Feature( NODE ), Group( LINKS )]
	[ShowIf( nameof( InEditor ), true )]
	protected virtual void ValidateDefaultLinks()
	{
		if ( DefaultLinks is null )
		{
			DefaultLinks = [];
			return;
		}

		// buh
		var goodGirls = DefaultLinks
			.Where( n => n.IsValid )
			.Distinct();

		DefaultLinks = [.. goodGirls];
	}

	protected virtual void ValidateLinks()
	{
		if ( Links is null )
			return;

		var badNodes = Links
			.Select( kv => kv.Key )
			.Where( n => !n.IsValid() );

		if ( !badNodes.Any() )
			return;

		foreach ( var node in badNodes.ToArray() )
			Links.Remove( node );
	}
}
