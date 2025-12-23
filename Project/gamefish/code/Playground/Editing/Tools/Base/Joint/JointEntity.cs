using Playground.Razor;

namespace Playground;

[Icon( "precision_manufacturing" )]
public abstract class JointEntity : Entity
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;
	protected const int PHYSICS_ORDER = EDITOR_ORDER + 10;
	protected const int SETTINGS_ORDER = EDITOR_ORDER + 50;

	protected override bool? IsNetworkedOverride => true;
	protected override bool IsNetworkedAutomatically => true;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.ClearOwner;

	[Sync]
	public ToolAttachPoint ParentPoint { get; set; }

	[Sync]
	public ToolAttachPoint TargetPoint { get; set; }

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

	public abstract void UpdateJoint( in float deltaTime );
	public abstract bool TryAttachTo( in ToolAttachPoint a, in ToolAttachPoint b );
}
