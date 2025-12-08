using System;

namespace GameFish;

/// <summary>
/// A trigger volume with tag, type and custom function filters. <br />
/// Capable of creating, updating and previewing its collision.
/// </summary>
[Title( "Filtered Trigger" )]
public partial class FilterTrigger : BaseTrigger
{
	public const string GROUP_FILTER_TAGS = "🏳 Tag Filter";
	public const string GROUP_FILTER_TYPE = "⌨ Type Filter";
	public const string GROUP_FILTER_FUNC = "💻 Function Filter";

	public const int FILTER_TAGS_ORDER = TRIGGER_ORDER + 10;
	public const int FILTER_TYPE_ORDER = FILTER_TAGS_ORDER + 1;
	public const int FILTER_FUNC_ORDER = FILTER_TYPE_ORDER + 2;

	/// <summary>
	/// If true: include/exclude by type.
	/// </summary>
	[Feature( FILTERS )]
	[Order( FILTER_TYPE_ORDER )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public bool FilterType { get; set; } = false;

	/// <summary>
	/// They must have this type of component on them.
	/// </summary>
	[Feature( FILTERS )]
	[Title( "Require Type" )]
	[Order( FILTER_TYPE_ORDER )]
	[ShowIf( nameof( FilterType ), true )]
	[TargetType( typeof( Component ) )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public Type FilterRequireType { get; set; } = typeof( Pawn );

	/// <summary>
	/// How to look for the component.
	/// </summary>
	[Feature( FILTERS )]
	[Title( "Find Mode" )]
	[Order( FILTER_TYPE_ORDER )]
	[ShowIf( nameof( FilterType ), true )]
	[TargetType( typeof( Component ) )]
	[Property, Group( GROUP_FILTER_TYPE )]
	public FindMode FilterFindMode { get; set; } = FindMode.EnabledInSelf | FindMode.InAncestors;

	/// <summary>
	/// If true: include/exclude by tags.
	/// </summary>
	[Feature( FILTERS )]
	[Order( FILTER_TAGS_ORDER )]
	[Property, Group( GROUP_FILTER_TAGS )]
	public bool FilterTags { get; set; } = true;

	/// <summary>
	/// An object with any of these tags are accepted.
	/// </summary>
	[Feature( FILTERS )]
	[Title( "Include Tags" )]
	[Order( FILTER_TAGS_ORDER )]
	[ShowIf( nameof( FilterTags ), true )]
	[Property, Group( GROUP_FILTER_TAGS )]
	public TagFilter FilterIncludeTags { get; set; } = new() { Enabled = true, Tags = [TAG_PAWN] };

	/// <summary>
	/// An object with any of these tags are always ignored. <br />
	/// They're excluded even if they have an <see cref="FilterIncludeTags"/> tag.
	/// </summary>
	[Feature( FILTERS )]
	[Title( "Exclude Tags" )]
	[Order( FILTER_TAGS_ORDER )]
	[Property, Group( GROUP_FILTER_TAGS )]
	[ShowIf( nameof( FilterTags ), true )]
	public TagFilter FilterExcludeTags { get; set; }

	/// <summary>
	/// An additional, final check you can do in ActionGraph.
	/// </summary>
	[Feature( FILTERS )]
	[Order( FILTER_FUNC_ORDER )]
	[Property, Group( GROUP_FILTER_FUNC ), Title( "Passes Filter" )]
	public Func<BaseTrigger, GameObject, bool> FilterFunction { get; set; }

	[Order( CALLBACKS_ORDER )]
	[Property, Feature( TRIGGER ), Group( CALLBACKS )]
	public Action<BaseTrigger, GameObject> OnFailedFilter { get; set; }

	public override Color DefaultGizmoColor { get; } = Color.Green.Desaturate( 0.6f ).Darken( 0.25f );

	protected override bool TestFilters( GameObject obj )
	{
		if ( !PassesFilters( obj ) )
		{
			if ( DebugTrigger )
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

		if ( DebugTrigger )
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

		if ( FilterFunction is not null )
		{
			try
			{
				return FilterFunction.Invoke( this, obj );
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

		if ( !FilterType || FilterRequireType is null )
			return true;

		return obj.Components.Get( FilterRequireType, FilterFindMode ).IsValid();
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
		if ( FilterIncludeTags.HasAny( tags ) )
			passed = true;

		// Exclude
		if ( FilterExcludeTags.HasAny( tags ) )
			return false;

		return passed;
	}
}
