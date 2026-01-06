using GameFish;

namespace Fishbox;

/// <summary>
/// Overrides the mass center to its position.
/// </summary>
[Icon( "filter_tilt_shift" )]
// [Icon( "flip_camera_android" )]
public partial class CenteringDevice : AttachedDevice
{
	public override bool RefreshPhysicsUponJoin => false;

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		Apply( Time.Delta );
	}

	protected override void OnDestroy()
	{
		if ( Rigidbody.IsValid() )
			Rigidbody.OverrideMassCenter = false;

		base.OnDestroy();
	}

	protected override void RenderDeviceHelpers()
	{
		if ( !Rigidbody.IsValid() )
			return;

		var c = Color.Magenta
			.Darken( 0.3f )
			.Desaturate( 0.4f )
			.WithAlpha( 0.3f );

		/*
		// This lags a lot??

		this.DrawSphere(
			r: 7f, center: default,
			cLines: c, cSolid: c,
			tWorld: new( WorldPosition )
		);
		*/

		var bounds = BBox.FromPositionAndSize( default, 12f );

		this.DrawBox(
			bounds,
			cLines: c, cSolid: c,
			tWorld: new( WorldPosition )
		);
	}

	public virtual void Apply( in float deltaTime )
	{
		if ( !Rigidbody.IsValid() )
			return;

		var pb = Rigidbody.PhysicsBody;

		if ( !pb.IsValid() )
			return;

		var tBody = pb.Transform;

		pb.OverrideMassCenter = true;
		pb.LocalMassCenter = tBody.PointToLocal( WorldPosition );
	}

	public virtual bool TryAttachTo( GameObject obj, in Offset offs )
	{
		if ( !obj.IsValid() )
			return false;

		WorldTransform = obj.WorldTransform.WithOffset( offs );

		GameObject.SetParent( obj, keepWorldPosition: true );

		Transform.ClearInterpolation();

		return true;
	}
}
