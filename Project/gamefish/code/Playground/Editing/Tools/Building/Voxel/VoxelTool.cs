using Boxfish.Library;
using Boxfish.Utility;
using Microsoft.VisualBasic;

namespace Playground;

public partial class VoxelTool : ShapeTool
{
	[Property]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile VoxelPrefab { get; set; }

	public NetworkedVoxelVolume TargetVolume => Scene?.Get<NetworkedVoxelVolume>();

	public override int PointLimit => 2;

	public override void OnExit()
	{
		base.OnExit();

		ClearTarget();
	}

	protected override void RenderPointer()
	{
		base.RenderPointer();

		if ( !TargetVolume.IsValid() )
			return;

		var placePos = GetPlaceWorldPosition( TargetTrace, TargetVolume.VoxelScale );
		var mins = TargetVolume.WorldToVoxel( placePos );

		DrawVoxelBox( TargetVolume, mins, mins + 1 );
	}

	protected void DrawVoxelBox( NetworkedVoxelVolume v, in Vector3Int mins, in Vector3Int maxs )
	{
		if ( !v.IsValid() )
			return;

		var tVox = v.WorldTransform;

		var bounds = new BBox( mins, maxs ) * v.VoxelScale;

		bounds = bounds.Translate( v.VoxelScale * -0.5f );
		bounds = bounds.Grow( -0.01f );

		this.DrawBox( bounds, ColorOutline, ColorFilled, tWorld: tVox );
	}

	protected override void OnPrimary( in SceneTraceResult tr )
	{
		// base.OnPrimary( tr );

		TryPlaceVoxel( in tr );
	}

	protected override void OnSecondary( in SceneTraceResult tr )
	{
		// base.OnSecondary( tr );

		TryBreakVoxel( in tr );
	}

	protected override void FindTarget( bool clearPrevious = true )
	{
		base.FindTarget( clearPrevious );
	}

	public override bool TryGetTargetComponent( in SceneTraceResult tr, out Component target )
	{
		target = TargetVolume;

		return target.IsValid();
	}

	public virtual Vector3 GetGridSnappedPosition( in Vector3 pos, in float voxelScale )
	{
		var x = (pos.x / voxelScale).Round() * voxelScale;
		var y = (pos.y / voxelScale).Round() * voxelScale;
		var z = (pos.z / voxelScale).Round() * voxelScale;

		return new( x, y, z );
	}

	public virtual Vector3 GetPlaceWorldPosition( in SceneTraceResult tr, in float voxelScale )
	{
		var offset = tr.Normal * (voxelScale / 2f).Min( 1f );
		var worldPos = tr.EndPosition + offset;

		return GetGridSnappedPosition( worldPos, in voxelScale );
	}

	public virtual Vector3 GetBreakWorldPosition( in SceneTraceResult tr, in float voxelScale )
	{
		var offset = tr.Normal * (voxelScale / 2f).Min( 1f );
		var worldPos = tr.EndPosition - offset;

		return GetGridSnappedPosition( worldPos, in voxelScale );
	}

	protected virtual bool TryPlaceVoxel( in SceneTraceResult tr )
	{
		if ( !TryGetGrid( out var v ) )
		{
			RpcHostCreateGrid();
			return false;
		}

		var placePos = GetPlaceWorldPosition( in tr, v.Scale );
		var voxPos = v.WorldToVoxel( placePos );

		// Random color.
		var hue = Random.Float( 0f, 360f );
		var color = new ColorHsv( hue, 0.7f, 0.9f ).ToColor();

		v.BroadcastSet( voxPos, new( color ) );

		return true;
	}

	protected virtual bool TryBreakVoxel( in SceneTraceResult tr )
	{
		if ( !TryGetGrid( out var v ) )
			return false;

		var breakPos = GetBreakWorldPosition( in tr, v.Scale );
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
