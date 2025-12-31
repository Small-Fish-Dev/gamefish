namespace Playground;

/// <summary>
/// A parent entity for <see cref="EditorObject"/>s.
/// Allows them to stick together without physics joints.
/// </summary>
[Icon( "data_array" )]
public partial class EditorIsland : EditorObject
{
	/// <summary>
	/// Finds all objects within that we should be concerned with.
	/// </summary>
	public IEnumerable<EditorObject> FindObjects()
		=> Components?.GetAll<EditorObject>( FindMode.Enabled | FindMode.InDescendants ) ?? [];

	/// <summary>
	/// Finds all objects within that we should be concerned with.
	/// </summary>
	public IEnumerable<Rigidbody> FindRigidbodies()
		=> Components?.GetAll<Rigidbody>( FindMode.InDescendants ) ?? [];

	protected override void OnStart()
	{
		base.OnStart();

		TogglePhysics();
	}

	/// <summary>
	/// Dunno why exactly but this fixes shit and is a clean enough hack.
	/// </summary>
	public virtual void TogglePhysics()
	{
		if ( !Rigidbody.IsValid() || !Rigidbody.Enabled )
			return;

		Rigidbody.Enabled = false;
		Rigidbody.Enabled = true;
	}

	public virtual bool TryAddObject( EditorObject ent, Offset offset )
	{
		if ( !ent.IsValid() || !ent.GameObject.IsValid() )
			return false;

		ent.GameObject.SetParent( GameObject );

		if ( ent.GameObject.Parent != GameObject )
			return false;

		ent.SetOffset( offset );
		ent.Transform.ClearInterpolation();

		return true;
	}

	public virtual void OnObjectAdded( EditorObject ent )
	{
		TogglePhysics();
	}

	public virtual void OnObjectDestroyed( EditorObject ent )
	{
		if ( !this.InGame() || !GameObject.IsValid() )
			return;

		if ( !FindObjects().Any() )
			GameObject.Destroy();
	}
}
