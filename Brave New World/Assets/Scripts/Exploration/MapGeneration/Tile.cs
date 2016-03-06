using UnityEngine;
namespace BraveNewWorld
{
    public class Tile
    {

        public Vector2 position;
        public TileTypeEnum tileType;
        public bool isOccupied { get; set; }

        public GameObject OccupyingObject{ get; set; }

        public int gCost;
        public int hCost;
        public Tile parent;

        public Tile(TileTypeEnum _tileType, Vector2 _tilePosition)
        {
            tileType = _tileType;            
            position = _tilePosition;
            OccupyingObject = null;
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