namespace GameFish;

public abstract class ShapePhysics : ControllerPhysics
{
	/// <summary>
	/// Render the shape in the editor?
	/// </summary>
	[Property]
	[Title( "Render Shape" )]
	[Feature( PAWN ), Group( DEBUG )]
	public bool RenderShapeEnabled { get; set; } = true;

	/// <summary>
	/// The color to render the shape as.
	/// </summary>
	[Property]
	[Title( "Shape Color" )]
	[Feature( PAWN ), Group( DEBUG )]
	public virtual Color RenderColor { get; set; } = Color.Cyan;

	public abstract Collider ShapeCollider { get; }

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( RenderShapeEnabled )
			RenderShape();
	}

	/// <summary>
	/// Draw this shape at the current origin.
	/// </summary>
	public abstract void RenderShape();

	protected override void SetupPhysics()
	{
		base.SetupPhysics();

		var rb = Rigidbody;

		if ( rb.IsValid() )
		{
			// Gravity is manually applied.
			rb.Gravity = false;

			// Fuck this garbage default.
			rb.EnableImpactDamage = false;

			// Prevent rotating from the physics engine.
			rb.Locking = rb.Locking with
			{
				Pitch = true,
				Yaw = true,
				Roll = true
			};
		}
	}

	protected override void UpdatePhysics()
	{
		base.UpdatePhysics();

		UpdateCollider();
	}

	/// <summary>
	/// Creates the collider for this shape.
	/// </summary>
	protected virtual void CreateCollider( out GameObject obj )
	{
		obj = null;

		if ( !Scene.IsValid() || !GameObject.IsValid() )
			return;

		// Don't duplicate on accident.
		if ( ShapeCollider.IsValid() && ShapeCollider.GameObject.IsValid() )
		{
			this.Warn( "Prevented creating a duplicate shape collider." );
			obj = ShapeCollider.GameObject;

			return;
		}

		obj = Scene.CreateObject();

		if ( !obj.IsValid() )
			return;

		obj.Name = "Collider";

		obj.Parent = GameObject;
		obj.LocalTransform = global::Transform.Zero;
	}

	/// <summary>
	/// Sets the transform and settings of the collider for this shape.
	/// </summary>
	protected virtual void UpdateCollider()
	{
		if ( !ShapeCollider.IsValid() )
			return;

		ShapeCollider.LocalTransform = TraceOffset;

		// Prevent negation of gravity from pushing into walls.
		ShapeCollider.Friction = 0f;
	}

	[Button( "Create" )]
	[ShowIf( nameof( InEditor ), true )]
	[Feature( PAWN ), Group( COLLISION )]
	protected void ButtonCreateCollider()
		=> CreateCollider( out _ );

	[Button( "Update" )]
	[ShowIf( nameof( InEditor ), true )]
	[Feature( PAWN ), Group( COLLISION )]
	protected void ButtonUpdateCollider()
		=> UpdateCollider();
}
