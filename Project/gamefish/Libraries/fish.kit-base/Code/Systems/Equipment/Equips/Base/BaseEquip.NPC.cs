namespace GameFish;

partial class BaseEquip
{
	/// <summary> Should an NPC consider this for combat? </summary>
	[Property, Feature( NPC )]
	public virtual bool UseInCombat { get; set; } = true;

	/// <summary>
	/// An NPC shouldn't use this outside of these ranges.
	/// </summary>
	[Property, Feature( NPC ), Group( GROUP_RANGE )]
	public virtual FloatRange IdealRange { get; set; } = new( 0f, 2048f );

	/// <returns> If an NPC is allowed to use this at the specified distance. </returns>
	public virtual bool UsableAtDistance( float targetDist )
		=> IdealRange.Within( targetDist );

	/// <returns> If an NPC will ever bother with this equipment. </returns>
	public virtual bool IsUsable( BaseNPC npc, in bool forCombat = true )
		=> forCombat == UseInCombat;

	/// <summary>
	/// Overrides where an <see cref="BaseNPC"/> aims this(if not null). <br />
	/// Would be very useful for aiming projectiles ahead.
	/// </summary>
	public virtual Vector3? GetAimTargetPosition( BasePawn pawn, in Vector3? visiblePos = null )
		=> null;
}
