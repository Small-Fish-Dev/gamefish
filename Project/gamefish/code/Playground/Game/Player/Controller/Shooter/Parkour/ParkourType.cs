namespace Fishbox;

partial class ShooterController
{
	public enum ParkourType
	{
		/// <summary>
		/// Hoofing it around like a fat cow.
		/// </summary>
		[Icon( "🐄" )]
		None,

		/// <summary>
		/// Sliding down slopes.
		/// </summary>
		[Icon( "🏄" )]
		Sliding,

		/// <summary>
		/// Running along the side of walls.
		/// </summary>
		[Icon( "🐱‍👤" )]
		Riding,

		/// <summary>
		/// Sticking to a surface while redirecting gravity.
		/// </summary>
		[Icon( "⬇" )]
		Sticking,

		/// <summary>
		/// Pulling self up a ledge.
		/// </summary>
		[Icon( "🧗" )]
		Mantling,
	}
}