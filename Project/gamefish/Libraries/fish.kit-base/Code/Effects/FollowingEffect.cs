using Microsoft.VisualBasic;

namespace GameFish;

/// <summary>
/// Allows an effect to follow something without being a child. <br />
/// It is not destroyed along with the target, allowing the effect to finish playing. <br />
/// Destroys the object it is on if the effect is done or wasn't configured properly.
/// </summary>
public class FollowingEffect : Component
{
	/// <summary>
	/// The object meant to be followed.
	/// </summary>
	[Sync]
	[Property]
	public GameObject Following { get; set; }

	/// <summary>
	/// The local transform to keep as an offset from <see cref="Following"/>.
	/// </summary>
	[Sync]
	[Property]
	public Transform Offset { get; set; } = global::Transform.Zero;

	/// <summary>
	/// The object of the effect we're making follow the target.
	/// </summary>
	[Sync]
	[Property]
	public GameObject EffectObject { get; set; }

	/// <summary>
	/// The minimum time this object can exist.
	/// </summary>
	public TimeUntil SelfDestruct { get; set; } = 10f;

	/// <summary>
	/// Makes an object follow something and destroy itself when all effects have completed.
	/// </summary>
	/// <returns> The following component(or null). </returns>
	public static FollowingEffect Create( GameObject toFollow, GameObject fxObj, Transform offset, float? selfDestruct = null )
		=> Create<FollowingEffect>( toFollow: toFollow, fxObj: fxObj, offset: offset, selfDestruct: selfDestruct );

	/// <summary>
	/// Makes an object follow something and destroy itself when all effects have completed.
	/// </summary>
	/// <returns> The following component(or null). </returns>
	public static TFollow Create<TFollow>( GameObject toFollow, GameObject fxObj, Transform offset, float? selfDestruct = null )
		where TFollow : FollowingEffect, new()
	{
		if ( !toFollow.IsValid() )
			return null;

		if ( !fxObj.IsValid() )
			return null;

		var fComp = fxObj.Components.GetOrCreate<TFollow>( FindMode.EverythingInSelf );
		fComp.Enabled = true;

		if ( !fComp.TryStartFollowing( fxObj: fxObj, toFollow: toFollow, offset: offset, selfDestruct: selfDestruct ) )
			return null;

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
	public virtual bool IsFinished()
	{
		foreach ( var iTemp in Components.GetAll<ITemporaryEffect>( FindMode.EnabledInSelfAndDescendants ) )
			if ( iTemp.IsActive )
				return false;

		return SelfDestruct;
	}

	public virtual bool TryStartFollowing( GameObject fxObj, GameObject toFollow, Transform offset, float? selfDestruct = null )
	{
		if ( !this.IsValid() )
			return false;

		if ( !toFollow.IsValid() || !fxObj.IsValid() )
		{
			DestroyGameObject();
			return false;
		}

		EffectObject = fxObj;
		Following = toFollow;
		Offset = offset;

		if ( selfDestruct is float timer )
			SelfDestruct = timer;

		return true;
	}

	public virtual void Follow()
	{
		if ( !this.IsValid() )
			return;

		if ( !IsProxy && IsFinished() )
		{
			GameObject?.Destroy();
			return;
		}

		if ( Following.IsValid() )
			WorldTransform = Following.WorldTransform.ToLocal( Offset );
	}
}
