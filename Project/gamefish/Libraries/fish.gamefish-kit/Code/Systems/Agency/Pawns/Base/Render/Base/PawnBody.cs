using System.Text.Json.Serialization;

namespace GameFish;

public abstract partial class PawnBody : Module
{
	protected const int MODEL_ORDER = MODULE_ORDER - 1000;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Pawn;

	[Title( "Opacity" )]
	[Property, JsonIgnore]
	[Range( 0f, 1f ), Step( 0.001f )]
	[Feature( MODEL ), Group( EFFECTS ), Order( MODEL_ORDER )]
	protected float InspectorOpacity
	{
		get => Opacity;
		set => Opacity = value;
	}

	/// <summary>
	/// The speed that this fades back in on its own.
	/// </summary>
	[Property]
	[Range( 0.1f, 10f, clamped: false )]
	[Feature( MODEL ), Group( EFFECTS ), Order( MODEL_ORDER )]
	public float OpacitySpeed { get; set; } = 2f;

	public virtual float Opacity
	{
		get => _opacity;
		set
		{
			_opacity = value.Clamp( 0f, 1f );
			OnSetOpacity( _opacity );
		}
	}

	protected float _opacity = 1f;

	public Model Model { get => GetModel(); set => SetModel( value ); }

	protected override void OnUpdate()
	{
		if ( !InGame )
			return;

		UpdateOpacity();
	}

	protected virtual Model GetModel() => null;
	protected virtual void SetModel( Model mdl ) { }

	protected virtual void OnSetOpacity( in float a )
	{
	}

	protected virtual void UpdateOpacity()
	{
		if ( Opacity >= 1f )
			return;

		Opacity += Time.Delta * OpacitySpeed;
	}

	/// <summary>
	/// Called from a pawn to manage things like distance fading.
	/// </summary>
	public virtual void OnViewUpdate( PawnView view )
	{
		if ( view.IsValid() )
			Opacity = OpacityFromDistance( view.DistanceFromEye );
	}

	// Hardcoded for consistency but you can easily override this.
	public virtual float OpacityFromDistance( in float distance )
		=> (distance * WorldScale.x.NonZero( 0.1f )).Remap( 15f, 25f );

	/// <summary>
	/// Set an animation parameter on the model rendering component.
	/// </summary>
	public virtual void SetAnim<T>( in string key, in T value )
	{
	}
}
