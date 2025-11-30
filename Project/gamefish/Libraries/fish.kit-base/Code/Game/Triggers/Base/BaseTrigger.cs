using System;
using System.Threading.Tasks;

namespace GameFish;

/// <summary>
/// A trigger volume with callbacks and no filters. <br />
/// Capable of creating, updating and rendering its collision.
/// </summary>
[Title( "Trigger" )]
[Group( Library.NAME )]
[Icon( "highlight_alt" )]
[EditorHandle( "materials/tools/mesh_icons/quad.png" )]
public partial class BaseTrigger : ModuleEntity, Component.ITriggerListener, Component.ExecuteInEditor
{
	protected const int TRIGGER_ORDER = DEFAULT_ORDER - 50;
	protected const int CALLBACKS_ORDER = 42069;

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
	[Property, Order( TRIGGER_ORDER )]
	[Feature( TRIGGER ), Group( COLLISION )]
	public virtual ColliderType Collider
	{
		get => _colType;
		set { _colType = value; UpdateColliders(); }
	}

	protected ColliderType _colType = ColliderType.Manual;

	public virtual bool UsingBox => Collider is ColliderType.Box;
	public virtual bool UsingSphere => Collider is ColliderType.Sphere;
	public virtual bool UsingCylinder => Collider is ColliderType.Cylinder;

	[ShowIf( nameof( UsingBox ), true )]
	[Property, Feature( TRIGGER ), Group( COLLISION )]
	public virtual BBox BoxSize
	{
		get => _boxSize;
		set { _boxSize = value; UpdateColliders(); }
	}

	protected BBox _boxSize = new( new Vector3( -128f, -128f, -128f ), new Vector3( 128f, 128f, 128f ) );

	[ShowIf( nameof( UsingSphere ), true )]
	[Property, Feature( TRIGGER ), Group( COLLISION )]
	public float SphereRadius
	{
		get => _sphereRadius;
		set
		{
			_sphereRadius = value;
			UpdateColliders();
		}
	}

	protected float _sphereRadius = 128f;

	[ShowIf( nameof( UsingCylinder ), true )]
	[Property, Feature( TRIGGER ), Group( COLLISION )]
	public float CylinderRadius
	{
		get => _cylinderRadius;
		set
		{
			_cylinderRadius = value;
			UpdateColliders();
		}
	}

	protected float _cylinderRadius = 128f;

	[ShowIf( nameof( UsingCylinder ), true )]
	[Property, Feature( TRIGGER ), Group( COLLISION )]
	public float CylinderHeight
	{
		get => _cylinderHeight;
		set
		{
			_cylinderHeight = value;
			UpdateColliders();
		}
	}

	protected float _cylinderHeight = 128f;

	[Range( 3, 32, clamped: true )]
	[ShowIf( nameof( UsingCylinder ), true )]
	[Property, Feature( TRIGGER ), Group( COLLISION )]
	public int CylinderSides
	{
		get => _cylinderSides;
		set
		{
			_cylinderSides = value;
			UpdateColliders();
		}
	}

	protected int _cylinderSides = 16;


	/// <summary>
	/// Print debug logs related to triggering?
	/// </summary>
	[Property, Feature( TRIGGER ), Group( DEBUG )]
	public bool DebugTrigger { get; set; } = false;

	/// <summary>
	/// Render gizmos in play mode?
	/// </summary>
	[Property, Feature( TRIGGER ), Group( DEBUG )]
	public bool DebugGizmos { get; set; } = false;

	/// <summary>
	/// Enables overriding the default color for the collider gizmo.
	/// </summary>
	[Title( "Use Custom Color" )]
	[Property, Feature( TRIGGER ), Group( DEBUG )]
	public bool UseCustomColor { get; set; } = false;

	/// <summary>
	/// Which custom color to use for the collider gizmo(if enabled).
	/// </summary>
	[Title( "Collider Color" )]
	[Property, Feature( TRIGGER ), Group( DEBUG )]
	[ShowIf( nameof( UseCustomColor ), true )]
	public virtual Color CustomColor { get; set; } = Color.White;

	/// <summary>
	/// The opacity of the solid part of the shape.
	/// </summary>
	[Title( "Solid Alpha" )]
	[Range( 0f, 1f, clamped: true )]
	[Property, Feature( TRIGGER ), Group( DEBUG )]
	public float DebugGizmoSolidAlpha { get; set; } = 0f;


