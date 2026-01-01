namespace Playground;

partial class EditorTool
{
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

	protected virtual bool TryGetPointer( in SceneTraceResult tr, out Transform tPointer )
	{
		if ( tr.Hit )
		{
			var rNormal = Rotation.LookAt( tr.Normal );
			tPointer = new( tr.EndPosition, rNormal );
			return true;
		}

		tPointer = new( tr.EndPosition );

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
	protected virtual void FindTarget( bool clearPrevious = true )
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
