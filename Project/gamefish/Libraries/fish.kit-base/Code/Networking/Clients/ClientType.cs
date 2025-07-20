namespace GameFish;

public enum ClientType
{
	/// <summary>
	/// Not yet properly configured.
	/// </summary>
	[Icon( "⚠" )]
	Invalid = 0,

	/// <summary>
	/// A real Client.
	/// </summary>
	[Icon( "🧔" )]
	User,

	/// <summary>
	/// A fake Client.
	/// </summary>
	[Icon( "🤖" )]
	Bot,
}
