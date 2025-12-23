using System.Drawing;

namespace Playground;

[Icon( "import_export" )]
public partial class SliderJoint : JointEntity
{
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Sandbox.SliderJoint Slider { get; set; }

	/// <summary>
	/// The key you press to activate the spiner you're placing.
	/// </summary>
	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public SliderSettings Settings { get; set; }

	/// <summary>
	/// Should the length be restricted? Otherwise it will move freely along.
	/// </summary>
	[Sync]
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public bool IsLengthEnabled { get; set; }

	[Sync]
	[Property, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public float Direction
	{
		get => _direction.Clamp( -1f, 1f );
		set => _direction = value.Clamp( -1f, 1f );
	}

	protected float _direction;

	[Sync]
	public float TargetLength { get; set; }

	[Sync]
	public float MinLength { get; set; }

	[Sync]
	public float MaxLength { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		// Snap to the offset without lag if we're the owner.
		if ( GameObject.Parent.IsValid() && !GameObject.Parent.IsProxy )
			TryAttachTo( a: ParentPoint, b: TargetPoint );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		UpdateInput( Time.Delta );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( !this.InGame() || !GameObject.IsValid() )
			return;

		// Auto-cleanup empty objects that only had a spiner.
		var comps = GameObject.Components.GetAll( FindMode.EverythingInSelf )
			.Where( comp => comp.IsValid() );

		if ( !comps.Any() )
			GameObject.Destroy();
	}

	protected void UpdateInput( in float deltaTime )
	{
		// Toggle Length
		var bToggle = !Settings.KeyToggle.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyToggle );

		if ( bToggle )
			IsLengthEnabled = !IsLengthEnabled;

		// Length Controls
		if ( !IsLengthEnabled )
			return;

		var dir = 0f;

		var bShorten = !Settings.KeyShorten.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyShorten );

		var bLengthen = !Settings.KeyLengthen.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyLengthen );

		if ( bShorten )
			dir += 1;

		if ( bLengthen )
			dir -= 1;

		Direction = dir;
	}

	protected override void DrawJointGizmo()
	{
		var c = Color.Red.Desaturate( 0.3f ).WithAlpha( 0.3f );

		var objParent = ParentPoint.Object;
		var objTarget = TargetPoint.Object;

		if ( !objParent.IsValid() || !objTarget.IsValid() )
			return;

		if ( ParentPoint.Offset is not Offset parentOffset )
			return;

		if ( TargetPoint.Offset is not Offset targetOffset )
			return;

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );
		var tTargetPoint = objTarget.WorldTransform.WithOffset( targetOffset );

		this.DrawArrow(
			from: tParentPoint.Position,
			to: tTargetPoint.Position,
			c: c, len: 16f, w: 4f, th: 3f,
			tWorld: global::Transform.Zero
		);
	}

	public override void ApplySettings()
	{
	}

	public override void UpdateJoint( in float deltaTime )
	{
		if ( !Slider.IsValid() )
			return;

		if ( !ParentPoint.Object.IsValid() )
			return;

		/*
		if ( Direction.AlmostEqual( 0f ) )
			return;

		if ( !IsLengthEnabled )
		{
			Slider.MinLength = 0f;
			Slider.MaxLength = 0f;

			return;
		}

		// var newLength = 
		*/
	}

	public override bool TryAttachTo( in ToolAttachPoint a, in ToolAttachPoint b )
	{
		// Must have a Slider(the entire point).
		if ( !Slider.IsValid() )
			return false;

		var objParent = a.Object;
		var objTarget = b.Object;

		if ( !objParent.IsValid() || !objTarget.IsValid() )
			return false;

		// Disgraceful..
		if ( objParent == objTarget )
			return false;

		var aPhys = Physics.FindBody( objParent );

		if ( !aPhys.IsValid() || a.Offset is not Offset parentOffset )
			return false;

		var bPhys = Physics.FindBody( objTarget );

		if ( !aPhys.IsValid() || b.Offset is not Offset targetOffset )
			return false;

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );
		var tTargetPoint = objTarget.WorldTransform.WithOffset( targetOffset );

		MaxLength = tParentPoint.Position.Distance( tTargetPoint.Position );

		Slider.MinLength = -MaxLength;
		Slider.MaxLength = 0f;

		var aPhysLocal = aPhys.Transform.ToLocal( tParentPoint );
		var bPhysLocal = bPhys.Transform.ToLocal( tParentPoint );

		// var aPhysPoint = new PhysicsPoint( aPhys, aPhysLocal.Position, aPhysLocal.Rotation );
		// var bPhysPoint = new PhysicsPoint( bPhys, bPhysLocal.Position, bPhysLocal.Rotation );

		if ( !ITransform.IsValid( aPhysLocal ) || !ITransform.IsValid( bPhysLocal ) )
			return false;

		LocalTransform = parentOffset;

		GameObject.SetParent( objParent, keepWorldPosition: true );
		Transform.ClearInterpolation();

		// Let the engine's component handle it.
		Slider.Attachment = Joint.AttachmentMode.LocalFrames;

		Slider.LocalFrame1 = aPhysLocal;
		Slider.LocalFrame2 = bPhysLocal;

		// Set the other body.
		Slider.Body = objTarget;

		return true;
	}
}
