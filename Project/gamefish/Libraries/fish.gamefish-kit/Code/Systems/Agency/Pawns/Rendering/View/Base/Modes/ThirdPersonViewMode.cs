namespace GameFish;

public partial class ThirdPersonViewMode : ViewMode
{
	[Property]
	[Feature( VIEW )]
	public virtual float? InitialDistance { get; set; } = 100f;

	[Property]
	[Feature( VIEW )]
	public virtual FloatRange DistanceRange { get; set; } = new( 50f, 300f );

	/// <summary>
	/// Sensitivity of the mouse wheel when used to change the distance. <br />
	/// Can be negative to invert the mouse wheel direction, or zero
	/// to disable the mouse wheel altogether.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	public virtual float ScrollSensitivity { get; set; } = 15f;

	/// <summary>
	/// How quickly scrolling is smoothed towards its intended distance. <br />
	/// If <c>zero</c> or less: disable smoothing altogether.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	public virtual float ScrollSpeed { get; set; } = 5f;

	public virtual float CurrentDistance { get; set; }
	public virtual float DesiredDistance { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		SetInitialDistance();
	}

	public override void OnModeEnter( ViewMode previousMode = null )
	{
		base.OnModeEnter( previousMode );

		SetInitialDistance();
	}

	protected virtual void SetInitialDistance()
	{
		if ( !InitialDistance.HasValue )
			return;

		DesiredDistance = InitialDistance.Value;
		CurrentDistance = DesiredDistance;
	}

	public override void OnModeUpdate( in float deltaTime )
	{
		if ( !View.IsValid() )
			return;

		var pawn = TargetPawn;

		if ( !pawn.IsValid() )
			return;

		if ( !Mouse.Active )
		{
			var scroll = Input.MouseWheel.y * ScrollSensitivity;
			DesiredDistance = (DesiredDistance - scroll).Clamp( DistanceRange );

			CurrentDistance = ScrollSpeed > 0f
				? CurrentDistance.LerpTo( DesiredDistance, ScrollSpeed * deltaTime )
				: DesiredDistance;
		}

		if ( View.Collision )
		{
			var tOrigin = View.GetViewOrigin();
			var aimDir = tOrigin.Forward;

			var startPos = tOrigin.Position;
			var endPos = startPos - (aimDir * CurrentDistance);
			var radius = View.GetCollisionRadius();

			var trAll = Scene.Trace.Sphere( radius, startPos, endPos )
				.IgnoreGameObject( pawn.GameObject )
				.WithAnyTags( View.CollisionHitTags )
				.WithoutTags( View.CollisionIgnoreTags )
				.RunAll();

			foreach ( var tr in trAll )
			{
				if ( !tr.Hit || (!View.CollideOwned && tr.GameObject.IsOwner()) )
					continue;

				CurrentDistance = CurrentDistance.Min( tr.Distance );
			}

			/*
			DebugOverlay.ScreenText( Vector2.One * 20,
				$"{DesiredDistance} {trace.Distance} {trace.Hit} {trace.GameObject?.Name}",
				flags: TextFlag.LeftTop );

			DebugOverlay.Line( trace.StartPosition, trace.EndPosition, Color.Red, overlay: true );
			*/
		}

		var tPos = Vector3.Backward * CurrentDistance;

		Relative = new( tPos, Rotation.Identity );
	}
}
