using System;

namespace GameFish;

partial class Library
{
	/// <summary>
	/// Returns <c>null</c>(explicitly) if this reference is invalid.
	/// </summary>
	/// <remarks>
	/// Lets you skip checking if <typeparamref name="TValid"/> is valid and not simply null.
	/// <br />
	/// <b> BEFORE: </b> <c> var thing = one.IsValid() ? one : (two.IsValid() ? two : null); </c>
	/// <br />
	/// <b> AFTER: </b> <c> var thing = one.AsValid() ?? two.AsValid() </c>
	/// </remarks>
	/// <param name="v"> :v </param>
	/// <param name="default"> The fallback(also valid checked). </param>
	/// <typeparam name="TValid"> Allows you to convert the type. </typeparam>
	/// <returns> The <typeparamref name="TValid"/> if it's valid(or null). </returns>
	public static TValid AsValid<TValid>( this TValid v, TValid @default = null ) where TValid : class, IValid
	{
		if ( v?.IsValid is true )
			return v;

		if ( @default?.IsValid is true )
			return @default;

		return null;
	}
}
