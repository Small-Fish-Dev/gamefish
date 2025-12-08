namespace GameFish;

/// <summary>
/// Lets you provide information.
/// </summary>
[Title( "Note" )]
[Group( Library.NAME )]
[Icon( "sticky_note_2" )]
public partial class NoteComponent : Component
{
	[Property]
	[TextArea, WideMode]
	// [Feature( GameFish.NOTE )] // why?
	public string Text { get; set; }
}
