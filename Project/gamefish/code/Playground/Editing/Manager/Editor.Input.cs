namespace Fishbox;

partial class Editor : IControls
{
	// Allow tool to decide if inputs are blocked.
	public virtual bool HasAimingFocus => Tool?.HasAimingFocus is true;
	public virtual bool HasMovingFocus => Tool?.HasMovingFocus is true;
	public virtual bool HasActionFocus => Tool?.HasActionFocus is true;
	public virtual bool HasScrollFocus => Tool?.HasScrollFocus is true;
}
