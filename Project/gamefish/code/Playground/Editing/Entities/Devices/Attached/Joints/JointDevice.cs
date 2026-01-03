using Playground.Razor;

namespace Playground;

[Icon( "precision_manufacturing" )]
public abstract class JointDevice : AttachDevice
{
	[Sync]
	public DeviceAttachPoint LocalPoint { get; set; }

	[Sync]
	public DeviceAttachPoint TargetPoint { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( EditorMenu.IsOpen )
			DrawJointGizmo();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		UpdateJoint( Time.Delta );
	}

	protected virtual void DrawJointGizmo()
	{
	}

	public abstract void ApplySettings();

	public abstract void UpdateJoint( in float deltaTime );

	public abstract bool TryAttachTo( in DeviceAttachPoint a, in DeviceAttachPoint b );
}