	/// <summary> An object that passed filters just touched this. </summary>
	[Order( CALLBACKS_ORDER )]
	[Property, Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<BaseTrigger, GameObject> OnEnter { get; set; }

	/// <summary> An object that passed filters just exited this. </summary>
	[Order( CALLBACKS_ORDER )]
	[Property, Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<BaseTrigger, GameObject> OnExit { get; set; }

	/// <summary> A passing object just entered this as it was previously empty. </summary>
	[Order( CALLBACKS_ORDER )]
	[Property, Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<BaseTrigger, GameObject> OnFirstEnter { get; set; }

	/// <summary> The only object occupying this trigger just exited. </summary>
	[Order( CALLBACKS_ORDER )]
	[Property, Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<BaseTrigger, GameObject> OnEmptied { get; set; }

	/// <summary> Called every update for each object within this trigger. </summary>
	[Order( CALLBACKS_ORDER )]
	[Property, Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<BaseTrigger, GameObject> OnInsideUpdate { get; set; }

	/// <summary> Called every update for each object within this trigger. </summary>
	[Order( CALLBACKS_ORDER )]
	[Property, Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<BaseTrigger, GameObject> OnInsideFixedUpdate { get; set; }


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
		if ( DebugTrigger )
			this.Log( log );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( DebugGizmos )
			DrawTriggerGizmos();

		if ( this.InEditor() )
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

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( this.InEditor() )
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
		if ( !this.IsValid() || !Scene.IsValid() )
			return;

		if ( Collider is ColliderType.Manual )
			return;

		// Box
		if ( Collider is ColliderType.Box )
		{
			if ( !Box.IsValid() )
				Box = Components.GetOrCreate<BoxCollider>( FindMode.EverythingInSelf );

			Box.Scale = BoxSize.Size;
			Box.Center = BoxSize.Mins + BoxSize.Extents;

			Box.Enabled = !this.InEditor();
			Box.IsTrigger = true;
		}
		else if ( Box.IsValid() )
		{
			Box.Enabled = false;
		}

		// Sphere
		if ( Collider is ColliderType.Sphere )
		{
			if ( !Sphere.IsValid() )
				Sphere = Components.GetOrCreate<SphereCollider>( FindMode.EverythingInSelf );

			Sphere.Radius = SphereRadius;

			Sphere.Enabled = !this.InEditor();
			Sphere.IsTrigger = true;
		}
		else if ( Sphere.IsValid() )
		{
			Sphere.Enabled = false;
		}

		// Cylinder
		if ( Collider is ColliderType.Cylinder )
		{
			if ( !Cylinder.IsValid() )
				Cylinder = Components.GetOrCreate<HullCollider>( FindMode.EverythingInSelf );

			Cylinder.Radius = CylinderRadius;
			Cylinder.Radius2 = CylinderRadius;
			Cylinder.Height = CylinderHeight;
			Cylinder.Slices = CylinderSides;

			Cylinder.Type = HullCollider.PrimitiveType.Cylinder;

			Cylinder.IsTrigger = true;
			Cylinder.Enabled = !this.InEditor();
		}
		else if ( Cylinder.IsValid() )
		{
			Cylinder.Enabled = false;
		}
	}

	public void OnTriggerEnter( GameObject obj )
	{
		if ( !TestFilters( obj ) )
			return;

		OnTouchStart( obj );
	}

	public void OnTriggerExit( GameObject obj )
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
			if ( DebugTrigger )
				DebugLog( obj + " FAILED the filter " );

			return false;
		}

		if ( DebugTrigger )
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

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		DrawTriggerGizmos();
	}

	public virtual void DrawTriggerGizmos()
	{
		var aSolid = this.InGame() || Gizmo.IsSelected ? 1f : 0.6f;

		var lineColor = GizmoColor;
		var solidColor = lineColor.WithAlpha( DebugGizmoSolidAlpha * aSolid );

		_ = Collider switch
		{
			ColliderType.Box => this.DrawBox( BoxSize, lineColor, solidColor ),
			ColliderType.Sphere => this.DrawSphere( SphereRadius, Sphere?.Center ?? Vector3.Zero, lineColor, solidColor ),
			ColliderType.Cylinder => this.DrawCylinder( CylinderRadius, CylinderHeight, lineColor, solidColor, CylinderSides ),
			_ => false
		};
	}
}
