namespace Fishbox;

partial class FishboxController
{
	public partial struct TraceSettings
	{
		public float Skin { get; set; } = 0f;

		public TraceSettings() { }

		public TraceSettings( in float skin )
		{
			Skin = skin;
		}
	}
}
