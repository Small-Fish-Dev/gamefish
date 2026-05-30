using System;
using System.Text.Json.Serialization;
using GameFish;
using GameFish.Razor;

namespace GameFish;

/// <summary>
/// A module that allows players to press buttons(typically USE) to interact with the world.
/// </summary>
[Title( "Usable" )]
[Icon( "touch_app" )]
public abstract partial class UsableModule : Module, IUsable
{
	protected const int USE_ORDER = DEFAULT_ORDER - 2000;
	protected const int USE_DEBUG_ORDER = USE_ORDER - 100;

	/// <summary>
	/// The input code for activation.
	/// </summary>
	[Property]
	[Title( "Logging" )]
	[Order( USE_DEBUG_ORDER )]
	[Feature( USE ), Group( DEBUG )]
	public virtual bool DebugLogging { get; set; } = false;

	/// <summary>
	/// The input code for activation.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Action" )]
	[Feature( USE ), Order( USE_ORDER )]
	public virtual string UseAction { get; set; } = "Use";

	/// <summary>
	/// The display data to use if nothing else is available.
	/// </summary>
	[Title( "Default" )]
	[Property, WideMode]
	[Feature( USE ), Group( DISPLAY )]
	public List<DisplayText> DefaultDisplay { get; set; } = [new( "Use", DisplayElement.Title )];

	/// <summary>
	/// A preview of what's gonna actually be displayed.. probably.
	/// </summary>
	[WideMode]
	[Title( "Preview" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( USE ), Group( DISPLAY )]
	protected List<string> InspectorDisplayLines => GetDisplayLines()?
		.Select( line => line.Text )
		.Where( line => !line.IsBlank() )
		.ToList();

	public virtual float UseOrder( Pawn pawn )
		=> pawn.Center.Distance( WorldPosition );

	/// <returns> The configurable text lines to display(or null). </returns>
	public virtual IEnumerable<DisplayText> GetDisplayLines()
		=> DefaultDisplay;

	/// <returns> If a player is close enough to use this. </returns>
	public virtual bool IsTouching( Pawn pawn )
		=> pawn.IsValid() && Center.Distance( pawn.Center ) < pawn.UseDistance;

	public virtual bool IsUsable( Pawn pawn )
	{
		if ( !pawn.IsValid() || !pawn.IsAlive )
			return false;

		if ( !IsTouching( pawn ) )
			return true;

		return true;
	}

	/// <summary>
	/// Called by the client to ask the owner to use this.
	/// </summary>
	[Rpc.Owner( NetFlags.Reliable | NetFlags.SendImmediate )]
	public void RpcUse()
	{
		if ( !Server.TryFindPawn<Player>( Rpc.Caller, out var pl ) )
		{
			this.Warn( $"Couldn't find {typeof( Player )} for connection:[{Rpc.Caller}]!" );
			return;
		}

		TryUse( pl );
	}

	public bool TryUse( Pawn pawn )
		=> TryUse( pawn as Player );

	/// <summary>
	/// Checks if this can be used and if so calls <see cref="OnUse"/> with error protection.
	/// </summary>
	/// <returns> If this could be used. </returns>
	protected virtual bool TryUse( Player pl )
	{
		if ( !Networking.IsHost || !GameObject.IsValid() )
			return false;

		if ( !pl.IsValid() || !IsUsable( pl ) )
			return false;

		try
		{
			OnUse( pl );
			return true;
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( OnUse )} exception: " + e );
			return false;
		}
	}

	/// <summary>
	/// Performs the function of this usable.
	/// </summary>
	protected virtual void OnUse( Pawn pawn )
	{
		if ( DebugLogging )
			this.Log( $"Used by pawn:[{pawn}]" );
	}

	/// <summary>
	/// Placeholder text/activation method.
	/// </summary>
	protected void DebugInput()
	{
		if ( !InGame )
			return;

		if ( !Server.TryFindPawn<Player>( Connection.Local, out var pl ) )
			return;

		if ( !IsUsable( pl ) )
			return;

		var t = new Transform( WorldPosition, Rotation.FromYaw( -90f ) );
		Gizmo.Draw.WorldText( "[USE]", t, size: 48f, font: "Poppins" );

		if ( Input.Pressed( UseAction ) )
			RpcUse();
	}
}
