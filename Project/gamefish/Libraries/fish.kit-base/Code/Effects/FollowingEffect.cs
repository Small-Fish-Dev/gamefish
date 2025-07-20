namespace GameFish;

/// <summary>
/// Allows an effect to follow something without being a child. <br />
/// It is not destroyed along with the target, allowing the effect to finish playing. <br />
/// Destroys the object it is on if the effect is done or wasn't configured properly.
/// </summary>
public abstract class FollowingEffect : Component
{
	/// <summary>
	/// The object meant to be followed.
	/// </summary>
	[Sync]
	public GameObject Following { get; set; }

	/// <summary>
	/// The local transform to keep as an offset from <see cref="Following"/>.
	/// </summary>
	[Sync]
	public Transform Offset { get; set; } = global::Transform.Zero;

	/// <summary>
	/// The object of the effect we're making follow the target.
	/// </summary>
	[Sync]
	public GameObject EffectObject { get; set; }

	public TimeUntil SelfDestruct { get; set; } = 30f;

	/// <summary>
	/// Tries to make an effect follow something maturely.
	/// </summary>
	/// <returns> If the effect </returns>
	public static TFollow Create<TFollow>( GameObject fxObj, GameObject toFollow, Transform offset, float? selfDestruct = null ) where TFollow : FollowingEffect, new()
	{
		if ( !toFollow.IsValid() || !fxObj.IsValid() )
			return null;

		// var fObj = toFollow.Scene.CreateObject( enabled: true );
		var fComp = fxObj.Components.GetOrCreate<TFollow>( FindMode.EverythingInSelf );

		if ( !fComp.TryStartFollowing( fxObj, toFollow, offset, selfDestruct ) )
			fComp?.Destroy();

		return fComp;
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( !EffectObject.IsValid() )
		{
			GameObject.Destroy();
			return;
		}

		if ( !Following.IsValid() )
		{
			Following = GameObject.Parent;

			if ( !Following.IsValid() )
			{
				GameObject.Destroy();
				return;
			}
		}

		Follow();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		Follow();
	}

	/// <returns> If the effect has finished playing. </returns>
	public abstract bool IsFinished();

	public virtual bool TryStartFollowing( GameObject fxObj, GameObject toFollow, Transform offset, float? selfDestruct = null )
	{
		if ( !this.IsValid() )
			return false;

		if ( !toFollow.IsValid() || !fxObj.IsValid() )
		{
			Destroy();
			return false;
		}

		EffectObject = fxObj;
		Following = toFollow;
		Offset = offset;

		if ( selfDestruct.HasValue )
			SelfDestruct = selfDestruct.Value;

		return true;
	}

	public virtual void Follow()
	{
		if ( !this.IsValid() )
			return;

		if ( IsFinished() )
		{
			GameObject?.Destroy();
			return;
		}

		if ( Following.IsValid() )
			WorldTransform = Following.WorldTransform.ToLocal( Offset );
	}
}
