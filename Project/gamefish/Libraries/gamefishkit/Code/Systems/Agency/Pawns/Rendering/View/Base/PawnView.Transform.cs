namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// The world position of where the view intends to be.
	/// </summary>
	[Sync( SyncFlags.Interpolate )]
	public Vector3 ViewPosition
	{
		get => _viewPos;
		set
		{
			if ( !IsProxy )
				OnSetViewPosition( ref value );

			_viewPos = value;
		}
	}

	protected Vector3 _viewPos;

	/// <summary>
	/// The world rotation of where the view intends to look.
	/// </summary>
	[Sync( SyncFlags.Interpolate )]
	public Rotation ViewRotation
	{
		get => _viewRotation;
		set
		{
			if ( !IsProxy )
				OnSetViewRotation( ref value );

			_viewRotation = value;
		}
	}

	protected Rotation _viewRotation = Rotation.Identity;

	public Vector3 ViewForward => ViewRotation.Forward;
	public Transform ViewTransform => new( ViewPosition, ViewRotation, WorldScale );

	public virtual Vector3 PawnEyePosition => TargetPawn?.EyePosition ?? ViewPosition;
	public virtual Rotation PawnEyeRotation => TargetPawn?.EyeRotation ?? ViewRotation;
	public virtual Vector3 PawnScale => TargetPawn?.WorldScale ?? Vector3.One;

	public Transform PawnEyeTransform => new( PawnEyePosition, PawnEyeRotation, PawnScale );

	/// <summary>
	/// Distance from this view to the pawn's first-person origin.
	/// </summary>
	public float DistanceFromEye => ViewPosition.Distance( PawnEyePosition );

	/// <summary>
	/// Allows you to override what position is being set(such as clamping distance).
	/// </summary>
	protected virtual void OnSetViewPosition( ref Vector3 pos ) { }

	/// <summary>
	/// Allows you to override what rotation is being set(such as clamping angle).
	/// </summary>
	protected virtual void OnSetViewRotation( ref Rotation r ) { }

	/// <summary>
	/// Allows this view to specify the origin from which it may offset from. <br />
	/// By default this is the <see cref="TargetPawn"/>'s eye transform.
	/// </summary>
	/// <returns> The base transform to offset from. </returns>
	public virtual Transform GetViewOrigin()
		=> PawnEyeTransform;

	/// <summary>
	/// Updates and then returns the view transform.
	/// </summary>
	/// <param name="updateView"> If true: update where the camera should render from. </param>
	/// <param name="updateObject"> If true: update the view's GameObject world transform. </param>
	/// <returns> Where the camera should be positioned. </returns>
	public virtual Transform GetViewTransform( bool updateView = true, bool updateObject = true )
	{
		UpdateViewTransform( updateView, updateObject );

		return ViewTransform;
	}

	/// <summary>
	/// Applies current offset/transition effects to the view's world position/rotation. <br />
	/// Also allows (not) setting the world transform of the viewing object itself.
	/// </summary>
	/// <param name="updateView"> If true: update where the camera should render from. </param>
	/// <param name="updateObject"> If true: update the view's GameObject world transform. </param>
	public virtual void UpdateViewTransform( bool updateView = true, bool updateObject = true )
	{
		if ( updateView )
			UpdateModeTransform();

		if ( updateObject )
			WorldTransform = ViewTransform;
	}

	/// <summary>
	/// Sets this view's transform according to the current mode.
	/// </summary>
	protected virtual void UpdateModeTransform()
	{
		Mode?.SetViewTransform();
	}
}
