using Boxfish;
using Boxfish.Library;

namespace Playground;

public partial class VoxelTool : ShapeTool
{
	[Property]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile VoxelPrefab { get; set; }

	public Vector3Int TargetVoxel { get; protected set; }
	public NetworkedVoxelVolume TargetVolume { get; protected set; }

	public Vector3Int OriginVoxel { get; protected set; }
	public NetworkedVoxelVolume OriginVolume { get; protected set; }

	public override bool ShowPointerTransform => false;

	public override int PointLimit => 2;

	protected override void ClearTarget()
	{
		base.ClearTarget();

		TargetVolume = null;
		TargetVoxel = default;
	}

	protected override void ClearOrigin()
	{
		base.ClearOrigin();

		OriginVolume = null;
		TargetVoxel = default;
	}

	protected override void OnSecondary( in SceneTraceResult tr )
	{
		if ( TryBreakVoxel( in tr ) )
		{
			Clear();
			return;
		}

		base.OnSecondary( tr );
	}

	protected VoxelBounds GetVoxelBounds()
	{
		if ( Points is null )
			return default;

		var iMins = OriginVoxel.ComponentMin( TargetVoxel );
		var iMaxs = TargetVoxel.ComponentMax( OriginVoxel );

		return new( iMins, iMaxs + 1 );
	}

	protected override void RenderBoxShape()
	{
		if ( !HasPoints || !HasOrigin )
			return;

		RenderVoxelBox( OriginVolume, GetVoxelBounds() );
	}

	protected override void RenderPointer()
	{
		base.RenderPointer();

		if ( !TargetVolume.IsValid() )
			return;

		RenderVoxelBox( TargetVolume, new( TargetVoxel, TargetVoxel + 1 ) );
	}

	protected override void SetTarget( GameObject obj = null, Component target = null, in SceneTraceResult tr = default )
	{
		base.SetTarget( obj, target, tr );

		TargetVolume = target as NetworkedVoxelVolume;

		if ( !TryGetPointer( in tr, out var tPointer ) )
		{
			ClearTarget();
			return;
		}

		TargetVoxel = TargetVolume.WorldToVoxel( tPointer.Position );
	}

	public override bool TryGetTargetComponent( in SceneTraceResult tr, out Component target )
	{
		target = null;

		const FindMode findMode = FindMode.EnabledInSelf | FindMode.InAncestors;

		if ( tr.GameObject.IsValid() && tr.GameObject.Components.TryGet<NetworkedVoxelVolume>( out var v, findMode ) )
		{
			target = v;
			return true;
		}

		if ( !tr.GameObject.IsValid() )
		{
			if ( TryGetVolume( out var vGlobal ) )
			{
				target = vGlobal;
				return true;
			}

			return false;
		}

		return false;
	}

	protected override bool TryGetPointer( in SceneTraceResult tr, out Transform tPointer )
	{
		if ( !base.TryGetPointer( tr, out tPointer ) )
			return false;

		var v = TargetVolume.AsValid() ?? OriginVolume;

		if ( !v.IsValid() )
			return false;

		var point = tPointer.Position;
		point += v.Scale * 0.5f;

		if ( tr.Hit )
			point -= tr.Normal * v.Scale * 0.5f;

		tPointer.Position = v.VoxelToWorld( v.WorldToVoxel( point ) );
		tPointer.Rotation = v.WorldRotation;

		return true;
	}

	protected void RenderVoxelBox( NetworkedVoxelVolume v, in VoxelBounds iBounds )
	{
		if ( !v.IsValid() )
			return;

		var tVox = v.WorldTransform;

		var fMins = iBounds.Mins * v.Scale;
		var fMaxs = iBounds.Maxs * v.Scale;

		var bounds = BBox.FromPoints( [fMins, fMaxs] )
			.Translate( v.Scale * -0.5f )
			.Grow( -0.01f );

		this.DrawBox( bounds, ColorOutline, ColorFilled, tWorld: tVox );
	}

	protected override void SetShapeOrigin( in SceneTraceResult tr, Transform tPointer )
	{
		if ( !HasTarget || !TargetVolume.IsValid() )
			return;

		OriginVoxel = TargetVoxel;
		OriginVolume = TargetVolume;

		var tVolume = TargetVolume.WorldTransform;

		var fVoxel = TargetVolume.VoxelToWorld( OriginVoxel );
		var tLocal = tVolume.ToLocal( new( fVoxel, tVolume.Rotation ) );

		SetOrigin( tLocal, TargetVolume.GameObject, TargetVolume );
	}

	protected override bool TryAddLocalPoint( Transform tLocal )
	{
		if ( !OriginVolume.IsValid() )
			return false;

		var tVolume = OriginVolume.WorldTransform;
		var tPoint = tVolume.PointToWorld( tLocal.Position );

		tLocal.Position = OriginVolume.WorldToVoxel( tPoint );

		return base.TryAddLocalPoint( tLocal );
	}

	protected override void OnPointAdded( in Vector3 pos, in Rotation r )
	{
		if ( !AtLimit )
			return;

		TryPlaceVoxels();
		Clear();
	}

	protected bool TryPlaceVoxels()
	{
		if ( !OriginVolume.IsValid() )
			return false;

		if ( !HasPoints )
			return false;

		// Random color.
		var hue = Random.Float( 0f, 360f );
		var color = new ColorHsv( hue, 0.7f, 0.9f ).ToColor();

		OriginVolume.BroadcastSetBounds( default, GetVoxelBounds(), new( color ) );

		return true;
	}

	protected virtual bool TryBreakVoxel( in SceneTraceResult tr )
	{
		if ( HasOrigin && OriginVolume.IsValid() )
		{
			OriginVolume.BroadcastSetBounds( default, GetVoxelBounds(), Voxel.Empty );
			return true;
		}

		if ( HasTarget && TargetVolume.IsValid() )
		{
			this.Log( TargetVoxel );
			TargetVolume.BroadcastSet( TargetVoxel, Voxel.Empty );
			return true;
		}

		return false;
	}

	protected override bool TryCreateShape( out GameObject obj )
		=> (obj = null).IsValid();

	protected virtual bool TryGetVolume( out NetworkedVoxelVolume v )
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

		if ( TryGetVolume( out _ ) )
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
