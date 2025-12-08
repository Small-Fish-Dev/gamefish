using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace GameFish;

partial class DataFile<TDataComp, TDataClass>
{
	/// <summary>
	/// If true: load the file whenever this component is loaded.
	/// <br />
	/// The component may use/apply its data on load,
	/// in which case you'd want to enable this.
	/// <br />
	/// Whenever you get or set something on an unloaded
	/// file it will try to load it beforehand.
	/// </summary>
	[Property, Group( FILE )]
	[Feature( SAVING ), Order( ORDER )]
	public virtual bool AutoLoading { get; set; } = true;

	/// <summary>
	/// Automatically save upon a change after <see cref="AutoSaveDelay"/>?
	/// </summary>
	[Property, Group( FILE )]
	[Feature( SAVING ), Order( ORDER )]
	public bool AutoSaving { get; set; } = true;

	/// <summary>
	/// The delay before automatically saving if there are unsaved changes.
	/// </summary>
	[Property, Group( FILE )]
	[Feature( SAVING ), Order( ORDER )]
	[Range( 5f, 60f, clamped: false ), Step( 1 )]
	public float AutoSaveDelay { get; set; } = 30f;

	/// <summary>
	/// The period after failing to load/save before trying again.
	/// </summary>
	[Property, Group( FILE )]
	[Title( "Failure Cooldown" )]
	[Feature( SAVING ), Order( ORDER )]
	[Range( 2f, 10f, clamped: false ), Step( 1 )]
	public float FileFailureCooldown { get; set; } = 5f;

	[JsonIgnore, ReadOnly]
	[Feature( SAVING ), Order( ORDER )]
	[Property, Group( FILE ), Title( "Next Auto-Save" )]
	public double? AutoSaveTimer => IsDirty && AutoSaving ? NextAutoSave.Relative : null;
	public RealTimeUntil NextAutoSave { get; protected set; }

	[JsonIgnore, ReadOnly]
	[Property, Title( "Folder" )]
	[Feature( SAVING ), Group( FILE ), Order( ORDER - 1 )]
	protected string InspectorFileFolder => FileFolder;
	public virtual string FileFolder => Library.GameIdent;

	/// <summary>
	/// The name/extension of the file itself to look for when loading and saving.
	/// </summary>
	[JsonIgnore]
	[Property, ReadOnly]
	[Title( "File Name" )]
	[Feature( SAVING ), Group( FILE ), Order( ORDER - 1 )]
	protected string InspectorFileName => FileName;

	/// <summary>
	/// The name/extension of the file itself to look for when loading and saving.
	/// </summary>
	public abstract string FileName { get; }

	[JsonIgnore, ReadOnly]
	[Feature( SAVING ), Order( ORDER + 10 )]
	[Property, Group( DATA ), Title( "Data" )]
	public Dictionary<string, JsonNode> Entries => DataObject?.ToDictionary();

	/// <summary>
	/// The Json-encoded data to get and set properties on.
	/// </summary>
	protected JsonObject DataObject { get; set; }
	public bool IsLoaded => DataObject is not null;

	/// <summary>
	/// If true: the file data has been modified in some way since saving.
	/// </summary>
	[Title( "Is Dirty" )]
	[Property, JsonIgnore, ReadOnly]
	[Feature( SAVING ), Group( FILE ), Order( ORDER )]
	protected bool InspectorIsDirty => IsDirty;

	/// <summary>
	/// If true: the file data has been modified in some way since saving.
	/// </summary>
	public bool IsDirty { get; protected set; }

	[Feature( SAVING ), Order( ORDER + 1 )]
	[Button, Group( FILE ), Title( "Load" )]
	protected virtual void TryLoadButton() => InternalTryLoad();

	[Feature( SAVING ), Order( ORDER + 1 )]
	[Button, Group( FILE ), Title( "Save" )]
	protected virtual void TrySaveButton() => InternalTrySave();

	[Feature( SAVING ), Order( ORDER + 1 )]
	[Button, Group( FILE ), Title( "Clear" )]
	protected virtual void ClearButton() => Clear();

	public virtual void Clear()
		=> DataObject?.Clear();

	/// <summary>
	/// If defined: tells you when we last failed to read/write the file.
	/// </summary>
	public TimeSince? TimeSinceFailure { get; protected set; }

	/// <summary>
	/// Called when a file's data could not be read/found and so a
	/// fresh instance of <typeparamref name="TDataClass"/> was created.
	/// </summary>
	protected virtual void OnDataInitialized()
	{
	}

	/// <summary>
	/// Called when a file's data was successfully loaded and/or initialized.
	/// </summary>
	protected virtual void OnDataLoaded()
	{
	}

	/// <summary>
	/// Called when a file's data was saved.
	/// </summary>
	protected virtual void OnDataSaved()
	{
	}

	/// <summary>
	/// Saves if any data was modified.
	/// </summary>
	public virtual bool AutoSave( in bool force = false )
	{
		if ( !IsDirty && !force )
			return false;

		if ( !TrySave() )
			return false;

		this.Log( "Auto-save successful." );

		return true;
	}

