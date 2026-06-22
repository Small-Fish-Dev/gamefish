using System;
using System.Threading.Tasks;

namespace GameFish;

[Hide, Obsolete( $"Use {nameof( Trigger )} instead." )]
public partial class BaseTrigger : Trigger;

/// <summary>
/// A trigger volume with callbacks and no filters. <br />
/// Capable of creating, updating and rendering its collision.
/// </summary>
[Title( "Trigger" )]
[Group( Library.NAME )]
[Icon( "highlight_alt" )]
[EditorHandle( "materials/tools/mesh_icons/quad.png" )]
public partial class Trigger : ModuleEntity, Component.ITriggerListener, Component.ExecuteInEditor
{
	protected const int TRIGGER_ORDER = DEFAULT_ORDER - 500;

	protected const int TRIGGER_DEBUG_ORDER = TRIGGER_ORDER - 10;
	protected const int TRIGGER_COLLISION_ORDER = TRIGGER_ORDER + 50;
	protected const int TRIGGER_CALLBACKS_ORDER = TRIGGER_ORDER + 100;

	public enum ColliderType
	{
		/// <summary>
		/// Doesn't create any colliders. Lets you add your own.
		/// </summary>
		Manual = 0,

		/// <summary>
		/// Creates a <see cref="BoxCollider"/>.
		/// </summary>
		Box = 1,

		/// <summary>
		/// Creates a <see cref="SphereCollider"/>.
		/// </summary>
		Sphere = 2,

		/// <summary>
		/// Creates a cylindrical <see cref="HullCollider"/>.
		/// </summary>
		Cylinder = 3,
	}

	/// <summary>
	/// Allows automatically creating, updating and previewing a collider.
	/// </summary>
	[Property]
	[Order( TRIGGER_COLLISION_ORDER )]
	[Feature( TRIGGER ), Group( COLLISION )]
	public virtual ColliderType Collider
	{
		get => _colType ?? DefaultColliderType;
		set
		{
			_colType = value;

			if ( InEditor )
				UpdateColliders();
		}
	}

	protected ColliderType? _colType;

	protected virtual ColliderType DefaultColliderType => ColliderType.Manual;

	public virtual bool UsingBox => Collider is ColliderType.Box;
	public virtual bool UsingSphere => Collider is ColliderType.Sphere;
	public virtual bool UsingCylinder => Collider is ColliderType.Cylinder;

	[Property]
	[Order( TRIGGER_COLLISION_ORDER )]
	[ShowIf( nameof( UsingBox ), true )]
	[Feature( TRIGGER ), Group( COLLISION )]
	public virtual BBox BoxSize
	{
		get => _boxSize ?? DefaultBoxSize;
		set
		{
			_boxSize = value;

			if ( InEditor )
				UpdateColliders();
		}
	}

	protected BBox? _boxSize;

	protected virtual BBox DefaultBoxSize => new( new Vector3( -128f, -128f, -128f ), new Vector3( 128f, 128f, 128f ) );

	[Property]
	[Order( TRIGGER_COLLISION_ORDER )]
	[ShowIf( nameof( UsingSphere ), true )]
	[Feature( TRIGGER ), Group( COLLISION )]
	public float SphereRadius
	{
		get => _sphereRadius;
		set
		{
			_sphereRadius = value;

			if ( InEditor )
				UpdateColliders();
		}
	}

	protected float _sphereRadius = 128f;

	[Property]
	[Order( TRIGGER_COLLISION_ORDER )]
	[ShowIf( nameof( UsingCylinder ), true )]
	[Feature( TRIGGER ), Group( COLLISION )]
	public float CylinderRadius
	{
		get => _cylinderRadius;
		set
		{
			_cylinderRadius = value;

			if ( InEditor )
				UpdateColliders();
		}
	}

	protected float _cylinderRadius = 128f;

