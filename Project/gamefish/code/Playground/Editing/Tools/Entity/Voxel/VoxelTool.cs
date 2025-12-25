using Boxfish.Library;
using Boxfish.Utility;
using Microsoft.VisualBasic;

namespace Playground;

public partial class VoxelTool : EditorTool
{
	[Property]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile VoxelPrefab { get; set; }

	public NetworkedVoxelVolume TargetVolume => Scene?.Get<NetworkedVoxelVolume>();

	public Vector3? TargetPosition { get; set; }

	public override void OnExit()
	{
		base.OnExit();

		ClearTarget();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		if ( !Mouse.Active )
			return;

		UpdateTarget( in deltaTime );

		UpdatePlacing();
		UpdateBreaking();
	}

	public override bool TryLeftClick()
		=> true;

	/*
	public override bool TryTrace( out SceneTraceResult tr )
	{
		if ( !Editor.TryGetAimRay( Scene, out var ray ) )
			return base.TryTrace( out tr );

		tr = Scene.Trace.Box( bounds.Extents, ray, Editor.TRACE_DISTANCE_DEFAULT )
			.IgnoreGameObjectHierarchy( Client.Local?.Pawn?.GameObject )
			.Rotated( GetPrefabRotation() )
			.Run();

		return true;
	}
	*/

	protected void ClearTarget()
	{
		// TargetVolume = null;
		TargetPosition = null;
	}

	protected void UpdatePlacing()
	{
		if ( !PressedPrimary )
			return;

		if ( !TryGetGrid( out _ ) )
		{
			RpcHostCreateGrid();
			return;
		}

		TryPlaceVoxel( TargetPosition );
	}

	protected void UpdateBreaking()
	{
		if ( !PressedSecondary )
			return;

		if ( !TryTrace( out var tr ) || !tr.Hit )
			return;

		if ( !TryGetGrid( out var v ) || TargetPosition is not Vector3 pos )
			return;

		TryBreakVoxel( pos );
	}

	protected virtual void UpdateTarget( in float deltaTime )
	{
		ClearTarget();

		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) || !tr.Hit )
			return;

		if ( !TryGetGrid( out var v ) )
			return;

		TargetPosition = tr.EndPosition;

		var c1 = Color.Black.WithAlpha( 0.5f );
		var c2 = Color.White.WithAlpha( 0.02f );

		var placePos = GetPlaceWorldPosition( in tr, v.Scale );
		var bounds = BBox.FromPositionAndSize( Vector3.Zero, v.Scale )
			.Grow( -0.01f );

		this.DrawBox( bounds, c1, c2, tWorld: new( placePos ) );
	}

	public virtual Vector3 GetGridSnappedPosition( in Vector3 worldPos, in float voxelScale )
	{
		var x = (worldPos.x / voxelScale).Round() * voxelScale;
		var y = (worldPos.y / voxelScale).Round() * voxelScale;
		var z = (worldPos.z / voxelScale).Round() * voxelScale;

		return new( x, y, z );
	}

	public virtual Vector3 GetPlaceWorldPosition( in SceneTraceResult tr, in float voxelScale )
	{
		var offset = tr.Normal * (voxelScale / 2f).Min( 1f );
		var worldPos = tr.EndPosition + offset;

		return GetGridSnappedPosition( worldPos, in voxelScale );
	}

	public virtual Vector3 GetBlockWorldPosition( in SceneTraceResult tr, in float voxelScale )
	{
		var offset = tr.Normal * (voxelScale / 2f).Min( 1f );
		var worldPos = tr.EndPosition - offset;

		return GetGridSnappedPosition( worldPos, in voxelScale );
	}

	protected virtual bool TryPlaceVoxel( in Vector3? worldPos )
	{
		if ( !TryTrace( out var tr ) )
			return false;

		if ( !TryGetGrid( out var v ) )
		{
			RpcHostCreateGrid();
			return false;
		}

		if ( !worldPos.HasValue )
			return false;

		var placePos = GetPlaceWorldPosition( in tr, v.Scale );
		var voxPos = v.WorldToVoxel( placePos );

		// Random color.
		var hue = Random.Float( 0f, 360f );
		var color = new ColorHsv( hue, 0.7f, 0.9f ).ToColor();

		v.BroadcastSet( voxPos, new( color ) );

		return true;
	}

	protected virtual bool TryBreakVoxel( in Vector3? worldPos )
	{
		if ( !TryTrace( out var tr ) )
			return false;

		if ( !TryGetGrid( out var v ) )
			return false;

		if ( !worldPos.HasValue )
			return false;

		var breakPos = GetBlockWorldPosition( in tr, v.Scale );
		var voxPos = v.WorldToVoxel( breakPos );

		v.BroadcastSet( voxPos, Voxel.Empty );

		return true;
	}

	protected virtual bool TryGetGrid( out NetworkedVoxelVolume v )
	{
		v = Scene?.Get<NetworkedVoxelVolume>();
		return v.IsValid();
	}

	/// <summary>
	/// Asks the host to create a voxel grid if there isn't one.
	/// </summary>
	[Rpc.Host]
	protected virtual void RpcHostCreateGrid()
	{
		if ( !TryUse( Rpc.Caller, out _ ) )
			return;

		if ( TryGetGrid( out _ ) )
			return;

		var gridOrigin = Vector3.Zero;

		if ( !VoxelPrefab.TrySpawn( gridOrigin, out var obj ) )
		{
			this.Warn( $"Missing/invalid voxel prefab:[{VoxelPrefab}]!" );
			return;
		}

		if ( !obj.Components.TryGet( out NetworkedVoxelVolume v ) )
		{
			this.Warn( $"Missing {typeof( NetworkedVoxelVolume )} on voxel grid:[{obj}]!" );
			obj.Destroy();
			return;
		}

		this.Log( $"Created new voxel grid:[{v}]" );

		obj.NetworkSetup(
			cn: Connection.Host,
			orphanMode: NetworkOrphaned.Host,
			ownerTransfer: OwnerTransfer.Fixed,
			netMode: NetworkMode.Object,
			ignoreProxy: true
		);
	}
}
