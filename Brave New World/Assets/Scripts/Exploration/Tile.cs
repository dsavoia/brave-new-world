using UnityEngine;
namespace BraveNewWorld
{
    public class Tile
    {

        public Vector2 position;
        public TileTypeEnum tileType;
        public bool isOccupied { get; set; }

        //public GameObject tile;

        public int gCost;
        public int hCost;
        public Tile parent;

        public Tile(TileTypeEnum _tileType, Vector2 _tilePosition)
        {
            tileType = _tileType;            
            position = _tilePosition;

            /*switch (tileType)
            {
                case TileTypeEnum.IndestructibleWall:
                case TileTypeEnum.Wall:
                    isOccupied = true;
                    break;
                default:
                    isOccupied = false;
                    break;
            }*/
        }

        /*public void SetTile(GameObject tilePrefab)
        {
            tile = tilePrefab;
        } */      

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }

    }
}