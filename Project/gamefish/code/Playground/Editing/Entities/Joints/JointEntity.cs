using Playground.Razor;

namespace Playground;

[Icon( "precision_manufacturing" )]
public abstract class JointEntity : EditorDevice
{
	protected const int PHYSICS_ORDER = EDITOR_ORDER + 10;
	protected const int SETTINGS_ORDER = EDITOR_ORDER + 50;

	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.ClearOwner;

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
