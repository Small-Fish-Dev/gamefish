namespace GameFish;

/// <summary>
/// ⚙ 🛠 📋 <br />
/// A component for managing a specific file. <br />
/// Can read/write any type of JSON-serializable data.
/// </summary>
[Icon( "description" )]
public abstract partial class DataFile<TDataComp, TDataClass> : Singleton<TDataComp> where TDataComp : DataFile<TDataComp, TDataClass>
{
	protected const string DATA = "🔍 Data";
	protected const int ORDER = DEFAULT_ORDER - 9999;

	protected override bool? IsNetworkedOverride => true;
	protected override NetworkMode NetworkingModeDefault => NetworkMode.Snapshot;

	/// <returns> This data file's type and filepath. </returns>
	public override string ToString()
		=> $"{this?.GetType().ToSimpleString()}|\"{this?.FileName}\"";

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( AutoLoading && !IsLoaded )
			TryLoad();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsDirty && NextAutoSave )
			AutoSave();
	}

	public static bool TrySet<T>( string name, T value, in bool dirty = true, in bool autoLoad = true )
		=> Instance?.Set( name, value, dirty, autoLoad ) is true;

	public static bool TryGet<T>( string name, out T value, T defaultValue = default, in bool autoLoad = true )
		=> Instance?.Get( name, out value, defaultValue, autoLoad ) ?? (value = default) is true;

	protected bool HasKey( string key )
	{
		return DataObject is not null && DataObject.ContainsKey( key );
	}

	public virtual bool Set<T>( string name, in T value, in bool dirty = true, in bool autoLoad = true )
	{
		// If we don't even have empty data then it wasn't loaded.
		if ( DataObject is null )
			if ( !autoLoad || !TryLoad() )
				return false;

		// Double-check that it was loaded properly.
		if ( DataObject is null )
			return false;

		try
		{
			DataObject[name] = Json.ToNode( value );

			if ( dirty && !IsDirty )
			{
				IsDirty = true;

				if ( AutoSaving )
					NextAutoSave = AutoSaveDelay;
			}

			if ( DebugLogging )
				this.Log( $"Set \"{name}\" to \"{value}\"" );

			return true;
		}
		catch
		{
			this.Warn( $"Exception when setting \"{name ?? "null"}\" to \"{value}\"" );
		}

		return false;
	}

	public virtual bool Get<T>( string name, out T value, in T defaultValue = default, in bool autoLoad = true )
	{
		// If we don't even have empty data then it wasn't loaded.
		if ( DataObject is null )
			if ( !autoLoad || !TryLoad() )
				goto Default;

		// Double-check that it was loaded properly.
		if ( DataObject is null )
			goto Default;

		try
		{
			value = DataObject.GetPropertyValue<T>( name, defaultValue );
			return true;
		}
		catch
		{
			this.Warn( $"Exception when getting \"{name}\" as {typeof( T )}" );
		}

		Default:

		value = defaultValue;
		return false;
	}
}
