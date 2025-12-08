using System.Text.Json.Serialization;

namespace GameFish;

partial class ActorMind
{
	[Property, JsonIgnore]
	[Title( "Mental State" )]
	[Feature( MIND ), Group( DEBUG )]
	protected MentalState InspectorMentalState
	{
		get => State;
		set
		{
			if ( this.InGame() )
				State = value;
		}
	}

	[Property, JsonIgnore]
	[Feature( MIND ), Group( DEBUG )]
	protected Pawn InspectorTarget
	{
		get => Target;
		set
		{
			if ( this.InGame() )
				Target = value;
		}
	}
}

