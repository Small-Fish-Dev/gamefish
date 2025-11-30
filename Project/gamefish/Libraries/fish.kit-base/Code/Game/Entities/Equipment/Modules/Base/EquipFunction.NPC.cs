namespace GameFish;

public abstract partial class EquipFunction : EquipModule
{
	/// <summary> Should an NPC attack with this? </summary>
	[Feature( NPC )]
	[Title( "For Combat" )]
	[Property, Order( EQUIP_ORDER + 1 )]
	public override bool IsCombatFunction { get; protected set; } = true;

	/// <summary>
	/// An NPC shouldn't use this function outside of these ranges.
	/// They'll use it in this range but try to get within <see cref="IdealRange"/> still.
	/// </summary>
	[Property]
	[Feature( NPC )]
	public virtual FloatRange UsableRange { get; set; } = new( 0f, 2048f );

	/// <summary>
	/// An NPC will try to get within this range when using this.
	/// If it is not encompassed by <see cref="UsableRange"/> then bugs may happen.
	/// </summary>
	[Property]
	[Feature( NPC )]
	public virtual FloatRange IdealRange { get; set; } = new( 200f, 1000f );

	/// <returns> If an NPC is allowed to use this at the specified distance. </returns>
	public virtual bool UsableAtDistance( in float targetDist )
		=> UsableRange.Within( targetDist );

	/// <summary>
	/// Allows you to make this equipment selected over others
	/// with respect to distance from the target.
	/// </summary>
	/// <returns> A number that's lower the better it is for this distance. </returns>
	public virtual float GetSelectionPriority( in float targetDist )
		=> (targetDist - IdealRange.Min).Abs();

	/// <summary>
	/// Overrides where an NPC aims this(if not null).
	/// Would be very useful for aiming projectiles ahead.
	/// </summary>
	/// <param name="pawn"> The shmuck. </param>
	/// <param name="aimStart"> Where the NPC would aim by default(or not). </param>
	/// <param name="clampLength"> Prevent aiming farther than we can attack? </param>
	public virtual Vector3? GetTargetAimPosition( BasePawn pawn, in Vector3? aimStart = null, in bool clampLength = true )
		=> null;
}
