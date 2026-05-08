using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A position and a rotation, but not scale.
/// Makes it easier to animate programmatically such as tweening transforms.
/// </summary>
public partial struct Offset : IValid
{
	[Hide, JsonIgnore]
	public readonly bool IsValid => ITransform.IsValid( in _pos ) && ITransform.IsValid( in _r );

	[InlineEditor]
	public Vector3 Position { readonly get => _pos; set { _pos = value; } }
	[Hide, JsonIgnore]
	private Vector3 _pos = Vector3.Zero;

	[InlineEditor]
	public Rotation Rotation { readonly get => _r; set { _r = value; } }
	[Hide, JsonIgnore]
	private Rotation _r = Rotation.Identity;

	[Hide, JsonIgnore]
	public Transform Transform
	{
		readonly get => new( Position, Rotation, Scale );
		set
		{
			Position = value.Position;
			Rotation = value.Rotation;
		}
	}

	public static Vector3 Scale => Vector3.One;

	public static implicit operator Transform( in Offset offset ) => offset.Transform;
	public static implicit operator Offset( in Transform t ) => new( t );

	public static bool operator ==( in Offset a, in Offset b ) => a.Equals( b );
	public static bool operator !=( in Offset a, in Offset b ) => !a.Equals( b );

	public readonly override bool Equals( [NotNullWhen( true )] object obj )
	{
		if ( obj is not Offset o )
			return false;

		return o.Position == Position && o.Rotation == Rotation;
	}

	public readonly override int GetHashCode()
		=> HashCode.Combine( Position, Rotation );

	public Offset()
	{
	}

	public Offset( in Transform t )
	{
		var pos = t.Position;
		var r = t.Rotation;

		Position = ITransform.IsValid( in pos ) ? pos : Vector3.Zero;
		Rotation = ITransform.IsValid( in r ) ? r : Rotation.Identity;
	}

	public Offset( in Vector3 pos )
	{
		Position = ITransform.IsValid( in pos ) ? pos : Vector3.Zero;
		Rotation = Rotation.Identity;
	}

	public Offset( in Rotation r )
	{
		Position = Vector3.Zero;
		Rotation = ITransform.IsValid( in r ) ? r : Rotation.Identity;
	}

	public Offset( in Vector3 pos, in Rotation r )
	{
		Position = ITransform.IsValid( in pos ) ? pos : Vector3.Zero;
		Rotation = ITransform.IsValid( in r ) ? r : Rotation.Identity;
	}

	public readonly Offset LerpTo( in Offset offset, in float frac )
		=> new( Transform.LerpTo( offset.Transform, frac ) );

	/// <summary>
	/// Adds this offset to a transform. <br />
	/// </summary>
	/// <remarks> This is ideal for affecting world transforms. </remarks>
	/// <returns> A transform offset by this position and rotation. </returns>
	public readonly Transform AddTo( in Transform t )
		=> t.WithOffset( this );
}
