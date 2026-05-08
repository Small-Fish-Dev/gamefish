namespace Fishbox;

[Icon( "alarm" )]
public class ShooterSlowMotion : Module
{
	protected const int BADASS_ORDER = MODULE_ORDER - 1000;

	[Property]
	[Title( "Time Scale (focus)" )]
	[Range( 0.05f, 0.9f, clamped: false )]
	[Feature( BADASS ), Order( BADASS_ORDER )]
	public float FocusTimeScale { get; set; } = 0.4f;

	/// <summary>
	/// How quickly
	/// </summary>
	[Property]
	[Title( "Time Scale Speed" )]
	[Range( 5f, 20f, clamped: false )]
	[Feature( BADASS ), Order( BADASS_ORDER )]
	public float FocusTimeSpeed { get; set; } = 8f;

	public override bool IsParent( ModuleEntity comp )
		=> comp is ShooterMode;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !Scene.InGame() )
			return;

		var anyFocus = Scene
			.GetAll<ShooterController>()
			.Any( sc => sc.IsFocusing );

		var focusScale = FocusTimeScale.Max( 0.01f );
		var targetTimeScale = anyFocus ? focusScale : 1f;

		var speed = RealTime.SmoothDelta * FocusTimeSpeed;

		Scene.TimeScale = Scene.TimeScale.LerpTo( targetTimeScale, speed );
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();

		if ( !Scene.InGame() )
			return;

		Scene.TimeScale = 1f;
	}
}
