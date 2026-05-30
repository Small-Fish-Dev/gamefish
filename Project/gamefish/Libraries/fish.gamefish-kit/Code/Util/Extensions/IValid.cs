namespace GameFish;

partial class Library
{
	/// <summary>
	/// Returns explicitly <c>null</c> if this reference is invalid.
	/// </summary>
	/// <remarks>
	/// Lets you skip checking if a <typeparamref name="TValid"/> is valid and not simply null.
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

	/// <summary>
	/// Returns explicitly <c>null</c> if this reference is not a valid <typeparamref name="TValid"/>.
	/// </summary>
	/// <remarks>
	/// Lets you skip checking if it's a valid <typeparamref name="TValid"/> and not simply null.
	/// <br /> <br />
	/// <b> BEFORE: </b> <c> if ( (Thing is <typeparamref name="TValid"/> thing &amp;&amp; thing.IsValid() ) thing.Foo(); </c>
	/// <br />
	/// <b> AFTER: </b> <c> Thing.AsValid&lt;Stuff&gt;()?.Foo(); </c>
	/// </remarks>
	/// <param name="v"> :v </param>
	/// <typeparam name="TValid"> The specific type of the object. </typeparam>
	/// <returns> The <typeparamref name="TValid"/> if it's valid(or null). </returns>
	public static TValid AsValid<TValid>( this IValid v ) where TValid : class, IValid
	{
		if ( v is null || !v.IsValid )
			return null;

		return v as TValid;
	}

	/// <summary>
	/// Tells you if that's a valid, non-<c>null</c> <typeparamref name="TValid"/>.
	/// </summary>
	/// <param name="v"> :v </param>
	/// <typeparam name="TValid"> The specific type of the object. </typeparam>
	/// <returns> The <typeparamref name="TValid"/> if it's valid(or null). </returns>
	public static bool IsValid<TValid>( this IValid v ) where TValid : class, IValid
		=> (v as TValid).IsValid();

	/// <summary>
	/// Provides a reference that's only EVER a valid <typeparamref name="TValid"/> or <c>null</c>.
	/// </summary>
	/// <remarks>
	/// Lets you skip defining a local variable of <typeparamref name="TValid"/> and then checking if it's valid.
	/// <br /> <br />
	/// <b> BEFORE: </b> <c> var thing = Thing; if ( thing.IsValid() ) thing.Foo(); </c>
	/// <br />
	/// <b> AFTER: </b> <c> if ( Thing.IsValid( out var thing ) ) thing.Foo(); </c>
	/// </remarks>
	/// <param name="v"> :v </param>
	/// <typeparam name="TValid"> The specific type of the object. </typeparam>
	/// <param name="obj"> The resulting <typeparamref name="TValid"/>(or null). </param>
	/// <returns> The <typeparamref name="TValid"/> if it's valid(or null). </returns>
	public static bool IsValid<TValid>( this TValid v, out TValid obj ) where TValid : class, IValid
	{
		obj = v.AsValid();
		return v is not null;
	}

	/// <summary>
	/// Provides a reference that's only EVER a valid <typeparamref name="TValid"/> or <c>null</c>.
	/// </summary>
	/// <remarks>
	/// Lets you skip defining a local variable of <typeparamref name="TValid"/> and then checking if it's valid.
	/// <br /> <br />
	/// <b> BEFORE: </b> <c> var thing = Thing; if ( thing.IsValid() ) thing.Foo(); </c>
	/// <br />
	/// <b> AFTER: </b> <c> if ( Thing.IsValid( out var thing ) ) thing.Foo(); </c>
	/// </remarks>
	/// <param name="v"> :v </param>
	/// <typeparam name="TValid"> The specific type of the object. </typeparam>
	/// <param name="obj"> The resulting <typeparamref name="TValid"/>(or null). </param>
	/// <returns> The <typeparamref name="TValid"/> if it's valid(or null). </returns>
	public static bool IsValid<TValid>( this IValid v, out TValid obj ) where TValid : class, IValid
		=> (v as TValid).IsValid( out obj );
}
