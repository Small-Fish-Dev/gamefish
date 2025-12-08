namespace GameFish;

/// <summary>
/// Add this to stuff that uses a <see cref="SkinnedModelRenderer"/>.
/// </summary>
public interface ISkinned
{
    public SkinnedModelRenderer SkinRenderer { get; set; }
}
