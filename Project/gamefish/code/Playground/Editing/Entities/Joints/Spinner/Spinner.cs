using System.Text.Json.Serialization;

namespace Playground;

[Icon( "rocket_launch" )]
public partial class Spinner : EditorDevice
{
	[Property, JsonIgnore, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Rigidbody Rigidbody => _rb.GetCached( GameObject, FindMode.InAncestors );
	protected Rigidbody _rb;

	/// <summary>
	/// The local transform of spin origin.
	/// </summary>
	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Offset Offset { get; set; }

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public SpinnerSettings Settings { get; set; }

	[Sync]
	[Property, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public float Direction { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		// Snap to the offset without lag if we're the owner.
		if ( Rigidbody.IsValid() && !Rigidbody.IsProxy )
			TryAttachTo( Rigidbody, Offset );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		DrawPhysicsGizmo();

		if ( IsProxy )
			return;

		var fwd = Input.Keyboard.Down( Settings.KeyForward );
		var back = Input.Keyboard.Down( Settings.KeyReverse );

		if ( fwd && !back )
			Direction = 1f;
		else if ( back && !fwd )
			Direction = -1f;
		else
			Direction = 0f;
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Direction == 0f )
			return;

		ApplyPhysics( Time.Delta, Direction );
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

	protected virtual void DrawPhysicsGizmo()
	{
		var tOrigin = GetPhysicsOrigin( Rigidbody );

		var c = Color.Cyan.WithAlpha( 0.3f );

		this.DrawArrow(
			from: tOrigin.Position,
			to: tOrigin.Position - (tOrigin.Forward * 24f),
			c: c, len: 7f, w: 5f,
			tWorld: global::Transform.Zero
		);
	}

	public virtual void ApplyPhysics( in float deltaTime, float spinDir )
	{
		if ( !Rigidbody.IsValid() || Rigidbody.IsProxy )
			return;

		var tWorld = GetPhysicsOrigin( Rigidbody );

		spinDir = spinDir.Clamp( -1f, 1f );
		var dir = tWorld.Rotation * Vector3.Forward * spinDir;

		var vel = dir * Settings.Speed;

		Rigidbody.AngularVelocity += vel * deltaTime;
	}

	public Transform GetPhysicsOrigin( Rigidbody rb )
	{
		// Fallback transform.
		if ( !rb.IsValid() )
			return WorldTransform;

		// Relative transform.
		return rb.WorldTransform.WithOffset( Offset );
	}

	public virtual bool TryAttachTo( Rigidbody rb, in Offset offs )
	{
		if ( !rb.IsValid() || !rb.GameObject.IsValid() )
			return false;

		WorldTransform = rb.WorldTransform.WithOffset( offs );

		GameObject.SetParent( rb.GameObject, keepWorldPosition: true );

		Transform.ClearInterpolation();

		return true;
	}
}
