using System;

namespace GameFish;

partial class DataFile<TDataComp, TDataClass>
{
	/// <returns> A new instance of <typeparamref name="TDataClass"/>. </returns>
	protected virtual TDataClass CreateData()
		=> TypeLibrary.Create<TDataClass>( typeof( TDataClass ), args: null );

	/// <summary>
	/// The class meant to contain <typeparamref name="TDataComp"/>'s data. <br />
	/// Define its <typeparamref name="TDataClass"/> as a derivation of this.
	/// </summary>
	public abstract partial class DataClass
	{
		public double Created { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
	}
}
