namespace GameFish;

public abstract partial class ActorModel : Component
{
	public const string GROUP_MODEL = "🕺 Model";

	public virtual Model Model { get => GetModel(); set => SetModel( value ); }
	private Model _model;

	public abstract Model GetModel();
	public abstract void SetModel( Model mdl );

	// public virtual void Set<T>( string key, T value ) { }
}
