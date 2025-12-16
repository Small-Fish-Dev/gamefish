namespace Playground;

public partial class GrabberHand : ModuleEntity
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;
	protected const int PHYSICS_ORDER = EDITOR_ORDER + 10;

	/// <summary>
	/// The joint(should be a child object).
	/// </summary>
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public FixedJoint Joint => _joint.GetCached( GameObject, FindMode.InChildren );
	protected FixedJoint _joint;

	[Sync]
	public Transform Offset { get; set; } = global::Transform.Zero;

	[Sync]
	public GameObject BodyObject
	{
		get => _bodyObject;
		set
		{
			_bodyObject = value;
			OnSetBodyObject( _bodyObject );
		}
	}

	protected GameObject _bodyObject;

	protected virtual void OnSetBodyObject( GameObject obj )
	{
		if ( Joint.IsValid() )
			Joint.Body = obj;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		AutoCleanup();
	}

	protected virtual void AutoCleanup()
	{
		if ( IsProxy )
			return;

		if ( Editor.TryGetInstance( out var s ) )
			if ( s.Tool is GrabberTool grabber && grabber.Hand == this )
				return;

		DestroyGameObject();
	}
}