	[Property]
	[Order( TRIGGER_COLLISION_ORDER )]
	[ShowIf( nameof( UsingCylinder ), true )]
	[Feature( TRIGGER ), Group( COLLISION )]
	public float CylinderHeight
	{
		get => _cylinderHeight;
		set
		{
			_cylinderHeight = value;

			if ( InEditor )
				UpdateColliders();
		}
	}

	protected float _cylinderHeight = 128f;

	[Property]
	[Range( 3, 32, clamped: true )]
	[Order( TRIGGER_COLLISION_ORDER )]
	[ShowIf( nameof( UsingCylinder ), true )]
	[Feature( TRIGGER ), Group( COLLISION )]
	public int CylinderSides
	{
		get => _cylinderSides;
		set
		{
			_cylinderSides = value;

			if ( InEditor )
				UpdateColliders();
		}
	}

	protected int _cylinderSides = 16;


	/// <summary>
	/// Print debug logs related to triggering?
	/// </summary>
	[Property]
	[Title( "Logging (trigger)" )]
	[Order( TRIGGER_DEBUG_ORDER )]
	[Feature( TRIGGER ), Group( DEBUG )]
	public bool DebugTriggerLogging { get; set; } = false;

	/// <summary>
	/// Render gizmos in play mode?
	/// </summary>
	[Property]
	[Title( "Render (ingame)" )]
	[Order( TRIGGER_DEBUG_ORDER )]
	[Feature( TRIGGER ), Group( DEBUG )]
	public bool DebugRenderInGame { get; set; } = false;

	/// <summary>
	/// Enables overriding the default color for the collider gizmo.
	/// </summary>
	[Property]
	[Title( "Use Custom Color" )]
	[Order( TRIGGER_DEBUG_ORDER )]
	[Feature( TRIGGER ), Group( DEBUG )]
	public bool UseCustomColor { get; set; } = false;

	/// <summary>
	/// Which custom color to use for the collider gizmo(if enabled).
	/// </summary>
	[Property]
	[Title( "Collider Color" )]
	[Order( TRIGGER_DEBUG_ORDER )]
	[Feature( TRIGGER ), Group( DEBUG )]
	[ShowIf( nameof( UseCustomColor ), true )]
	public virtual Color CustomColor { get; set; } = Color.White;

	/// <summary>
	/// The opacity of the solid part of the shape.
	/// </summary>
	[Property]
	[Title( "Solid Alpha" )]
	[Order( TRIGGER_DEBUG_ORDER )]
	[Range( 0f, 1f, clamped: true )]
	[Feature( TRIGGER ), Group( DEBUG )]
	public float DebugGizmoSolidAlpha { get; set; } = 0.05f;


