namespace GameFish;

/// <summary>
/// A <see cref="DataFile{TDataComp, TDataClass}"/> meant for saves.
/// Uses your Steam ID and a save ID for the filename.
/// </summary>
[Icon( "save" )]
public abstract partial class SaveData<TSaveData> : DataFile<SaveData<TSaveData>, TSaveData>
	where TSaveData : DataFile<SaveData<TSaveData>, TSaveData>.DataClass // 🤓
{
	protected const int SAVE_ORDER = DEFAULT_ORDER - 777;

	/// <summary> The default/fallback identifying part of the save's name. </summary>
	public const string ID_DEFAULT = "default";

	// Save files should never auto-load since we might be a non-host in multiplayer.
	public override bool AutoLoading => false;

	// Incorporate the SteamId and save name in the filename.
	public override string FileName => $"{Game.SteamId}_{SaveID ?? ID_DEFAULT}.json";

	/// <summary>
	/// The identifying string of this save file.
	/// </summary>
	public virtual string SaveID { get; protected set; }

	/// <summary>
	/// Does this save file belong to the local player?
	/// </summary>
	public bool HasSave => !SaveID.IsBlank();

	protected override bool InternalTryLoad()
	{
		if ( !HasSave )
		{
			this.Warn( $"Invalid {nameof( SaveID )}:[{SaveID}] when loading." );
			return false;
		}

		return base.InternalTryLoad();
	}

	protected override bool InternalTrySave()
	{
		if ( !HasSave )
		{
			this.Warn( $"Invalid {nameof( SaveID )}:[{SaveID}] when saving." );
			return false;
		}

		return base.InternalTrySave();
	}

	/// <summary>
	/// Save any changes, clear all data and try to load/create a new save.
	/// </summary>
	/// <param name="id"> The identifying part of the file name. </param>
	/// <param name="isNew"> Should the file be cleared and saved? </param>
	/// <returns> If <paramref name="id"/> was valid and the file could be written. </returns>
	public virtual bool TrySetID( string id, bool isNew = false )
	{
		if ( id.IsBlank() )
		{
			this.Warn( $"Tried to create save with invalid name:[{id}]" );
			return false;
		}

		id ??= ID_DEFAULT;

		// Make sure the previous data(if any) is saved.
		AutoSave();

		// Clean the slate for the new file.
		Clear();

		// Set the identifying part of the save file's path.
		SaveID = id;

		// If creating a new file then save after clearing the data.
		if ( isNew )
		{
			if ( TrySave() )
				return true;

			this.Warn( $"Failed to create new save: \"{id}\"" );
			return false;
		}

		// Otherwise load what may exist of the save.
		return TryLoad();
	}
}
