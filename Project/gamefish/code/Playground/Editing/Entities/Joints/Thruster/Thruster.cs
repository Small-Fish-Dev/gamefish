using System.Text.Json.Serialization;

namespace Playground;

[Icon( "rocket_launch" )]
public partial class Thruster : Entity
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;
	protected const int PHYSICS_ORDER = EDITOR_ORDER + 10;

	[Property, JsonIgnore, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Rigidbody Rigidbody => _rb.GetCached( GameObject, FindMode.InAncestors );
	protected Rigidbody _rb;

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Offset Offset { get; set; }

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public ThrusterSettings Settings { get; set; }

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

		DrawThrusterGizmo();

		if ( IsProxy )
			return;

		var fwd = Input.Keyboard.Down( Settings.KeyForward );
		var back = Input.Keyboard.Down( Settings.KeyBackward );

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

		// Auto-cleanup empty objects that only had a thruster.
		var comps = GameObject.Components.GetAll( FindMode.EverythingInSelf )
			.Where( comp => comp.IsValid() );

		if ( !comps.Any() )
			GameObject.Destroy();
	}

	protected virtual void DrawThrusterGizmo()
	{
		var tOrigin = GetPhysicsOrigin( Rigidbody );

		var c = Color.Orange.WithAlpha( 0.3f );

		this.DrawArrow(
			from: tOrigin.Position - (tOrigin.Forward * 10f),
			to: tOrigin.Position,
			c: c, len: 7f, w: 5f,
			tWorld: global::Transform.Zero
		);
	}

	public virtual void ApplyPhysics( in float deltaTime, float thrustDir )
	{
		if ( !Rigidbody.IsValid() || Rigidbody.IsProxy )
			return;

		var tWorld = GetPhysicsOrigin( Rigidbody );

		thrustDir = thrustDir.Clamp( -1f, 1f );
		var dir = tWorld.Rotation * Vector3.Forward * thrustDir;

		var vel = dir * Settings.Force;
		vel *= Rigidbody.Mass;

		Rigidbody.ApplyForceAt( tWorld.Position, vel );
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
