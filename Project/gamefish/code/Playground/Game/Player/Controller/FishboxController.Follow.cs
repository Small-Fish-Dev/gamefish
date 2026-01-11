using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController
{
	[Sync]
	public GameObject FollowObject
	{
		get => _followObj;

		protected set
		{
			var old = _followObj;
			_followObj = value;

			OnSetFollowParent( _followObj, old );
		}
	}

	protected GameObject _followObj;

	/// <summary>
	/// Where we were last known to be at(locally) on the parent.
	/// </summary>
	[Sync]
	public Transform? FollowLastWorldTransform { get; protected set; }

	/// <summary>
	/// Where we were last known to be at(locally) on the parent.
	/// </summary>
	[Sync]
	public Transform? FollowLastOffset { get; protected set; }

	protected virtual void OnSetFollowParent( GameObject newFollow, GameObject oldFollow )
	{
		if ( IsProxy )
			return;

		if ( !newFollow.IsValid() )
		{
			OnStopFollowingParent( oldFollow );

			if ( oldFollow.IsValid() && TryGetGroundNormal( out var vUp ) )
			{
				GravityDirection = -vUp;
				SetUpDirection( vUp );
			}

			return;
		}

		if ( newFollow != oldFollow && newFollow.IsValid() )
			OnStartFollowingParent( newFollow );
	}

	protected virtual void OnStopFollowingParent( GameObject oldParent )
	{
		FollowLastOffset = null;
	}

	protected virtual void OnStartFollowingParent( GameObject objFollow )
	{
		if ( !objFollow.IsValid() )
			return;

		FollowLastWorldTransform = null;
		FollowLastOffset = null;

		FollowParent();
	}

	protected virtual void FollowParent()
	{
		if ( IsProxy || !FollowObject.IsValid() )
			return;

		var tSelf = WorldTransform;
		var tParent = FollowObject.WorldTransform;
		var tParentLast = FollowLastWorldTransform ??= tParent;
		var tOffset = FollowLastOffset ??= tParent.ToLocal( tSelf );

		if ( tParent.AlmostEqual( tParentLast ) )
			return;

		var tRelativePrev = tParentLast.ToWorld( tOffset );
		var tRelativeNow = tParent.ToWorld( tOffset );

		var vDelta = tRelativeNow.Position - tRelativePrev.Position;
		var rDelta = tRelativePrev.RotationToLocal( tRelativeNow.Rotation );

		var tDest = tSelf;
		tDest.Position += vDelta;
		tDest.Rotation *= rDelta;

		FollowLastWorldTransform = tParent;
		FollowLastOffset = tParent.ToLocal( tDest );

		if ( Rigidbody.IsValid() && Rigidbody.PhysicsBody.IsValid() )
			SetPhysicsTransform( tDest );
	}
}
