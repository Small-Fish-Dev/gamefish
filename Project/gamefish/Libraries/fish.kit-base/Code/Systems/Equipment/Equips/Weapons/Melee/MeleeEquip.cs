namespace GameFish;

public partial class MeleeEquip : BaseEquip
{
	[Property, Feature( MELEE )]
	public SoundEvent AttackHitSound { get; set; }
	[Property, Feature( MELEE )]
	public SoundEvent AttackMissSound { get; set; }
	[Property, Feature( MELEE )]
	public PrefabFile AttackEffectPrefab { get; set; }

	[Property, Feature( MELEE )]
	public float AttackDamage { get; set; } = 20f;
	[Property, Feature( MELEE )]
	public float AttackDistance { get; set; } = 100f;
	[Property, Feature( MELEE )]
	public float AttackRadius { get; set; } = 16f;
	[Property, Feature( MELEE )]
	public float AttackKnockback { get; set; } = 0f;

	public override void OnPrimary()
	{
		if ( !Scene.IsValid() || !Owner.IsValid() )
			return;

		// var aimPos = AimPosition;
		var aimDir = AimDirection;

		// var allyTag = Owner.Team is  ? ;

		var attackDist = AttackDistance.Positive() - AttackRadius.Positive();

		var tr = Owner.GetEyeTrace( distance: attackDist, dir: aimDir )
			// .WithoutTags( allyTag )
			.Radius( AttackRadius )
			.Run();

		if ( !tr.Hit || tr.GameObject is not GameObject obj )
			return;

		var tags = new TagSet( [DamageTags.MELEE] );

		var dmgInfo = new DamageInfo( AttackDamage, Owner.GameObject, GameObject )
		{ Tags = tags };

		// Attempt to deal damage and play appropriate sound.
		if ( obj.TryDamage( dmgInfo ) )
		{
			if ( AttackKnockback != 0f )
			{
				if ( obj.Components.TryGet<Rigidbody>( out var rb, FindMode.EnabledInSelf | FindMode.InAncestors ) )
					rb.Velocity += tr.Direction * AttackKnockback;
				else if ( obj.Components.TryGet<IVelocity>( out var vel, FindMode.EnabledInSelf | FindMode.InAncestors ) )
					vel.TryModifyVelocity( tr.Direction * AttackKnockback );
			}

			// Blood effect or something.
			AttackEffectPrefab.TrySpawn( tr.HitPosition, out _ );

			if ( AttackHitSound.IsValid() )
				Sound.Play( AttackHitSound, tr.HitPosition );
		}
		else
		{
			if ( AttackMissSound.IsValid() )
				Sound.Play( AttackMissSound, tr.HitPosition );
		}
	}
}
