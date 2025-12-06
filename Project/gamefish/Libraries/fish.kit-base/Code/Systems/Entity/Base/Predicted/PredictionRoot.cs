using System.Threading.Tasks;

namespace GameFish;

/// <summary>
/// Manages client-side prediction for this object.
/// </summary>
public partial class PredictionRoot : Entity
{
	// Prediction root/manager is never networked.
	protected override bool? IsNetworkedOverride => false;

	[Property, Feature( NETWORKING )]
	public PredictedEntity EntitySource { get; set; }

	[ReadOnly]
	[Property, Feature( NETWORKING )]
	public PredictedEntity EntityProxy { get; set; }
}
