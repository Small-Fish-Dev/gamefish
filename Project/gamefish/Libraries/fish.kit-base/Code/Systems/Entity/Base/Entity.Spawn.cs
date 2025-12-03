namespace GameFish;

partial class Entity
{
	public static PrefabFile GetEntityPrefab( string classId )
		=> TryGetEntityPrefab( classId, out var prefabFile ) ? prefabFile : null;

	public static bool TryGetEntityPrefab( string classId, out PrefabFile prefabFile )
	{
		prefabFile = null;

		if ( string.IsNullOrWhiteSpace( classId ) || PrefabLibrary.All is null )
			return false;

		foreach ( var def in PrefabLibrary.FindByComponent<Entity>() )
		{
			if ( !def.Prefab.IsValid() )
				continue;

			var comps = def.PrefabScene?.Components;

			if ( comps is null || !comps.TryGet<Entity>( out var ent, FindMode.EverythingInSelf ) )
				continue;

			if ( ent.IsClass && ent.ClassId == classId )
				return (prefabFile = def.Prefab).IsValid();
		}

		return false;
	}

	/// <summary>
	/// Lets you spawn entities where you're looking by their class ID(if they have one).
	/// </summary>
	[Rpc.Host]
	[ConCmd( "ent_spawn" )]
	public static void SpawnEntityCommand( string classId, float distance = 1000f )
	{
		if ( !Server.CheatsEnabled && !Server.CanCheat( Rpc.Caller ) )
			return;

		if ( !TryGetEntityPrefab( classId, out var prefabFile ) )
		{
			Print.WarnFrom( typeof( Entity ), $"Couldn't find Entity with ID:\"{classId}\"." );
			return;
		}

		GameObject go;

		// Try to spawn it under our current pawn's aim.
		if ( Server.FindPawn( Rpc.Caller ) is var pawn && pawn.IsValid() )
		{
			var tr = pawn.GetEyeTrace( distance ).Run();
			var pos = tr.Hit ? tr.HitPosition : tr.StartPosition + (tr.Direction * tr.Distance);

			prefabFile.TrySpawn( pos, pawn.EyeRotation, out go );
		}
		else
		{
			prefabFile.TrySpawn( out go );
		}

		if ( go.IsValid() && go.Components.TryGet<Entity>( out var ent ) )
			ent.TrySetNetworkOwner( Connection.Host );
	}
}
