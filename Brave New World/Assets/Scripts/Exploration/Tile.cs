using UnityEngine;
namespace BraveNewWorld
{
    public class Tile
    {

        public Vector2 position;
        public TileTypeEnum tileType;
        public bool isOccupied { get; set; }

        public GameObject tile;

        public int gCost;
        public int hCost;
        public Tile parent;

        public Tile(TileTypeEnum _tileType, GameObject tilePrefab, Vector2 _tilePosition)
        {
            tileType = _tileType;
            tile = tilePrefab;
            position = _tilePosition;

            switch (tileType)
            {
                case TileTypeEnum.OutterWall:
                case TileTypeEnum.Obstacles:
                    isOccupied = true;
                    break;
                default:
                    isOccupied = false;
                    break;
            }
        }

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }

    }
}