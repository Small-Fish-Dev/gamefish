using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// An <see cref="Entity"/> that supports health and physics.
/// <br /> <br />
/// <b> NOTE: </b> Looks for a <see cref="Sandbox.Rigidbody"/> by default.
/// </summary>
[Icon( "sports_volleyball" )]
public abstract partial class DynamicEntity : ModuleEntity
{
	protected const int PHYSICS_ORDER = ENTITY_ORDER + 50;
}
