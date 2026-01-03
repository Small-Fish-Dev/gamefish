using System;
using System.Runtime.CompilerServices;

namespace Playground;

/// <summary>
/// Data for trying to spawn something with a tool.
/// </summary>
public struct ObjectSpawnConfig : IValid
{
	/// <summary>
	/// Does this have a valid prefab and island/transform configuration?
	/// </summary>
	public readonly bool IsValid => Prefab.IsValid()
		&& ITransform.IsValid( Transform );

	// TODO: Use IDs or resources for security?
	public PrefabFile Prefab { get; set; }

	/// <summary>
	/// Should we parent this to an island?
	/// We'll create one if one can't be found.
	/// </summary>
	public bool InGroup { get; set; }

	/// <summary>
	/// The group to put this object in.
	/// </summary>
	public EditorIsland Island { get; set; }

	public Transform Transform { get; set; }
	public bool IsTransformLocal { get; set; }

	/// <summary>
	/// Should this start with its motion enabled?
	/// <br /> <br />
	/// <b> NOTE: </b> Not yet implemented.
	/// </summary>
	public bool Motion { get; set; } = true;

	public ObjectSpawnConfig() { }

	/// <summary>
	/// For spawning an entity on its own in world space.
	/// </summary>
	public ObjectSpawnConfig( PrefabFile prefab, in Transform tWorld )
	{
		Prefab = prefab;

		Transform = tWorld;
		IsTransformLocal = false;

		InGroup = false;
		Island = null;
	}

	/// <summary>
	/// For spawning an entity attached to other stuff.
	/// </summary>
	public ObjectSpawnConfig( PrefabFile prefab, EditorIsland group, in Transform tLocal )
	{
		Prefab = prefab;

		Transform = tLocal;
		IsTransformLocal = true;

		InGroup = true;
		Island = group;
	}

	public static ObjectSpawnConfig FromWorld( PrefabFile prefab, EditorIsland group, in Transform tWorld )
	{
		if ( group.IsValid() )
		{
			var tLocal = group.WorldTransform.ToLocal( tWorld );
			return new( prefab, group, tLocal );
		}

		return new( prefab, tWorld );
	}

	public ObjectSpawnConfig WithMotion( in bool isEnabled = true )
		=> this with { Motion = isEnabled };

	public ObjectSpawnConfig WithIsland( in bool isEnabled = true, EditorIsland island = null )
		=> this with
		{
			InGroup = isEnabled,
			Island = island
		};

	public readonly bool TryGetWorldTransform( out Transform tWorld )
	{
		if ( !ITransform.IsValid( Transform ) )
			goto Bad;

		if ( !IsTransformLocal )
		{
			tWorld = Transform;
			return true;
		}

		if ( Island.IsValid() )
		{
			tWorld = Island.WorldTransform.ToLocal( Transform );
			return true;
		}

		Bad:

		tWorld = Transform.Zero;
		return false;
	}
}
