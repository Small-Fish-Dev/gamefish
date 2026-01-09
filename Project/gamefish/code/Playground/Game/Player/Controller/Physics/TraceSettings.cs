namespace Fishbox;

partial class FishboxController
{
	public partial struct TraceSettings
	{
		public float Grow { get; set; } = 0f;
		public float Skin { get; set; } = 0.5f;

		public TraceSettings() { }

		public TraceSettings( in float fGrow, in float fSkin )
		{
			Grow = fGrow;
			Skin = fSkin;
		}
	}
}
