using System.Collections.Generic;
using Godot;

namespace Bulwark.Mapping
{
    public class ProceduralMap : MultiMeshInstance
    {
        private const float TILE_WIDTH = 17.315f;
        private const float TILE_HEIGHT = 20f;
        
        [Export] private Vector2 mapSize = new Vector2(10, 10);
        [Export] private int mapSeed = 1001;
        [Export] private List<Mesh> tileMeshes;

        private readonly OpenSimplexNoise _noise = new OpenSimplexNoise();
        private readonly RandomNumberGenerator rng = new RandomNumberGenerator();

        private Vector3 mapStartingPosition;

        public override void _Ready()
        {
            _noise.Seed = mapSeed;
            rng.Seed = (ulong) mapSeed;
            
            CalculateMapStartingPosition();
            
            for (var x = 0; x < mapSize.x; x++)
            {
                for (var y = 0; y < mapSize.y; y++)
                {
                    var tilePosition = CalculateTileWorldPosition(x, y);

                    var noiseSample = _noise.GetNoise3dv(tilePosition);
                    if (noiseSample < -0.1f) continue;

                    var selectedMesh = tileMeshes[rng.RandiRange(0, tileMeshes.Count - 1)];
                    var tileMeshInstance = new MeshInstance
                    {
                        Mesh = selectedMesh, 
                        Translation = tilePosition,
                        Name = $"Tile_{selectedMesh.ResourceName}_{x}_{y}"
                    };
                    AddChild(tileMeshInstance);
                }
            }
        }
        
        private void CalculateMapStartingPosition()
        {
            float offset = 0;
            if (mapSize.y / 2 % 2 != 0)
                offset = TILE_WIDTH / 2;

            var x = -TILE_WIDTH * (mapSize.x / 2) - offset;
            var z = TILE_HEIGHT * 0.75f * (mapSize.y / 2);

            mapStartingPosition = new Vector3(x, 0, z);
        }
        
        private Vector3 CalculateTileWorldPosition(int x, int y)
        {
            var offset = 0f;
            if (y % 2 != 0)
                offset = TILE_WIDTH / 2f;

            var rx = mapStartingPosition.x + x * TILE_WIDTH + offset;
            var rz = mapStartingPosition.y + y * TILE_HEIGHT * 0.75f;

            return new Vector3(rx, 0, rz);
        }
    }
}
