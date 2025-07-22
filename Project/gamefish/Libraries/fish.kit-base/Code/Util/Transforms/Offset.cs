namespace GameFish;

/// <summary>
/// A position and a rotation, but not scale. <br />
/// Makes it very easy to animate
/// </summary>
public partial struct Offset
{
	public Vector3 Position { readonly get => _pos; set { _pos = value; } }
	[Hide] private Vector3 _pos;

	public Rotation Rotation { readonly get => _r; set { _r = value; } }
	[Hide] private Rotation _r = Rotation.Identity;

	public static Vector3 Scale => Vector3.One;

	[Hide]
	public Transform Transform
	{
		readonly get => new( Position, Rotation, Scale );
		set
		{
			Position = value.Position;
			Rotation = value.Rotation;
		}
	}

	public Offset()
	{
	}

	public Offset( in Transform t )
	{
		Position = t.Position;
		Rotation = t.Rotation;
	}

	public Offset( in Vector3 pos, in Rotation r )
	{
		Position = pos;
		Rotation = r;
	}

	public readonly Offset LerpTo( in Offset offset, in float frac )
		=> new( Transform.LerpTo( offset.Transform, frac ) );

	/// <param name="t"> The transform to offset from. </param>
	/// <returns> The resulting transform of this offset from the specified transform. </returns>
	public readonly Transform ToWorld( in Transform t )
		=> t.ToWorld( Transform );
}
