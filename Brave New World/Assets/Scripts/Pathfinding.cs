using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{

    public class Pathfinding : MonoBehaviour
    {

        ExplorationSceneManager explorationManager;

        //public Transform seeker, target;

        public GameObject pathPrefab;
        private Transform pathParent;

        void Awake()
        {
            explorationManager = GameObject.Find("ExplorationManager").GetComponent<ExplorationSceneManager>();
        }

        void Update()
        {
            //if (Input.GetMouseButtonDown(0))
            //{
                //FindPath(seeker.position, target.position);
            //}

        }

        public List<Tile> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            //Debug.Log("Calculating Path");
            //Debug.Log(startPos);
            //Debug.Log(targetPos);

            Tile startTile = explorationManager.boardManager.board[(int)startPos.x, (int)startPos.y];
            Tile targetTile = explorationManager.boardManager.board[(int)targetPos.x, (int)targetPos.y]; ;

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

                foreach (Tile neighbour in explorationManager.boardManager.GetNeighbours(currentTile))
                {
                    if (neighbour.isOccupied || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
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
            //ShowPath(path);
            //Debug.Log(path.ToArray());

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

        /*void ShowPath(List<Tile> finalPath)
        {
            if (pathParent != null)
                Destroy(pathParent.gameObject);

            pathParent = new GameObject("PathParent").transform;
            pathParent.transform.SetParent(explorationManager.boardManager.boardParent);

            GameObject instance;

            for (int i = 0; i < finalPath.Count; i++)
            {
                instance = Instantiate(pathPrefab, finalPath[i].position, Quaternion.identity) as GameObject;
                instance.transform.SetParent(pathParent);
            }

            //showedPossibleMovements = true;
        }*/

    }
}
