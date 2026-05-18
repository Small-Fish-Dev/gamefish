namespace GameFish;

/// <summary>
/// Applies gravitational pull from gravity fields.
/// </summary>
[Icon( "balance" )]
public class GravityModule : Module
{
	protected const int GRAVITY_ORDER = ENTITY_ORDER - 1000;

	public override bool IsParent( ModuleEntity comp )
		=> comp is DynamicEntity;

	public DynamicEntity Entity => Parent as DynamicEntity;
	public IGravity GravityObject => Parent as IGravity;

	public override Vector3 Center => Entity?.Center ?? base.Center;

	/// <summary>
	/// If true: applies gravity continuously by itself.
	/// <br /> <br />
	/// <b> NOTE: </b> Might double gravity if it's not disabled on the parent entity's <see cref="Rigidbody"/>.
	/// </summary>
	[Property]
	[Feature( GRAVITY )]
	[Title( "Auto-Apply" )]
	[Order( GRAVITY_ORDER )]
	public bool AutoApply { get; set; } = true;

	/// <summary>
	/// If true: apply default gravity if not in a field.
	/// </summary>
	[Property]
	[Feature( GRAVITY )]
	[Title( "Use Default" )]
	[Order( GRAVITY_ORDER )]
	public bool UseDefault { get; set; } = true;

	/// <summary>
	/// If defined: toggles gravity on the parent entity's <see cref="Rigidbody"/>.
	/// </summary>
	[Property]
	[Feature( GRAVITY )]
	[Order( GRAVITY_ORDER )]
	public bool? BodyGravity { get; set; } = false;

	protected virtual Vector3 Velocity
	{
		get => Entity?.Velocity ?? default;
		set => Entity?.Velocity = value;
	}

	protected virtual Vector3 DefaultGravity => GravityObject?.Gravity ?? Scene?.PhysicsWorld?.Gravity ?? default;

	/// <summary>
	/// The gravity field overriding the rest.
	/// </summary>
	[Sync]
	public GravityField Field
	{
		get => _field;
		set
		{
			if ( _field == value )
				return;

			_field = value;
			OnSetField( in value );
		}
	}

	protected GravityField _field;

	[Sync]
	public NetList<GravityField> Within { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( BodyGravity is bool isEnabled )
			ToggleGravity( isEnabled );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( AutoApply && InGame )
			ApplyGravity( Time.Delta );
	}

	protected virtual void ToggleGravity( in bool isEnabled )
	{
		var rb = Entity?.Rigidbody;

		if ( rb.IsValid() )
			rb.Gravity = isEnabled;
	}

	public virtual void ApplyGravity( in float deltaTime )
	{
		if ( Field.IsValid() )
			Velocity += Field.GetForce( Center ) * deltaTime;
		else if ( UseDefault )
			Velocity += DefaultGravity * deltaTime;
	}

	public virtual void OnEnter( GravityField field )
	{
		if ( !field.IsValid() )
			return;

		if ( !IsProxy )
		{
			Field = field;

			Within ??= [];
			Within?.Add( field );
		}
	}

	public virtual void OnExit( GravityField field )
	{
		if ( IsProxy )
			return;

		if ( Field == field )
			Field = null;

		Within?.Remove( field );

		ValidateFields();
	}

	public virtual void ValidateFields()
	{
		if ( IsProxy )
			return;

		var bad = Within?.Where( f => !f.IsValid() );

		if ( bad?.Any() is not true )
			return;

		foreach ( var f in bad.ToArray() )
			Within?.Remove( f );
	}

	protected virtual void OnSetField( in GravityField field )
	{
	}
}
