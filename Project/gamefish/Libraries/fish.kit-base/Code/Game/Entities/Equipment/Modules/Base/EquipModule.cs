namespace GameFish;

/// <summary>
/// A module for an equipment that is told to simulate
/// by its owner while deployed each frame.
/// </summary>
[Icon( "backpack" )]
public abstract class EquipModule : Module
{
	protected const int EQUIP_ORDER = DEFAULT_ORDER - 777;

	/// <summary>
	/// The default order(less is earlier) of equipment module simulation.
	/// </summary>
	public const int EXECUTION_ORDER_DEFAULT = 0;

	/// <summary>
	/// Affects the order in which this module works(lower is first). <br />
	/// A module with a lower number simulates before the other.
	/// <br /> <br />
	/// <b> TODO: </b> Actually implement this.
	/// </summary>
	public virtual int ExecutionOrder { get; } = EXECUTION_ORDER_DEFAULT;

	public Equipment Equip => Parent as Equipment;
	public Pawn Pawn => Equip?.Pawn;

	public Vector3 AimPosition => Pawn?.EyePosition ?? WorldPosition;
	public Rotation AimRotation => Pawn?.EyeRotation ?? Rotation.Identity;

	public Transform AimTransform => new( AimPosition, AimRotation, Pawn?.WorldScale ?? Vector3.One );
	public Vector3 AimDirection => AimRotation.Forward;

	public bool IsDeployed => Equip?.IsDeployed is true;

	/// <summary> Should an NPC consider this for combat? </summary>
	public virtual bool IsCombatFunction { get; protected set; } = false;

	public override bool IsParent( ModuleEntity comp )
		=> comp.IsValid() && comp is Equipment;

	/// <summary>
	/// Called each frame by the owner while held.
	/// </summary>
	public virtual void Simulate( in float deltaTime ) { }

	public virtual void OnEquip( Pawn owner ) { }
	public virtual void OnDrop() { }

	public virtual void OnDeploy() { }
	public virtual void OnHolster() { }
}
