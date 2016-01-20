using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationSceneManager : MonoBehaviour
    {


        public Vector2 boardSize;
        public int obstaclesQuantitity, enemiesQty;

        public bool playersTurn, enemiesTurn;
        private bool enemiesMoving = false;
        private bool enemiesFinishedMoving = true;

        public GameObject[] enemyPrefab;
        public BoardManager boardManager;
        List<GameObject> enemiesList;

        private Transform enemiesParent;

        // Use this for initialization
        void Awake()
        {
            boardManager = GetComponent<BoardManager>();
            playersTurn = true;
            enemiesTurn = false;            
            enemiesList = new List<GameObject>();
            InitExploration();
            
        }

        void SetEnemies(int quantity)
        {
            Vector2 randomPos;            

            enemiesParent = new GameObject("EnemiesParent").transform;
            enemiesParent.parent = boardManager.boardParent;

            while (quantity > 0) {
                randomPos = new Vector2(Random.Range(1,boardSize.x), Random.Range(1, boardSize.y));
                if ((randomPos.x != 1 && randomPos.y != 1) && !boardManager.board[(int)randomPos.x, (int)randomPos.y].isOccupied)
                {
                    GameObject enemy = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], new Vector2((int)randomPos.x, (int)randomPos.y), Quaternion.identity) as GameObject;
                    enemy.transform.parent = enemiesParent;
                    boardManager.board[(int)randomPos.x, (int)randomPos.y].isOccupied = true;
                    enemiesList.Add(enemy);
                    quantity--;
                }
             
            }
        }

        void InitExploration()
        {
            boardManager.BoardSetUp(boardSize, obstaclesQuantitity);            
            SetEnemies(enemiesQty);
        }

        void Update()
        {
            if (!playersTurn && !enemiesMoving)
            {
               MoveEnemies();
            }
            else if (!playersTurn && enemiesMoving)
            {
                foreach (GameObject enemy in enemiesList)
                {
                    if (!enemy.GetComponent<ExplorationEnemy>().finishedMoving)
                    {
                        enemiesMoving = true;                      
                        return;
                    }                    
                }                
                enemiesFinishedMoving = true;
                enemiesMoving = false;
            }

            if (!playersTurn && enemiesFinishedMoving)
            {   
                playersTurn = true;
            }
        }

        void MoveEnemies()
        {
            enemiesMoving = true;
            enemiesFinishedMoving = false;
            foreach(GameObject enemy in enemiesList)
            {
                enemy.GetComponent<ExplorationEnemy>().Move();
            }     
        }
    }
}