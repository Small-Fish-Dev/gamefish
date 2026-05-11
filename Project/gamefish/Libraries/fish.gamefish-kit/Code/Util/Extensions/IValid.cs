using System;

namespace GameFish;

partial class Library
{
	/// <summary>
	/// Returns <c>null</c>(explicitly) if this reference is invalid.
	/// </summary>
	/// <remarks>
	/// Lets you skip checking if <typeparamref name="TValid"/> is valid and not simply null.
	/// <br /> <br />
	/// <b> BEFORE: </b> <c> var thing = one.IsValid() ? one : (two.IsValid() ? two : null); </c>
	/// <br />
	/// <b> AFTER: </b> <c> var thing = one.AsValid() ?? two.AsValid() </c>
	/// </remarks>
	/// <param name="v"> :v </param>
	/// <typeparam name="TValid"> The specific type of the object. </typeparam>
	/// <returns> The <typeparamref name="TValid"/> if it's valid(or null). </returns>
	public static TValid AsValid<TValid>( this TValid v ) where TValid : class, IValid
	{
		if ( v is null || !v.IsValid )
			return null;

		return v;
	}
}
