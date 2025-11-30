using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A <see cref="DataFile{TDataComp, TDataClass}"/> component designed for performance profiling.
/// </summary>
public partial class TestFile : DataFile<TestFile, TestFile.Data>
{
	[Property]
	[Feature( SAVING ), Group( TIMING ), Order( ORDER - 5 )]
	public int Iterations { get; set; } = 100;

	[Property, ReadOnly, JsonIgnore]
	[Feature( SAVING ), Group( TIMING ), Order( ORDER - 5 )]
	public string TimeTaken { get; set; }

	public override string FileName => $"test.json";

	public partial class Data : DataClass
	{
		public List<int> TestList { get; set; }
		public int TestInteger { get; set; }
	}

	[Button( "Test Read" )]
	[Feature( SAVING ), Group( TIMING ), Order( ORDER - 5 )]
	public void DoReadTest()
	{
		int integer = Random.Int( 0, int.MaxValue - 1 );

		this.Log( $"{nameof( Data.TestInteger )} pre: {integer}" );

		Set( nameof( Data.TestInteger ), integer );

		RealTimeSince startTime = 0f;

		using ( new CodeTimer( "Read Performance Test" ) )
			for ( var i = 0; i < Iterations; i++ )
				Get( nameof( Data.TestInteger ), out integer, Random.Int( int.MinValue + 1, 0 ) );

		TimeTaken = startTime.Relative.ToString();

		this.Log( $"{nameof( Data.TestInteger )} post: {integer}" );
	}

	[Button( "Test Read Write" )]
	[Feature( SAVING ), Group( TIMING ), Order( ORDER - 5 )]
	public void DoReadWriteTest()
	{
		RealTimeSince startTime = 0f;

		using ( new CodeTimer( "Write Performance Test" ) )
		{
			Set( nameof( Data.TestList ), new List<int>() );

			for ( var i = 0; i < Iterations; i++ )
			{
				if ( Get( nameof( Data.TestList ), out List<int> list, [] ) )
				{
					list ??= [];
					list.Add( i );

					if ( !Set( nameof( Data.TestList ), list ) )
						this.Warn( "Couldn't set." );
				}
			}
		}

		TimeTaken = startTime.Relative.ToString();
	}
}
