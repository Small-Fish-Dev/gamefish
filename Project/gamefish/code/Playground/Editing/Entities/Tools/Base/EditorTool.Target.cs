namespace Playground;

partial class EditorTool
{
	[Property]
	[ToolSetting]
	[Title( "Trace Filter" )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public TraceFilter Filter { get; set; }

	/// <summary>
	/// Was our target considered valid?
	/// </summary>
	public bool HasTarget { get; protected set; }

	/// <summary>
	/// The last targeting trace attempt.
	/// </summary>
	public SceneTraceResult TargetTrace { get; protected set; }

	/// <summary>
	/// The object we're looking at.
	/// </summary>
	public GameObject TargetObject { get; protected set; }
	public Component TargetComponent { get; protected set; }

	protected bool IsPointerSnapping => AllowPointerSnapping && HoldingShift;
	protected virtual bool AllowPointerSnapping => true;
	protected virtual float PointerSnapGrid => Editor?.GridSize ?? 4f;

	/// <summary>
	/// Runs the primary trace for this tool.
	/// </summary>
	protected virtual SceneTraceResult RunTrace( in Ray ray )
		=> RunDefaultTrace( in ray );

	/// <summary>
	/// Runs a simple ray trace(the default).
	/// </summary>
	protected SceneTraceResult RunDefaultTrace( in Ray ray )
	{
		if ( !ITransform.IsValid( ray.Position )
		  || !ITransform.IsValid( ray.Forward ) )
			return default;

		return Editor.Trace( Scene, ray );
	}

	public virtual bool TryTrace( out SceneTraceResult tr )
	{
		if ( !Editor.TryGetAimRay( Scene, out var ray ) )
		{
			tr = default;
			return false;
		}

		tr = RunTrace( ray );

		return true;
	}

	protected virtual bool TryGetPointer( in SceneTraceResult tr, out Transform tPointer )
	{
		var hitObj = tr.GameObject;

		if ( IsPointerSnapping && TryGetOrigin( out var tOrigin ) )
		{
			var vPointer = tr.Hit ? tr.HitPosition : tr.EndPosition;
			var vLocal = tOrigin.PointToLocal( vPointer ).SnapToGrid( PointerSnapGrid );
			var vSnap = tOrigin.PointToWorld( vLocal );

			var vPlaneDir = Rotation.LookAt( tOrigin.Forward, tOrigin.Up )
				.ClosestAxis( tr.Direction );

			var oPlane = new Plane( vSnap, vPlaneDir );
			vPointer = oPlane.SnapToPlane( vPointer );
			vLocal = tOrigin.PointToLocal( vPointer ).SnapToGrid( PointerSnapGrid );
			vSnap = tOrigin.PointToWorld( vLocal );

			tPointer = new( vSnap, tOrigin.Rotation );
		}
		else if ( tr.Hit && hitObj.IsValid() )
		{
			var tObj = hitObj.WorldTransform;

			// Using only the normal vector for rotation is buggy.
			// This weird nerd shit gets a stable relative up axis.
			var vProjected = Vector3.VectorPlaneProject( tr.Direction, tr.Normal );
			var vClosestAxis = tObj.Rotation.ClosestAxis( vProjected );
			var rNormal = Rotation.LookAt( tr.Normal, vClosestAxis );

			tPointer = new( tr.HitPosition, rNormal );

			if ( IsPointerSnapping )
			{
				var tSnap = new Transform( hitObj.WorldPosition, rNormal );

				var vLocal = tSnap.PointToLocal( tPointer.Position ).SnapToGrid( PointerSnapGrid );
				var vWorld = tSnap.PointToWorld( vLocal );

				var trPlane = new Plane( tr.HitPosition, tSnap.Forward );

				tPointer.Position = trPlane.SnapToPlane( vWorld );
			}
		}
		else
		{
			tPointer = new( tr.EndPosition );

			if ( IsPointerSnapping )
				tPointer.Position = tPointer.Position.SnapToGrid( PointerSnapGrid );
		}

		return true;
	}

	protected virtual void ClearTarget()
	{
		HasTarget = false;

		TargetTrace = default;
		TargetObject = null;
		TargetComponent = null;
	}

	public virtual bool IsValidTarget( Component ent )
		=> ent.IsValid();

	/// <summary>
	/// Figure out what we're looking at right now.
	/// </summary>
	protected virtual void UpdateTarget( bool clearPrevious = true )
	{
		if ( clearPrevious )
			ClearTarget();

		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		TargetTrace = tr;

		if ( !TryGetTargetComponent( in tr, out var target ) )
			return;

		TrySetTarget( in tr, target );
	}

	public virtual bool TrySetTarget( in SceneTraceResult tr, Component target )
	{
		if ( !target.IsValid() )
			return false;

		SetTarget( target?.GameObject, target, in tr );

		return true;
	}

	protected virtual void SetTarget( GameObject obj = null, Component target = null, in SceneTraceResult tr = default )
	{
		HasTarget = obj.IsValid();

		TargetTrace = tr;
		TargetObject = target.GameObject;
		TargetComponent = target;
	}

	public virtual bool TryGetTargetComponent( in SceneTraceResult tr, out Component target )
	{
		if ( TryGetTargetEntity( in tr, out var ent ) )
		{
			target = ent;
			return true;
		}

		if ( tr.Component.IsValid() )
		{
			target = tr.Component;
			return true;
		}

		if ( tr.Collider.IsValid() && tr.Collider.Static is true )
		{
			target = tr.Collider;
			return true;
		}

		target = null;
		return false;
	}

	protected virtual bool TryGetTargetEntity( in SceneTraceResult tr, out EditorObject e )
	{
		e = null;

		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		const FindMode findMode = FindMode.EnabledInSelf
			| FindMode.InAncestors;

		return tr.GameObject.Components.TryGet( out e, findMode );
	}
}
