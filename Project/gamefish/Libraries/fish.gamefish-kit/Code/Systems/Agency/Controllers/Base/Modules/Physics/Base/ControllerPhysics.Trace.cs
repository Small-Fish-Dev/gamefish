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

	/// <summary>
	/// The transform that the physics traces originate from.
	/// </summary>
	public Transform TraceOrigin => Origin.WithOffset( TraceOffset );

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
	/// Creates the default collision trace and sets the start and end transforms.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public virtual SceneTrace Trace( in Transform tFrom, in Vector3 to, in float skin = 0f )
		=> Trace( skin: skin ).FromTo( tFrom, to );

	/// <returns> If the space projection would be at is currently free. </returns>
	protected virtual bool IsEmpty( in Vector3 pos, in bool skin, ProjectedMovement move, out SceneTraceResult trEmpty )
	{
		trEmpty = move.Trace( move.WithPosition( pos ), in pos, in skin ).Run();
		return !trEmpty.StartedSolid;
	}

	/*
	/// <returns> If that space is free. </returns>
	public virtual bool IsEmpty( in Transform tSpace, out SceneTraceResult trEmpty, in float skin = 0f )
	{
		trEmpty = Trace( in tSpace, in tSpace.Position, skin: in skin ).Run();
		return !trEmpty.StartedSolid;
	}
	*/

	protected virtual SceneTraceResult GroundTrace( ProjectedMovement move )
	{
		var dist = move.IsGrounded ? GroundDistance.Positive() : 0f;
		dist = dist.Max( SkinWidth * 2f ).Max( 1f );

		var dest = move.Position + (Gravity.Normal * dist);
		var tr = move.Trace( in move.Point, in dest, skin: false );

		return tr.Run();
	}
}