	/// <summary>
	/// Sanitizes then combines the folder and file name into a path.
	/// </summary>
	/// <returns> If the resulting path is valid. </returns>
	public virtual bool TryGetFilePath( out string folder, out string fileName, out string path )
	{
		try
		{
			var folderInvalid = Path.GetInvalidPathChars();
			folder = (FileFolder ?? string.Empty).Split( folderInvalid ).Implode();

			var fileInvalid = Path.GetInvalidFileNameChars();
			fileName = (FileName ?? string.Empty).Split( fileInvalid ).Implode();
			fileName = Path.GetFileName( fileName ?? "" );

			if ( folder.IsBlank() || fileName.IsBlank() )
			{
				path = string.Empty;
				return false;
			}

			path = Path.Combine( folder, fileName );

			return !path.IsBlank();
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( TryGetFilePath )} EXCEPTION: {e}" );

			folder = fileName = path = null;

			return false;
		}
	}

	public virtual void OnLoadFailed()
	{
		TimeSinceFailure = 0;
	}

	public virtual void OnSaveFailed()
	{
		TimeSinceFailure = 0;
	}

	public bool TryLoad()
	{
		if ( TimeSinceFailure.HasValue && TimeSinceFailure.Value < FileFailureCooldown )
			return false;

		if ( InternalTryLoad() )
			return true;

		OnLoadFailed();

		return false;
	}

	public bool TrySave()
	{
		if ( InternalTrySave() )
			return true;

		OnSaveFailed();

		return false;
	}

	protected virtual bool InternalTryLoad()
	{
		DebugLog( $"Loading.." );

		if ( !TryGetFilePath( out var folder, out var fileName, out var path ) )
		{
			this.Warn( $"Invalid path! folder:[{folder}] fileName:[{fileName}] path:[{path}]" );
			return false;
		}

		try
		{
			FileSystem.OrganizationData.CreateDirectory( folder );
			var fileContents = FileSystem.OrganizationData.ReadAllText( path );

			// Initialize with new data if the file was missing/blank.
			if ( fileContents.IsBlank() )
			{
				DebugLog( $"File was missing/blank. Attempting to initialize." );

				DataObject = JsonSerializer.SerializeToNode( CreateData() ) as JsonObject;

				if ( DataObject is null )
				{
					this.Warn( $"Failed to initialize data. Result: null." );
					return false;
				}

				this.Log( $"Initialized data." );

				try
				{
					OnDataInitialized();
					OnDataLoaded();
				}
				catch ( Exception e )
				{
					this.Warn( $"{nameof( OnDataInitialized )}/{nameof( OnDataLoaded )} exception: {e}" );
				}

				return true;
			}

			// The target file's contents were loaded. Try to parse it.
			var data = CreateData();
			var objParsed = Json.ParseToJsonObject( fileContents );

			if ( objParsed is not null )
			{
				var propertyTable = TypeLibrary.GetPropertyDescriptions( data, onlyOwn: false );

				foreach ( var (name, node) in objParsed )
				{
					var p = propertyTable.FirstOrDefault( p => p?.Name == name );

					if ( p is null )
					{
						DebugLog( $"File had unhandled member \"{name}\"." );
						continue;
					}

					if ( Json.TryDeserialize( node?.ToJsonString(), p.PropertyType, out var val ) )
					{
						if ( !TypeLibrary.SetProperty( data, p.Name, val ) )
							this.Warn( $"Failed to set {name} to \"{val}\"" );
					}
					else
					{
						this.Warn( $"Failed to deserialize {name} as {p.PropertyType}" );
					}
				}
			}

			// Create a Json node so we can do safe dynamically typed member access at runtime.
			// Maybe there's a better way but the performance impact is typically negligible.
			if ( JsonSerializer.SerializeToNode( data ) is not JsonObject obj )
			{
				this.Warn( $"Failed to serialize {typeof( TDataClass )} into a {nameof( JsonObject )}." );
				return false;
			}

			// We parsed what data we could. Now store it on the component.
			DataObject?.Clear();
			DataObject = obj;

			DebugLog( $"Successfully loaded." );

			try
			{
				OnDataLoaded();
			}
			catch ( Exception e )
			{
				this.Warn( $"{nameof( OnDataLoaded )} exception: {e}" );
			}

			return true;
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( TryLoad )} EXCEPTION: {e}" );
			return false;
		}
	}

	protected virtual bool InternalTrySave()
	{
		DebugLog( $"Saving.." );

		IsDirty = false;

		if ( !TryGetFilePath( out var folder, out var fileName, out var path ) )
		{
			this.Warn( $"Invalid path! folder:[{folder}] fileName:[{fileName}] path:[{path}]" );
			return false;
		}

		try
		{
			if ( DataObject is null )
			{
				DebugLog( $"Wasn't loaded. Nothing to save." );
				return true;
			}

			var fileContents = DataObject.ToJsonString();

			if ( fileContents.IsBlank() )
			{
				DebugWarn( $"Null/blank JSON string." );
				return false;
			}

			FileSystem.OrganizationData.CreateDirectory( folder );
			FileSystem.OrganizationData.WriteAllText( path, fileContents );

			DebugLog( $"Successfully saved." );

			try
			{
				OnDataSaved();
			}
			catch ( Exception e )
			{
				this.Warn( $"{nameof( OnDataSaved )} exception: {e}" );
			}

			return true;
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( TrySave )} EXCEPTION: {e}" );
			return false;
		}
	}
}
