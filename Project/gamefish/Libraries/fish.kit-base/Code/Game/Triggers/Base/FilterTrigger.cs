using System;

namespace GameFish;

/// <summary>
/// A trigger volume with tag, type and custom function filters. <br />
/// Capable of creating, updating and previewing its collision.
/// </summary>
[Title( "Filtered Trigger" )]
public partial class FilterTrigger : BaseTrigger
{
	public const string FEATURE_FILTERS = "📋 Filters";

	public const string GROUP_FILTER_TAGS = "🏳 Tag Filter";
	public const string GROUP_FILTER_TYPE = "⌨ Type Filter";
	public const string GROUP_FILTER_FUNC = "💻 Function Filter";

	public const int ORDER_FILTER_TAGS = 69;
	public const int ORDER_FILTER_TYPE = 88;

	/// <summary>
	/// If true: include/exclude by type.
	/// </summary>
	[Order( ORDER_FILTER_TYPE )]
	[Feature( FEATURE_FILTERS )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public bool FilterType { get; set; } = true;

	/// <summary>
	/// They must have this type of component on them.
	/// </summary>
	[Order( ORDER_FILTER_TYPE )]
	[Feature( FEATURE_FILTERS )]
	[ShowIf( nameof( FilterType ), true )]
	[TargetType( typeof( Component ) )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public Type RequireType { get; set; } = typeof( BasePlayer );

	/// <summary>
	/// How to look for the component.
	/// </summary>
	[Order( ORDER_FILTER_TYPE )]
	[Feature( FEATURE_FILTERS )]
	[ShowIf( nameof( FilterType ), true )]
	[TargetType( typeof( Component ) )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public FindMode FindMode { get; set; } = FindMode.EnabledInSelf | FindMode.InAncestors | FindMode.InDescendants;

	/// <summary>
	/// If true: include/exclude by tags.
	/// </summary>
	[Order( ORDER_FILTER_TAGS )]
	[Feature( FEATURE_FILTERS )]
	[Property, Group( GROUP_FILTER_TAGS )]
	public bool FilterTags { get; set; } = true;

	/// <summary>
	/// An object with any of these tags are accepted.
	/// </summary>
	[Order( ORDER_FILTER_TAGS )]
	[Feature( FEATURE_FILTERS )]
	[ShowIf( nameof( FilterTags ), true )]
	[Property, Group( GROUP_FILTER_TAGS )]
	public TagFilter IncludeTags { get; set; } = new() { Enabled = true, Tags = [BaseEntity.TAG_PLAYER] };

	/// <summary>
	/// An object with any of these tags are always ignored. <br />
	/// They're excluded even if they have an <see cref="IncludeTags"/> tag.
	/// </summary>
	[Order( ORDER_FILTER_TAGS )]
	[Feature( FEATURE_FILTERS )]
	[Property, Group( GROUP_FILTER_TAGS )]
	[ShowIf( nameof( FilterTags ), true )]
	public TagFilter ExcludeTags { get; set; }

	/// <summary>
	/// An additional, final check you can do in ActionGraph.
	/// </summary>
	[Order( ORDER_FILTER_TYPE + 1 )]
	[Feature( FEATURE_FILTERS )]
	[Property, Group( GROUP_FILTER_FUNC ), Title( "Passes Filter" )]
	public Func<BaseTrigger, GameObject, bool> FunctionFilter { get; set; }

	[Order( ORDER_CALLBACK )]
	[Property, Group( GROUP_CALLBACK )]
	public Action<BaseTrigger, GameObject> OnFailedFilter { get; set; }

	public override Color GizmoColor { get; } = Color.Green.Desaturate( 0.6f ).Darken( 0.25f );

	protected override bool TestFilters( GameObject obj )
	{
		if ( !PassesFilters( obj ) )
		{
			DebugLog( obj + " FAILED the filter " );

			if ( OnFailedFilter is not null )
			{
				try
				{
					OnFailedFilter.Invoke( this, obj );
				}
				catch ( Exception e )
				{
					this.Warn( $"{nameof( OnFailedFilter )} callback exception: {e}" );
				}
			}

			return false;
		}

		DebugLog( obj + " PASSED the filter" );

		return true;
	}

	/// <returns> If the object passes this trigger's tag filters(if any) and custom filter(if any). </returns>
	public override bool PassesFilters( GameObject obj )
	{
		if ( !obj.IsValid() || !base.PassesFilters( obj ) )
			return false;

		if ( !TagsPassFilters( obj.Tags ) )
			return false;

		if ( !TypesPassFilters( obj ) )
			return false;

		if ( FunctionFilter is not null )
		{
			try
			{
				return FunctionFilter.Invoke( this, obj );
			}
			catch ( Exception e )
			{
				this.Warn( $"PassesFilter callback exception: {e}" );
			}
		}

		return true;
	}

	/// <summary>
	/// Returns if this set of tags is allowed. <br />
	/// Defaults to true if <see cref="FilterTags"/> is disabled.
	/// </summary>
	public virtual bool TypesPassFilters( GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		if ( !FilterType || RequireType is null )
			return true;

		return obj.Components.Get( RequireType, FindMode ).IsValid();
	}

	/// <summary>
	/// Returns if this set of tags is allowed. <br />
	/// Defaults to true if <see cref="FilterTags"/> is disabled.
	/// </summary>
	public virtual bool TagsPassFilters( ITagSet tags )
	{
		if ( !FilterTags )
			return true;

		if ( tags is null )
			return false;

		var passed = false;

		// Include
		if ( IncludeTags.HasAny( tags ) )
			passed = true;

		// Exclude
		if ( ExcludeTags.HasAny( tags ) )
			return false;

		return passed;
	}
}
