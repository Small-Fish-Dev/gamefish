namespace GameFish;

/// <summary>
/// Puts <see cref="Clothing"/> on an <see cref="PawnSkinnedModel"/>.
/// </summary>
public partial class CitizenOutfitter : Component, Component.ExecuteInEditor
{
	public const string FEATURE_OUTFIT = "👗 Outfit";
	public const string GROUP_AVATAR = "Avatar";
	public const string GROUP_DEFAULT = "Default";

	/// <summary>
	/// Send the owner's avatar when outfitting?
	/// </summary>
	[Property, Feature( FEATURE_OUTFIT ), Group( GROUP_AVATAR )]
	public bool UseAvatar { get; set; } = true;

	/// <summary>
	/// Allow loading your avatar in the editor?
	/// This can be annoying with multiple developers causing frequent changes.
	/// </summary>
	[ShowIf( nameof( UseAvatar ), true )]
	[Property, Feature( FEATURE_OUTFIT ), Group( GROUP_AVATAR )]
	public bool EditorAvatar { get; set; }

	/// <summary>
	/// Prevent avatar clothing with these slots from being used.
	/// </summary>
	[Title( "Prevent Slots" )]
	[ShowIf( nameof( UseAvatar ), true )]
	[Property, Feature( FEATURE_OUTFIT ), Group( GROUP_AVATAR )]
	public Clothing.Slots PreventAvatarSlots { get; set; } = 0;

	/// <summary>
	/// If true: overlays a default outfit. <br />
	/// Overrides any avatar outfitting. Ignores <see cref="PreventAvatarSlots"/>.
	/// </summary>
	[Property, Feature( FEATURE_OUTFIT ), Group( GROUP_DEFAULT )]
	public bool UseDefault { get; set; }

	[Title( "Default Outfit" )]
	[ShowIf( nameof( UseDefault ), true )]
	[Property, InlineEditor, Feature( FEATURE_OUTFIT ), Group( GROUP_DEFAULT )]
	public CitizenOutfit Default { get; set; }

	[Sync]
	public CitizenOutfit Avatar { get => _avatar; set { _avatar = value; ApplyOutfit(); } }
	private CitizenOutfit _avatar;

	/// <summary>
	/// Allows you to temporarily override the avatar and/or custom outfit. <br />
	/// Useful if you wanted to change clothing or make them a skeleton or something.
	/// </summary>
	[Sync]
	public CitizenOutfit Override { get => _override; set { _override = value; ApplyOutfit(); } }
	private CitizenOutfit _override;

	protected ClothingContainer ClothingContainer { get; set; }

	private PawnSkinnedModel _skin;
	protected PawnSkinnedModel Skin
	{
		get => _skin.IsValid() ? _skin : _skin = Components?.Get<PawnSkinnedModel>( FindMode.EnabledInSelfAndDescendants );
		set => _skin = value;
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( this.InGame() )
			ForceUpdate();
	}

	/// <summary>
	/// Immediately applies the current configuration.
	/// </summary>
	[Title( "Apply" )]
	[Button, Feature( FEATURE_OUTFIT )]
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void ForceUpdate()
	{
		// SetAvatar auto-updates outfit.
		if ( !SetAvatar() )
			ApplyOutfit();
	}

	protected void ApplyOutfit( in CitizenOutfit outfit )
	{
		ClothingContainer ??= new();

		foreach ( var element in outfit.Clothing ?? [] )
			if ( element.IsValid() )
				ClothingContainer.Add( element.GetEntry() );

		ClothingContainer.PrefersHuman = outfit.Human ?? ClothingContainer.PrefersHuman;
		ClothingContainer.Height = outfit.Height ?? ClothingContainer.Height;
	}

	/// <summary>
	/// Allows you to temporarily override the avatar and/or custom outfit. <br />
	/// Useful if you wanted to change clothing or make them a skeleton or something.
	/// </summary>
	public void SetOverride( in CitizenOutfit outfit )
		=> Override = outfit;

	/// <summary>
	/// Clears all temporarily forced outfitting.
	/// </summary>
	public void ResetOverrides()
		=> Override = default;

	/// <summary>
	/// Sets the current outfit to the local user's avatar(if you own this).
	/// </summary>
	/// <returns> If you own this. </returns>
	// [ShowIf( nameof( UseAvatar ), true )]
	// [Button, Feature( FEATURE_OUTFIT ), Group( GROUP_AVATAR )]
	protected virtual bool SetAvatar()
	{
		if ( !Scene.IsValid() || !this.IsValid() )
			return false;

		if ( this.InEditor() && !EditorAvatar )
		{
			Avatar = default;
			return false;
		}

		if ( !Network.IsOwner && !this.InEditor() )
			return false;

		var outfit = new CitizenOutfit();
		outfit.Clothing ??= [];

		var userAvatar = ClothingContainer.CreateFromLocalUser();
		var userClothing = userAvatar?.Clothing ?? [];

		foreach ( var entry in userClothing )
		{
			if ( entry?.Clothing.IsValid() ?? false )
			{
				var slotsOver = entry.Clothing.SlotsOver;
				var slotsUnder = entry.Clothing.SlotsUnder;

				if ( PreventAvatarSlots != 0 )
				{
					if ( slotsOver != 0 && PreventAvatarSlots.HasFlag( slotsOver ) )
						continue;

					if ( slotsUnder != 0 && PreventAvatarSlots.HasFlag( slotsUnder ) )
						continue;
				}

				outfit.Add( entry );
			}
		}

		Avatar = outfit;
		return true;
	}

	public virtual void ApplyOutfit( bool forceAvatar = false, bool forceDefault = false )
	{
		if ( !Scene.IsValid() )
			return;

		ClothingContainer ??= new();
		ClothingContainer.Clothing?.Clear();

		if ( forceAvatar || UseAvatar )
			ApplyOutfit( Avatar );

		if ( forceDefault || UseDefault )
			ApplyOutfit( Default );

		ApplyOutfit( Override );

		ClothingContainer.Normalize();

		try
		{
			if ( Skin?.SkinRenderer.IsValid() ?? false )
				ClothingContainer.Apply( Skin.SkinRenderer );
		}
		catch
		{
		}
	}
}
