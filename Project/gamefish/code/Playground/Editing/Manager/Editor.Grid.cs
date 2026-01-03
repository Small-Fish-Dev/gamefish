namespace Playground;

partial class Editor
{
	[Property]
	[Range( 0f, 1024f )]
	[Feature( EDITOR ), Group( TRANSFORMS )]
	public float GridSize
	{
		get => _gridSize.Max( GRID_SIZE_MIN );
		set
		{
			if ( _gridSize == value )
				return;

			var old = _gridSize;
			_gridSize = value.Clamp( GRID_SIZE_MIN, GRID_SIZE_MAX );

			OnGridSet( in _gridSize, in old );
		}
	}

	protected float _gridSize = 8f;

	protected virtual void OnGridSet( in float newGrid, in float oldGrid )
	{
	}

	protected virtual void UpdateGrid( in float deltaTime )
	{
		if ( Input.Pressed( "Grid Decrease" ) )
			GridSize = (GridSize / 2f).Round();

		if ( Input.Pressed( "Grid Increase" ) )
			GridSize = (GridSize * 2f).CeilToInt();
	}
}
