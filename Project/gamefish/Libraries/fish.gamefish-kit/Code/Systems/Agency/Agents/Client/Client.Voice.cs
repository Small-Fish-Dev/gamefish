using System.Text.Json.Serialization;

namespace GameFish;

partial class Client
{
	/// <summary>
	/// Are they muted on your end?
	/// </summary>
	[Property]
	[JsonIgnore]
	[Title( "Is Muted" )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( CLIENT ), Group( VOICE )]
	protected bool InspectorIsMuted
	{
		get => IsMuted;
		set => IsMuted = value;
	}

	/*
	[Property]
	[Title( "Component" )]
	[Feature( CLIENT ), Group( VOICE )]
	protected Voice Voice { get; set; }
	*/

	public bool IsMuted
	{
		get => _isMuted;
		set
		{
			var oldValue = _isMuted;
			_isMuted = value;

			OnSetIsMuted( value, oldValue );
		}
	}

	protected bool _isMuted = false;

	protected virtual void OnSetIsMuted( in bool isMuted, in bool wasMuted )
	{
	}

	/// <summary>
	/// For managing voice component state, transform and such.
	/// </summary>
	protected virtual void UpdateVoice()
	{
	}
}
