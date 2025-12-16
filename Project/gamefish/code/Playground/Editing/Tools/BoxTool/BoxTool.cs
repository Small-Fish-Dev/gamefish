using System.ComponentModel;
using System.Drawing;

namespace Playground;

public partial class BoxTool : EditorTool
{
	[Property]
	[Title( "Box" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile BoxPrefab { get; set; }

	[Property]
	[Title( "Box Size" )]
	[Range( 1f, 1024f, clamped: false )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public float BoxSize { get; set; } = 50f;

	[Property]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float ScrollSensitivity { get; set; } = 4f;

	/// <summary>
	/// The current target height of the box.
	/// </summary>
	[Property]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float PlaceHeight { get; set; } = 32f;

	[Property]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float HeightLimit { get; set; } = 1024f;

	/// <summary>
	/// If defined: the starting point for the shape.
	/// </summary>
	public Vector3? StartPoint { get; set; }

	public static bool PressedPoint => Input.Pressed( "Attack1" );
	public static bool PressedCancel => Input.Pressed( "Attack2" );

	public override void OnExit()
	{
		base.OnExit();

		StopShaping();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		UpdatePlace( in deltaTime );
		UpdateCancel( in deltaTime );
	}

	protected virtual void UpdateCancel( in float deltaTime )
	{
		if ( PressedCancel )
			StopShaping();
	}

	protected virtual void UpdatePlace( in float deltaTime )
	{
		if ( !Mouse.Active || !TryTrace( out var tr ) )
			return;

		// Height Adjustment
		var yScroll = Input.MouseWheel.y;

		if ( yScroll != 0f )
		{
			var scrollHeight = yScroll * ScrollSensitivity;
			PlaceHeight = (PlaceHeight + scrollHeight).Clamp( -HeightLimit, HeightLimit );
		}

		// Point Height
		var point = tr.EndPosition;

		if ( StartPoint is Vector3 firstPoint )
			point.z = firstPoint.z + PlaceHeight;

		// Point Placement
			if ( PressedPoint )
				TryPlacePoint( point );

		// Shape Helper Rendering
		var c1 = Color.White.WithAlpha( 0.4f );
		var c2 = Color.White.WithAlpha( 0.1f );

		this.DrawSphere( 2f, point, Color.Transparent, c1, global::Transform.Zero );

		if ( StartPoint is Vector3 start )
		{
			var bounds = BBox.FromPoints( [start, point] )
				.Grow( -0.1f ); // lessen z-fighting

			if ( bounds.Volume > 1f )
			{
				this.DrawArrow(
					from: start, to: point,
					c: c1, len: 3f, w: 1f,
					tWorld: global::Transform.Zero
				);

				this.DrawBox( bounds, c1, c2, global::Transform.Zero );
			}
		}
	}

	protected virtual void StopShaping()
	{
		StartPoint = null;
	}

	protected virtual bool TryPlacePoint( in Vector3 point )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( StartPoint is Vector3 start )
		{
			if ( !TryCreateBox( start, in point, out _ ) )
				return false;

			StopShaping();

			return true;
		}

		StartPoint = point;

		return true;
	}

	protected virtual bool TryCreateBox( in Vector3 a, in Vector3 b, out GameObject objBox )
	{
		var bounds = BBox.FromPoints( [a, b] );

		if ( bounds.Volume < 1f )
		{
			objBox = null;
			return false;
		}

		var center = bounds.Center;
		var scale = bounds.Size / BoxSize.Max( 1f );

		var tBox = new Transform( center, Rotation.Identity, scale );

		if ( !BoxPrefab.TrySpawn( tBox, out objBox ) )
			return false;

		objBox.NetworkSetup(
			cn: Connection.Local,
			orphanMode: NetworkOrphaned.ClearOwner,
			ownerTransfer: OwnerTransfer.Takeover,
			netMode: NetworkMode.Object,
			ignoreProxy: true
		);

		return true;
	}
}
