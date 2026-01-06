namespace Fishbox;

public partial struct PropellerSettings
{
	[Title( "Accelerate" )]
	public string KeyForward { get; set; } = "KP_8";

	[Title( "Reverse" )]
	public string KeyReverse { get; set; } = "KP_2";

	[Title( "Spin Speed" )]
	public float SpinSpeed { get; set; } = 30f;

	[Title( "Torque Limit" )]
	public float SpinLimit { get; set; } = 40f;

	/// <summary>
	/// The lift always added by current torque.
	/// </summary>
	public float BaseLift { get; set; } = 300000f;

	/// <summary>
	/// Applies extra lift by factoring mass with torque.
	/// </summary>
	public float MassLift { get; set; } = 6f;

	[Title( "Friction" )]
	public Friction Friction { get; set; } = new( 0.5f, 2f );

	public PropellerSettings() { }
}
