using Sandbox.Navigation;

namespace GameFish;

/// <summary>
/// Provides navigation and movement for the <see cref="Actor"/>.
/// </summary>
[Icon( "directions_run" )]
public partial class ActorNavigation : ActorFeature
{
	protected const int NAVIGATION_ORDER = DEFAULT_ORDER - 1492;

	[Property]
	[Feature( NAVIGATION )]
	public NavMeshAgent NavAgent { get; set; }

	[Property]
	[Feature( NAVIGATION ), Group( MOVEMENT )]
	public new BaseController Controller => base.Controller;

	[Sync]
	public Vector3? Destination { get; set; }

	public List<Vector3> CurrentPath { get; set; }

	public virtual void Stop()
	{
		Destination = null;
		CurrentPath = null;

		WishVelocity = default;

		NavAgent?.Stop();
	}

	/// <summary>
	/// Try to path somewhere.
	/// </summary>
	/// <returns> If it's somewhere we'll try to go. </returns>
	public virtual bool TryMoveTo( Vector3? dest = null )
	{
		if ( !dest.HasValue )
			return false;

		if ( !NavAgent.IsValid() || !Controller.IsValid() )
			return false;

		Destination = dest.Value;
		NavAgent.MoveTo( dest.Value );

		return true;
	}

	public virtual bool TryUpdatePath( in Vector3 to )
	{
		if ( !Scene.IsValid() || Scene.NavMesh is null || !Scene.NavMesh.IsEnabled )
		{
			// Log.Warning( $"NavMesh is (null:{Scene.NavMesh is null}, enabled:{Scene.NavMesh.IsEnabled})" );
			return false;
		}

		var groundPos = WorldPosition;

		bool AlmostEqual( in Vector3 to ) => to.AlmostEqual( groundPos, 5f );

		// If we are basically there then don't bother.
		if ( AlmostEqual( to ) )
			return false;

		var path = Scene.NavMesh.CalculatePath( new CalculatePathRequest() { Start = groundPos, Target = to } );
		var firstPoint = path.Points?.ElementAtOrDefault( 1 ).Position;

		Destination = firstPoint;

		if ( firstPoint is null )
		{
			Destination = default;
			return false;
		}

		Destination = firstPoint.Value;
		return true;
	}

	public virtual Vector3? GetClosestPoint( in Vector3 to )
		=> Scene?.NavMesh?.GetClosestPoint( to );
}
