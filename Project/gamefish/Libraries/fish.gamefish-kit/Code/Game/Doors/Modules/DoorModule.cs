namespace GameFish;

/// <summary>
/// Added to a <see cref="Door"/> to hook into its events.
/// </summary>
[Icon( "door_front" )]
public abstract class DoorModule : Module
{
	protected const int DOOR_ORDER = MODULE_ORDER - 1000;
	protected const int DOOR_DEBUG_ORDER = DOOR_ORDER - 5;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Door;

	public Door Door => Parent as Door;

	/// <inheritdoc cref="Door.OnOpened" />
	public virtual void OnOpened() { }
	/// <inheritdoc cref="Door.OnOpening" />
	public virtual void OnOpening() { }

	/// <inheritdoc cref="Door.OnClosed" />
	public virtual void OnClosed() { }
	/// <inheritdoc cref="Door.OnClosing" />
	public virtual void OnClosing() { }

	/// <inheritdoc cref="Door.OnLocked" />
	public virtual void OnLocked() { }
	/// <inheritdoc cref="Door.OnUnlocked" />
	public virtual void OnUnlocked() { }
}
