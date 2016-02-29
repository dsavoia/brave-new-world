using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{

    public class Pathfinding : MonoBehaviour
    {        

        public GameObject pathPrefab;

        public List<Tile> FindPath(Vector3 startPos, Vector3 targetPos)
        {       

            Tile startTile = ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)startPos.x, (int)startPos.y];
            Tile targetTile = ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)targetPos.x, (int)targetPos.y];

            List<Tile> openSet = new List<Tile>();
            HashSet<Tile> closedSet = new HashSet<Tile>();

            openSet.Add(startTile);

            while (openSet.Count > 0)
            {
                Tile currentTile = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentTile.fCost || openSet[i].fCost == currentTile.fCost && openSet[i].hCost < currentTile.hCost)
                    {
                        currentTile = openSet[i];
                    }
                }

                openSet.Remove(currentTile);
                closedSet.Add(currentTile);

                if (currentTile == targetTile)
                {
                    return RetracePath(startTile, targetTile);                    
                }

                foreach (Tile neighbour in ExplorationSceneManager.instance.dungeonManager.dungeon.GetNeighbours(currentTile))
                {
                    //if (neighbour.isOccupied || closedSet.Contains(neighbour))
                    if (neighbour.tileType != TileTypeEnum.Floor || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);

                    if (neighbour.isOccupied)
                    {
                        newMovementCostToNeighbour += 20;//Arbitrary cost just so occupied tiles are not counted on the movment
                    }

                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetTile);
                        neighbour.parent = currentTile;

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            return null;
        }

        List<Tile> RetracePath(Tile startTile, Tile targetTile)
        {
            List<Tile> path = new List<Tile>();
            Tile currentTile = targetTile;

            while (currentTile != startTile)
            {
                path.Add(currentTile);
                currentTile = currentTile.parent;
            }

            path.Reverse();

            return path;
        }

        int GetDistance(Tile tileA, Tile tileB)
        {
            int dstX = (int)Mathf.Abs(tileA.position.x - tileB.position.x);
            int dstY = (int)Mathf.Abs(tileA.position.y - tileB.position.y);

            //TODO: the value 1 is the cost to get to the tile. To implement terrain with difficulties to move this value should be changed!!
            //Using Manhattan distance
            return 1 * (dstX + dstY);

            /*if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);*/

        }       
    }
}
