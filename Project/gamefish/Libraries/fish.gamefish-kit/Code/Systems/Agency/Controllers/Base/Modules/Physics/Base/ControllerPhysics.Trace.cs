using System;

namespace GameFish;

partial class ControllerPhysics
{
	/// <summary>
	/// Movement/collision logic tries to stay this far away
	/// from surfaces to prevent getting stuck in them.
	/// </summary>
	[Property]
	[Range( 0.01f, 5f, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public float SkinWidth { get; set; } = 0.5f;

	[Property]
	[Title( "Offset" )]
	[InlineEditor, WideMode]
	[Feature( PAWN ), Group( PHYSICS )]
	public virtual Offset TraceOffset { get; set; } = new();

	public virtual ITagSet TraceTags => Tags;

	public Transform Origin => Pawn?.WorldTransform ?? WorldTransform;

	/// <summary>
	/// This is where solid object filters and such go.
	/// </summary>
	/// <param name="skin"> The skin width. Grows/shrinks shape size. </param>
	/// <returns> The basis of every collison trace. </returns>
	public virtual SceneTrace Trace( in float skin = 0f )
	{
		if ( !Scene.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject );

		return tr;
	}

	/// <summary>
	/// Creates the default collision trace and sets the start and end points.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public SceneTrace Trace( in Vector3 from, in Vector3 to, in float skin = 0f )
	{
		var tFrom = Origin.WithPosition( from );

		return Trace( in tFrom, in to, skin: skin );
	}

	/// <summary>
	/// Creates the default collision trace and sets the end point relative to our starting position.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public SceneTrace Trace( in Vector3 vDelta, in float skin = 0f )
	{
		var tFrom = Origin;
		var to = tFrom.Position + vDelta;

		return Trace( in tFrom, in to, skin: skin );
	}

	/// <summary>
	/// Creates the default collision trace and sets the start and end transforms.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public virtual SceneTrace Trace( in Transform tFrom, in Vector3 to, in float skin = 0f )
		=> Trace( skin: skin ).FromTo( tFrom, to );

	/// <returns> If that space is free. </returns>
	protected bool IsEmpty( out SceneTraceResult trEmpty, in float skin, ProjectedResult result )
		=> IsEmpty( in result.Point, out trEmpty, in skin );

	/// <returns> If that space is free. </returns>
	public virtual bool IsEmpty( in Transform tSpace, out SceneTraceResult trEmpty, in float skin = 0f )
	{
		trEmpty = Trace( in tSpace, in tSpace.Position, skin: in skin ).Run();

		if ( trEmpty.Hit )
			return false;

		if ( trEmpty.StartedSolid )
			return false;

		return true;
	}

	protected virtual SceneTraceResult GroundTrace( in Transform tStart, float dist )
	{
		dist = (dist + SkinWidth).Max( SkinWidth );

		var endPos = tStart.Position + (Down * dist);
		var tr = Trace( in tStart, in endPos, skin: -SkinWidth );

		return tr.Run();
	}
}
