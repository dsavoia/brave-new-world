using System;
using UnityEngine;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class Dungeon
    {
        /*public struct door{
            Vector2 coord;		
            int doorToDungeonNumber;
        }*/

        public Dungeon()
        {

        }

        public Tile[,] map { get; set; }
        public int MapWidth { get; set; }
        public int MapHeigth { get; set; }

        public List<Vector2> FloorCoords { get; set; }
        public List<Vector2> WallCoords { get; set; }
        public List<Vector2> DoorCoords { get; set; }
        public List<Vector2> IndestructibleWallCoords { get; set; }
        public Vector2 InitialPos { get; set; }
        //public int MapMovementType { get; set; }
        //public int MapNumber { get; set; }
        //public List<door> Doors {get; set;}

        public List<Tile> GetNeighbours(Tile tile)
        {
            List<Tile> neighbours = new List<Tile>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    //Not considering the diagonal pos and the origin pos
                    if (Mathf.Abs(x) == Mathf.Abs(y))
                        continue;

                    //if (x == 0 && y == 0)
                    //continue;

                    int checkX = (int)tile.position.x + x;
                    int checkY = (int)tile.position.y + y;

                    if (checkX >= 0 && checkX < MapWidth && checkY >= 0 && checkY < MapHeigth)
                    {
                        neighbours.Add(map[checkX, checkY]);
                    }
                }
            }

            return neighbours;
        }

    }
}