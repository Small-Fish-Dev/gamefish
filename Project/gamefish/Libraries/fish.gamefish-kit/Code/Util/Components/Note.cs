using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Lets you provide information.
/// </summary>
[Title( "Note" )]
[Group( Library.NAME )]
[Icon( "sticky_note_2" )]
public partial class NoteComponent : Component
{
	[DefaultValue( Todo )]
	public enum CommentType
	{
		[Icon( "📝" )]
		[Title( "To-do" )]
		Todo,

		[Icon( "📰" )]
		Info,

		[Icon( "🐞" )]
		Bug,

		[Icon( "👽" )]
		Funny,

		[Icon( "⚠" )]
		[Title( "IMPORTANT" )]
		Important,
	}

	public struct Comment
	{
		[JsonInclude]
		[KeyProperty]
		[WideMode( HasLabel = false )]
		public CommentType Type;

		// [TextArea]
		[JsonInclude]
		[KeyProperty]
		[WideMode( HasLabel = false )]
		public string Message;
	}

	/// <summary>
	/// The main message.
	/// </summary>
	[Property]
	[TextArea]
	[Title( "Message" )]
	[WideMode( HasLabel = false )]
	public string Text { get; set; }

	[TextArea]
	[Property]
	[InlineEditor]
	[WideMode( HasLabel = false )]
	[Group( COMMENTS, StartFolded = false )]
	public List<Comment> Comments { get; set; }
}