	/// <summary> An object that passed filters just touched this. </summary>
	[Property]
	[Order( TRIGGER_CALLBACKS_ORDER )]
	[Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<Trigger, GameObject> OnEnter { get; set; }

	/// <summary> An object that passed filters just exited this. </summary>
	[Property]
	[Order( TRIGGER_CALLBACKS_ORDER )]
	[Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<Trigger, GameObject> OnExit { get; set; }

	/// <summary> A passing object just entered this as it was previously empty. </summary>
	[Property]
	[Order( TRIGGER_CALLBACKS_ORDER )]
	[Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<Trigger, GameObject> OnFirstEnter { get; set; }

	/// <summary> The only object occupying this trigger just exited. </summary>
	[Property]
	[Order( TRIGGER_CALLBACKS_ORDER )]
	[Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<Trigger, GameObject> OnEmptied { get; set; }

	/// <summary> Called every update for each object within this trigger. </summary>
	[Property]
	[Order( TRIGGER_CALLBACKS_ORDER )]
	[Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<Trigger, GameObject> OnInsideUpdate { get; set; }

	/// <summary> Called every update for each object within this trigger. </summary>
	[Property]
	[Order( TRIGGER_CALLBACKS_ORDER )]
	[Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<Trigger, GameObject> OnInsideFixedUpdate { get; set; }


	/// <summary>
	/// Has <see cref="OnStart"/> been called yet?
	/// </summary>
	public bool Initialized { get; set; }

	/// <summary>
	/// Has this ever once been triggered before?
	/// </summary>
	public bool HasTriggered { get; set; }

	public List<GameObject> Touching { get; set; }

	public BoxCollider Box { get; set; }
	public SphereCollider Sphere { get; set; }
	public HullCollider Cylinder { get; set; }

	/// <summary>
	/// The color of this trigger's gizmos. Supports custom coloring.
	/// </summary>
	public Color GizmoColor => UseCustomColor ? CustomColor : DefaultGizmoColor;
	public virtual Color DefaultGizmoColor { get; } = Color.Green.Desaturate( 0.8f ).Darken( 0.2f );

	public virtual TagSet DefaultTags { get; } = [TAG_TRIGGER];

	protected override Task OnLoad()
	{
		if ( !Scene.IsValid() )
			return base.OnLoad();

		// Update tags immediately.
		Tags?.Add( DefaultTags ?? [] );

		// Give us a collider if we have none.
		UpdateColliders();

		return base.OnLoad();
	}

	protected override void OnStart()
	{
		base.OnStart();

		UpdateColliders();

		Transform.OnTransformChanged += UpdateColliders;

		Initialized = true;
	}

	protected void DebugLog( params object[] log )
	{
		if ( DebugTriggerLogging )
			this.Log( log );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !InGame )
			return;

		UpdateInside();

		RenderTrigger( isGizmo: false );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !InGame )
			return;

		UpdateInsideFixed();
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		RenderTrigger( isGizmo: true );
	}

	protected virtual void RenderTrigger( in bool isGizmo )
	{
		if ( !isGizmo && !DebugRenderInGame )
			return;

		var aSolid = InGame || Gizmo.IsSelected ? 1f : 0.6f;

		var lineColor = GizmoColor;
		var solidColor = lineColor.WithAlpha( DebugGizmoSolidAlpha * aSolid );

		if ( !IsOn )
		{
			lineColor.a *= 0.35f;
			solidColor.a *= 0.2f;
		}

		_ = Collider switch
		{
			ColliderType.Box => this.DrawBox( BoxSize, lineColor, solidColor ),
			ColliderType.Sphere => this.DrawSphere( SphereRadius, Sphere?.Center ?? Vector3.Zero, lineColor, solidColor ),
			ColliderType.Cylinder => this.DrawCylinder( CylinderRadius, CylinderHeight, lineColor, solidColor, CylinderSides ),
			_ => false
		};
	}

	protected virtual void UpdateInside()
	{
		if ( !IsOn )
			return;

		if ( OnInsideUpdate is null || Touching is null )
			return;

		try
		{
			foreach ( var obj in Touching )
				OnInsideUpdate.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnInsideUpdate )} callback exception: {e}" );
		}
	}

	protected virtual void UpdateInsideFixed()
	{
		if ( !IsOn )
			return;

		if ( OnInsideFixedUpdate is null || Touching is null )
			return;

		try
		{
			foreach ( var obj in Touching )
				OnInsideFixedUpdate.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnInsideFixedUpdate )} callback exception: {e}" );
		}
	}

