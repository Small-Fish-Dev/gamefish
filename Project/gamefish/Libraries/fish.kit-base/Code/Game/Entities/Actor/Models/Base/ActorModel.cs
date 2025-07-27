namespace GameFish;

public abstract partial class ActorModel : Component
{
	public const string MODEL = "🕺 Model";
	public const string DEBUG = BaseEntity.DEBUG;

	public const string GROUP_OPACITY = "Opacity";

	/// <summary>
	/// The speed that this model back fades in on its own.
	/// </summary>
	[Property]
	[Feature( MODEL ), Group( GROUP_OPACITY )]
	public float OpacitySpeed { get; set; } = 2f;

	[Property]
	[Range( 0f, 1f, step: 0.001f )]
	[Feature( DEBUG ), Group( GROUP_OPACITY )]
	public virtual float Opacity
	{
		get => _opacity;
		set
		{
			_opacity = value.Clamp( 0f, 1f );
			SetModelOpacity( _opacity );
		}
	}

	protected float _opacity;

	public TimeUntil OpacityReset { get; set; }

	public virtual Model Model { get => GetModel(); set => SetModel( value ); }

	protected override void OnUpdate()
	{
		UpdateOpacity();
	}

	public abstract Model GetModel();
	public abstract void SetModel( Model mdl );

	protected abstract void SetModelOpacity( in float opacity );

	public virtual void SetOpacity( in float a, in float resetDelay = 0.1f )
	{
		Opacity = a;
		OpacityReset = resetDelay;
	}

	protected virtual void UpdateOpacity()
	{
		if ( Opacity >= 1f )
			return;

		// if ( !OpacityReset )
		// return;

		Opacity += Time.Delta * OpacitySpeed;
	}

	// public virtual void Set<T>( string key, T value ) { }
}
