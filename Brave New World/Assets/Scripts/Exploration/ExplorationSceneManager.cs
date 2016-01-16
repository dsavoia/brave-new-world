using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationSceneManager : MonoBehaviour
    {


        public Vector2 boardSize;
        public int obstaclesQuantitity;

        public bool playersTurn, waitingForMovement;

        public BoardManager boardManager;
        List<Enemy> enemiesList;

        // Use this for initialization
        void Awake()
        {
            boardManager = GetComponent<BoardManager>();
            playersTurn = true;
            waitingForMovement = false;
            InitExploration();
        }

        void InitExploration()
        {
            boardManager.BoardSetUp(boardSize, obstaclesQuantitity);
        }


        // Update is called once per frame
        void Update()
        {
            if (!playersTurn && !waitingForMovement)
                StartCoroutine(MockEnemiesTurn());
        }

        IEnumerator MockEnemiesTurn()
        {
            waitingForMovement = true;
            yield return new WaitForSeconds(1.5f);
            playersTurn = true;
            waitingForMovement = false;
        }
    }
}