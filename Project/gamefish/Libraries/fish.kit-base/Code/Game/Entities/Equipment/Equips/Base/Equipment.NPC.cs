namespace GameFish;

partial class Equipment
{
	/// <summary> Should an NPC consider this for combat? </summary>
	public virtual bool UseInCombat { get; set; } = true;

	/// <returns> If an NPC will ever bother with this equipment. </returns>
	public virtual bool IsUsable( Actor npc, in bool forCombat )
		=> forCombat == UseInCombat;

	/// <returns> If an NPC is allowed to use this at the specified distance. </returns>
	public virtual bool UsableAtDistance( in float targetDist )
		=> PrimaryFunction?.UsableRange.Within( targetDist ) is true;

	/// <summary>
	/// Allows you to make this equipment selected over others
	/// with respect to distance from the target.
	/// </summary>
	/// <returns> A number that's lower the better it is for this distance. </returns>
	public virtual float GetSelectionPriority( in float targetDist )
		=> PrimaryFunction?.GetSelectionPriority( in targetDist ) ?? EquipModule.EXECUTION_ORDER_DEFAULT;

	/// <summary>
	/// An NPC shouldn't use this outside of these ranges.
	/// They'll attack in this range but try to get within <see cref="IdealRange"/> still.
	/// </summary>
	public virtual FloatRange? UsableRange => PrimaryFunction?.UsableRange;

	/// <summary>
	/// An NPC will try to get within this range when using this.
	/// If it is not encompassed by <see cref="UsableRange"/> then bugs may happen.
	/// </summary>
	public virtual FloatRange? IdealRange => PrimaryFunction?.IdealRange;

	/// <summary>
	/// NPCs will use this function when attacking.
	/// </summary>
	[Property]
	[Feature( NPC ), Group( MODULES )]
	public virtual EquipFunction PrimaryFunction
	{
		get => _primary.IsValid() ? _primary : _primary = GetModules<EquipFunction>().FirstOrDefault();
		set => _primary = value;
	}

	protected EquipFunction _primary;

	/// <summary>
	/// Called by NPCs owning this to attack or whatever.
	/// </summary>
	public virtual bool TryPrimary( in float targetDist )
	{
		var primary = PrimaryFunction;

		// Prevents them from shooting from too far away or blowing themselves/others up.
		if ( !primary.IsValid() || !primary.UsableAtDistance( targetDist ) )
			return false;

		return primary.TryActivate();
	}

	/// <summary>
	/// Overrides where an NPC aims this(if not null).
	/// </summary>
	/// <remarks> Would be very useful for aiming projectiles ahead. </remarks>
	/// <param name="targetPawn"> The shmuck. </param>
	/// <param name="baseAim"> Where the NPC would aim by default(or not). </param>
	public virtual Vector3? GetTargetAimPosition( BasePawn targetPawn, in Vector3? baseAim = null )
		=> PrimaryFunction?.GetTargetAimPosition( targetPawn, in baseAim );

	/// <summary>
	/// Called by NPCs we're simulating while actively deployed.
	/// </summary>
	public virtual void Simulate( Actor npc, in float deltaTime )
	{
	}
}
