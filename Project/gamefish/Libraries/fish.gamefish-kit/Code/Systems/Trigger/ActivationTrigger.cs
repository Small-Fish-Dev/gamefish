namespace GameFish;

/// <summary>
/// Sends an activation signal to a component with <see cref="IActivate"/> when triggered.
/// <code> trigger_multiple </code>
/// </summary>
[Icon( "electric_bolt" )]
public partial class ActivationTrigger : FilterTrigger
{
	protected const int LOGIC_ORDER = TRIGGER_ORDER - 1000;

	/// <summary>
	/// The parent component implementing <see cref="IActivate"/> to use.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( LOGIC ), Order( LOGIC_ORDER )]
	public virtual IActivate Target
	{
		get => _target;
		set => _target = value;
	}

	protected IActivate _target;

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		Target?.Activate( obj );
	}
}
