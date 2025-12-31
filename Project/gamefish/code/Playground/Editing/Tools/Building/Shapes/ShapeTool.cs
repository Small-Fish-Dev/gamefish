using System.Text.Json.Serialization;

namespace Playground;

/// <summary>
/// Makes shapes. Previews said shape.
/// </summary>
public abstract class ShapeTool : EditorTool
{
	[Property]
	[Title( "Shape" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER - 2 )]
	public PrefabFile ShapePrefab { get; set; }

	[Property]
	[ToolSetting]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float ScrollSensitivity { get; set; } = 32f;

	[Property]
	[ToolSetting]
	[Range( 0f, 4096f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float Distance { get; set; } = 512f;

	[Property]
	[Range( 0f, 4096f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public FloatRange DistanceRange { get; set; } = new( 16f, 2048f );

	/// <summary>
	/// The coordinates of the shape(in local or world space).
	/// </summary>
	[Property, JsonIgnore, WideMode]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Group( DEBUG ), Order( EDITOR_DEBUG_ORDER )]
	public List<(Vector3 Position, Rotation Rotation)> Points { get; set; }

	/// <summary>
	/// Are our <see cref="Points"/> not null with at least one point defined?
	/// </summary>
	public bool HasPoints => Points?.Count > 0;

	/// <summary>
	/// Do we have enough(or too many) <see cref="Points"/>?
	/// </summary>
	public bool AtLimit => HasPoints && Points.Count >= PointLimit;

	/// <summary>
	/// Can something be made of this shape?
	/// </summary>
	public virtual bool ValidShape => Points?.Count >= 2;

	/// <summary>
	/// The maximum number of points for making this shape.
	/// </summary>
	public abstract int PointLimit { get; }

	public override void OnExit()
	{
		base.OnExit();

		Clear();
	}

	protected override void Clear()
	{
		base.Clear();

		Points?.Clear();
	}

	public override bool TryRightClick()
	{
		if ( HasPoints )
		{
			Clear();
			return true;
		}

		return false;
	}

	public override bool TryTrace( out SceneTraceResult tr )
		=> Editor.TryTrace( Scene, out tr, dist: Distance );

	protected override bool TryGetPointer( in SceneTraceResult tr, out Transform tCursor )
	{
		if ( !base.TryGetPointer( in tr, out tCursor ) )
			return false;

		if ( HoldingShift )
			tCursor.Position = tCursor.Position.SnapToGrid( 16f );

		return true;
	}

	protected override void OnPrimary( in SceneTraceResult tr )
	{
		if ( !TryGetPointer( in tr, out var tCursor ) )
			return;

		if ( !HasPoints )
		{
			if ( HasTarget && TargetObject.IsValid() )
			{
				var tWorld = TargetObject.WorldTransform;
				var tLocal = tWorld.ToLocal( tCursor );

				SetOrigin( tLocal, TargetObject, TargetComponent );
			}
			else
			{
				var vWorldAxis = Rotation.Identity.ClosestAxis( tr.Direction );
				SetOrigin( new( tr.EndPosition, Rotation.LookAt( vWorldAxis ) ) );
			}
		}

		TryAddWorldPoint( new( tCursor.Position, Rotation.Identity ) );
	}

	public override bool TryMouseWheel( in Vector2 dir )
	{
		var scroll = dir.y != 0f ? -dir.y : dir.x;
		scroll *= ScrollSensitivity;

		OnScroll( in scroll );

		return true;
	}

	protected virtual void OnScroll( in float scroll )
	{
		if ( !Mouse.Active )
			return;

		Distance = (Distance + scroll).Clamp( DistanceRange );
	}

	protected override void OnReload( in SceneTraceResult tr )
	{
		base.OnReload( tr );

		Clear();
	}

	protected override void RenderHelpers()
	{
		base.RenderHelpers();

		RenderShape();
	}

	protected virtual void RenderShape()
	{
		RenderBoxShape();
	}

	protected virtual void RenderBoxShape()
	{
		if ( !HasPoints || !HasOrigin )
			return;

		var tOrigin = GetShapeOrigin();

		var points = Points?.Select( pr => pr.Position ).ToList();

		if ( TryTrace( out var tr ) && TryGetPointer( in tr, out var tCursor ) )
			points.Add( tOrigin.PointToLocal( tCursor.Position ) );

		var box = BBox.FromPoints( points );
		box = BBox.FromPositionAndSize( box.Center, box.Size - 0.1f );

		this.DrawBox( box, ColorOutline, ColorFilled, tOrigin );
	}

	protected virtual bool TryAddWorldPoint( Transform tWorld )
	{
		// Floating origin.
		if ( !HasOrigin && !HasPoints )
			SetOrigin( new( tWorld.Position, Rotation.Identity ) );

		var tOrigin = GetShapeOrigin();
		var tLocal = tOrigin.ToLocal( in tWorld );

		return TryAddLocalPoint( tLocal );
	}

	protected virtual bool TryAddLocalPoint( Transform tLocal )
	{
		AddPoint( tLocal.Position, tLocal.Rotation );
		return true;
	}

	protected virtual void AddPoint( Vector3 pos, Rotation r )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return;

		Points ??= [];

		if ( AtLimit )
			Points.RemoveAt( Points.Count - 1 );

		if ( !AtLimit )
			Points.Add( (pos, r) );

		OnPointAdded( pos, r );
	}

	protected virtual void OnPointAdded( in Vector3 pos, in Rotation r )
	{
		if ( AtLimit )
		{
			TryCreateShape( out _ );
			Clear();
		}
	}

	public virtual Transform GetShapeOrigin( in Transform? tOverride = null )
	{
		// Might be snapping to something.
		if ( !HasOrigin )
		{
			if ( tOverride.HasValue )
				return tOverride.Value;

			// fallback or something idk
			if ( HasTarget && TargetObject.IsValid() )
				return TargetObject.WorldTransform;

			return global::Transform.Zero;
		}

		var tOrigin = tOverride
			?? OriginObject.AsValid()?.WorldTransform
			?? OriginWorldTransform;

		tOrigin = tOrigin.WithOffset( OriginOffset );

		return tOrigin;
	}

	protected virtual bool TryGetShapeTransform( out Transform tWorld )
	{
		var tOrigin = GetShapeOrigin();
		tWorld = tOrigin;

		if ( !HasPoints || !ValidShape )
			return false;

		var points = Points?.Select( pr => pr.Position ).ToList() ?? [];
		var box = BBox.FromPoints( points );

		// If the shape is too small then prevent it to stop physics bugs.
		var size = box.Size.Abs();

		if ( size.x <= 1 || size.y <= 1 || size.z <= 1 )
			return false;

		tWorld = tOrigin.ToWorld( new( box.Center, Rotation.Identity, box.Size ) );

		return true;
	}

	protected virtual bool TryCreateShape( out GameObject obj )
	{
		obj = null;

		if ( !TryGetShapeTransform( out var tShape ) )
			return false;

		if ( Editor.TryFindIsland( OriginObject, out var island ) )
			if ( TrySpawnObject( ShapePrefab, tShape, island, out _ ) )
				return true;

		if ( TrySpawnObject( ShapePrefab, tShape, out _, withIsland: true ) )
			return true;

		return false;
	}
}
