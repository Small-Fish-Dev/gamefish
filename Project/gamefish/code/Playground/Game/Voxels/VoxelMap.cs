using System.Threading.Tasks;
using Boxfish.Library;

namespace Fishbox;

public partial class VoxelMap : NetworkedVoxelVolume, Component.ExecuteInEditor
{
	[Property]
	[Group( MAP )]
	[Sync( SyncFlags.FromHost )]
	[FilePath( Extension = "vox,vxl" )]
	[Change( Name = nameof( ReloadMap ) )]
	public string MapPath { get; set; }

	public override bool StoreEditorChunks => true;

	/*
	protected override async Task OnLoad()
	{
		await base.OnLoad();

		await LoadMap( MapPath );
	}
	*/

	protected override void OnStart()
	{
		base.OnStart();

		Task.RunInThreadAsync( ReloadMap );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !this.InEditor() )
			return;

		if ( Chunks is null || !Chunks.Any() )
			return;

		using ( Gizmo.Scope( MapPath, global::Transform.Zero ) )
		{
			// Render chunk models in editor only!
			foreach ( var (position, model) in EditorChunks )
			{
				var transform = new Transform( position );
				var obj = Gizmo.Draw.Model( model, transform );
				SetAttributes( obj.Attributes );
			}
		}
	}

	protected async Task LoadMap( string path = null )
	{
		if ( path.IsBlank() )
			return;

		await Task.MainThread();

		// Load the map.
		await Import( path );

		// Generate visuals and collision.
		var chunks = Chunks?.Values ?? [];

		await GenerateMeshes( chunks );
	}

	[Button]
	[Group( MAP )]
	public async Task ReloadMap()
		=> await LoadMap( MapPath );

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( MapPath.IsBlank() )
			return;

		if ( Chunks?.Any() is not true )
			Task.RunInThreadAsync( ReloadMap );
	}
}
