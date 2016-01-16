using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace BraveNewWorld
{
    public class BoardManager : MonoBehaviour
    {

        public GameObject[] obstaclesTiles;
        public GameObject[] outterWallTiles;
        public GameObject[] floorTiles;

        [HideInInspector]
        public Vector2 boardSize;
        [HideInInspector]
        public int obstaclesQuantitity;

        public Transform boardParent;
        private Transform outterWallParent;
        private Transform floorParent;
        private Transform obstaclesParent;

        public Tile[,] board;
        List<Tile> obstacles;

        public void BoardSetUp(Vector2 size, int obstaclesQty)
        {
            boardSize = size;
            obstaclesQuantitity = obstaclesQty;
            board = new Tile[(int)boardSize.x, (int)boardSize.y];
            obstacles = new List<Tile>();

            InitBoard();
            placeObstacles();
            InstantiateBoard();
        }

        private void InitBoard()
        {
            for (int i = 0; i < boardSize.x; i++)
            {
                for (int j = 0; j < boardSize.y; j++)
                {
                    if (i == 0 || i == boardSize.x - 1 || j == 0 || j == boardSize.y - 1)
                    {
                        board[i, j] = new Tile(TileTypeEnum.OutterWall, outterWallTiles[Random.Range(0, outterWallTiles.Length)], new Vector2(i, j));
                    }
                    else
                    {
                        board[i, j] = new Tile(TileTypeEnum.Floor, floorTiles[Random.Range(0, floorTiles.Length)], new Vector2(i, j));
                    }
                }
            }
        }

        private void placeObstacles()
        {
            int obstaclesQty = obstaclesQuantitity;
            Vector2 randomPos;

            while (obstaclesQty > 0)
            {
                randomPos = new Vector2((int)Random.Range(1, boardSize.x), (int)Random.Range(1, boardSize.y));

                if (!board[(int)randomPos.x, (int)randomPos.y].isOccupied)
                {
                    obstacles.Add(new Tile(TileTypeEnum.Obstacles, obstaclesTiles[Random.Range(0, obstaclesTiles.Length)], randomPos));
                    board[(int)randomPos.x, (int)randomPos.y].isOccupied = true;
                    obstaclesQty--;
                }
            }
        }

        public List<Tile> GetNeighbours(Tile tile)
        {
            List<Tile> neighbours = new List<Tile>();

            for (int x = -1; x<= 1; x++)
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

                    if(checkX >= 0 && checkX < boardSize.x && checkY >= 0 && checkY < boardSize.y)
                    {
                        neighbours.Add(board[checkX,checkY]);
                    }
                }
            }

            return neighbours;
        }


        private void InstantiateBoard()
        {
            boardParent = new GameObject("BoardParent").transform;
            outterWallParent = new GameObject("OutterWallParent").transform;
            floorParent = new GameObject("FloorParent").transform;
            obstaclesParent = new GameObject("ObstaclesParent").transform;

            outterWallParent.transform.SetParent(boardParent);
            floorParent.transform.SetParent(boardParent);
            obstaclesParent.transform.SetParent(boardParent);

            GameObject instance;

            for (int i = 0; i < boardSize.x; i++)
            {
                for (int j = 0; j < boardSize.y; j++)
                {
                    instance = Instantiate(board[i, j].tile, board[i, j].position, Quaternion.identity) as GameObject;

                    switch (board[i, j].tileType)
                    {
                        case TileTypeEnum.Floor:
                            instance.transform.SetParent(floorParent);
                            break;
                        case TileTypeEnum.OutterWall:
                            instance.transform.SetParent(outterWallParent);
                            break;
                        default:
                            instance.transform.SetParent(boardParent);
                            break;
                    }
                }
            }

            for (int i = 0; i < obstacles.Count; i++)
            {
                instance = Instantiate(obstacles[i].tile, obstacles[i].position, Quaternion.identity) as GameObject;
                instance.transform.SetParent(obstaclesParent);
            }
        }

        public List<Tile> path;
        public void ShowPath()
        {
            if(board != null)
            {
                foreach (Tile t in board)
                {
                    
                    if(path != null)
                    {
                        
                        if (path.Contains(t))
                        {                           
                            //Gizmos.color = Color.yellow;
                            //Gizmos.DrawCube(new Vector3(t.position.x, t.position.y, 0), Vector3.one);
                            Debug.Log(t.position);
                            //board[(int)t.position.x, (int)t.position.y].tile.GetComponent<SpriteRenderer>().color = Color.yellow;
                        }//else
                            //board[(int)t.position.x, (int)t.position.y].tile.GetComponent<SpriteRenderer>().color = Color.red;


                    }
                }
            }
        }
    }
}