	protected virtual void UpdateColliders()
	{
		if ( !Scene.IsValid() || GameObject.IsDestroyed() )
			return;

		if ( Collider is ColliderType.Manual )
			return;

		// Box
		if ( Collider is ColliderType.Box )
		{
			if ( !Box.IsValid() )
			{
				if ( InEditor )
				{
					Box = Components.GetOrCreate<BoxCollider>( FindMode.EverythingInSelf );

					Box.Scale = BoxSize.Size;
					Box.Center = BoxSize.Mins + BoxSize.Extents;

					Box.IsTrigger = true;
				}
				else
				{
					Box = Components.Get<BoxCollider>( FindMode.EverythingInSelf );
				}
			}

			if ( Box.IsValid() )
				Box.Enabled = IsOn && InGame;
		}
		else if ( Box.IsValid() )
		{
			Box.Destroy();
		}

		// Sphere
		if ( Collider is ColliderType.Sphere )
		{
			if ( !Sphere.IsValid() )
			{
				if ( InEditor )
				{
					Sphere = Components.GetOrCreate<SphereCollider>( FindMode.EverythingInSelf );

					Sphere.Radius = SphereRadius;

					Sphere.IsTrigger = true;
				}
				else
				{
					Sphere = Components.Get<SphereCollider>( FindMode.EverythingInSelf );
				}
			}

			if ( Sphere.IsValid() )
				Sphere.Enabled = IsOn && InGame;
		}
		else if ( Sphere.IsValid() )
		{
			Sphere.Destroy();
		}

		// Cylinder
		if ( Collider is ColliderType.Cylinder )
		{
			if ( !Cylinder.IsValid() )
			{
				if ( InEditor )
				{
					Cylinder = Components.GetOrCreate<HullCollider>( FindMode.EverythingInSelf );

					Cylinder.Radius = CylinderRadius;
					Cylinder.Radius2 = CylinderRadius;
					Cylinder.Height = CylinderHeight;
					Cylinder.Slices = CylinderSides;

					Cylinder.Type = HullCollider.PrimitiveType.Cylinder;

					Cylinder.IsTrigger = true;
				}
				else
				{
					Cylinder = Components.Get<HullCollider>( FindMode.EverythingInSelf );
				}
			}

			if ( Cylinder.IsValid() )
				Cylinder.Enabled = IsOn && InGame;
		}
		else if ( Cylinder.IsValid() )
		{
			Cylinder.Destroy();
		}
	}

	void ITriggerListener.OnTriggerEnter( GameObject obj )
	{
		if ( !TestFilters( obj ) )
			return;

		OnTouchStart( obj );
	}

	void ITriggerListener.OnTriggerExit( GameObject obj )
	{
		if ( obj is not null && (Touching?.Contains( obj ) ?? false) )
			OnTouchStop( obj );
	}

	/// <summary>
	/// Run filtering checks and optional debug logging.
	/// </summary>
	protected virtual bool TestFilters( GameObject obj )
	{
		if ( !PassesFilters( obj ) )
		{
			if ( DebugTriggerLogging )
				DebugLog( obj + " FAILED the filter " );

			return false;
		}

		if ( DebugTriggerLogging )
			DebugLog( obj + " PASSED the filter" );

		return true;
	}

	/// <returns> If the object passes this trigger's filters(if any). </returns>
	public virtual bool PassesFilters( GameObject obj )
	{
		return obj.IsValid();
	}

	/// <summary>
	/// Called when an object passing our filters has entered this trigger.
	/// </summary>
	protected virtual void OnTouchStart( GameObject obj )
	{
		Touching ??= [];

		var firstTouch = !Touching.Any( obj => obj.IsValid() );

		if ( !Touching.Contains( obj ) )
			Touching.Add( obj );

		try
		{
			if ( firstTouch )
				OnFirstEntered( obj );

			OnEnter?.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnEnter )} callback exception: {e}" );
		}

		// Let 'em know.
		HasTriggered = true;
	}

	/// <summary>
	/// Called when an object that previously passed our filter leaves.
	/// </summary>
	protected virtual void OnTouchStop( GameObject obj )
	{
		Touching?.Remove( obj );

		// Validate
		Touching?.RemoveAll( obj => !PassesFilters( obj ) );

		if ( Touching is null || Touching.Count <= 0 )
			OnLastExit( obj );

		try
		{
			OnExit?.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnExit )} callback exception: {e}" );
		}
	}

	/// <summary>
	/// Called when this is empty(of filter-passing objects) and an object(that is filter-passing) touches it.
	/// </summary>
	protected virtual void OnFirstEntered( GameObject obj )
	{
		try
		{
			OnFirstEnter?.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnFirstEnter )} callback exception: {e}" );
		}
	}

	protected virtual void OnLastExit( GameObject obj )
	{
		try
		{
			OnEmptied?.Invoke( this, obj );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnEmptied )} callback exception: {e}" );
		}
	}
}
