using System.Text.Json.Serialization;

namespace Playground;

partial class EditorTool
{
	/// <summary>
	/// Do we have an origin we're working from?
	/// </summary>
	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Group( DEBUG ), Order( EDITOR_DEBUG_ORDER )]
	public bool HasOrigin { get; set; }

	/// <summary>
	/// The thing we're trying to do stuff on top of.
	/// </summary>
	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Group( DEBUG ), Order( EDITOR_DEBUG_ORDER )]
	public GameObject OriginObject { get; set; }

	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Group( DEBUG ), Order( EDITOR_DEBUG_ORDER )]
	public Component OriginComponent { get; set; }

	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Group( DEBUG ), Order( EDITOR_DEBUG_ORDER )]
	public Offset OriginOffset { get; set; }

	/// <summary>
	/// The last known transform of the origin.
	/// </summary>
	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Group( DEBUG ), Order( EDITOR_DEBUG_ORDER )]
	public Transform OriginWorldTransform { get; protected set; } = global::Transform.Zero;

	protected virtual void ClearOrigin()
	{
		HasOrigin = false;

		OriginObject = null;
		OriginComponent = null;
		OriginOffset = default;

		OriginWorldTransform = global::Transform.Zero;
	}

	protected virtual void UpdateOrigin( in float deltaTime )
	{
		if ( !HasOrigin )
			return;

		if ( OriginObject.IsValid() )
			OriginWorldTransform = OriginObject.WorldTransform;
	}

	protected virtual bool TrySetOrigin( GameObject obj, Component c, Offset offset, bool allowReplace = true )
	{
		if ( !obj.IsValid() )
			return false;

		// respec'
		if ( !allowReplace && HasOrigin )
			return false;

		SetOrigin( offset, obj, c );

		return true;
	}

	protected virtual void SetOrigin( Offset offset, GameObject obj = null, Component c = null )
	{
		HasOrigin = true;

		OriginObject = obj;
		OriginComponent = c;
		OriginOffset = offset;
	}
}
