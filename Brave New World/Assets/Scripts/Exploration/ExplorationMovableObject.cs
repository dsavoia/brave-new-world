using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//using UnityEditor;

namespace BraveNewWorld
{
    public class ExplorationMovableObject : MonoBehaviour
    {
        protected ExplorationSceneManager explorationManager;
        protected List<Vector2> possibleMovement;
        protected Transform movementParent;
        protected List<Tile> path;

        protected bool showMyPossibleMovement = true;
        protected bool showedPossibleMovements;

        public GameObject movementHighlight;
        public int movementRange = 1;

        protected Pathfinding pathFinding;

        public bool isMoving = false;
        public bool finishedMoving = true;

        public float movementSpeed;

        protected void Awake()
        {
            path = new List<Tile>();            
            possibleMovement = new List<Vector2>();
            explorationManager = GameObject.Find("ExplorationManager").GetComponent<ExplorationSceneManager>();
            pathFinding =  GameObject.Find("Pathfinding").GetComponent<Pathfinding>();
        }

        protected void CalculatePossiblePosition(int steps, Vector2 pos)
        {
            if (steps < 0)
                return;

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (Mathf.Abs(i) == Mathf.Abs(j))
                        continue;
                    if (((int)pos.x + i > 0 && (int)pos.x + i < explorationManager.boardManager.boardSize.x) &&
                    ((int)pos.y + j > 0 && (int)pos.y + j < explorationManager.boardManager.boardSize.y))
                    {
                        if (!explorationManager.boardManager.board[(int)pos.x + i, (int)pos.y + j].isOccupied)
                        {
                            if (!possibleMovement.Contains(new Vector2(pos.x, pos.y)))
                            {
                                possibleMovement.Add(new Vector2(pos.x, pos.y));
                            }
                            CalculatePossiblePosition(steps - 1, new Vector2(pos.x + i, pos.y + j));
                        }
                    }
                }
            }            
        }

        protected void PossibleMovement()
        {
            possibleMovement = new List<Vector2>();
            CalculatePossiblePosition(movementRange, transform.position);

            if (showMyPossibleMovement)
            {
                //Debug.Log(gameObject.name + " Showed possible movement");
                
                movementParent = new GameObject(gameObject.name + " MovementParent").transform;
                movementParent.transform.SetParent(explorationManager.boardManager.boardParent);
                //EditorApplication.isPaused = true;
                GameObject instance;               

                for (int i = 0; i < possibleMovement.Count; i++)
                {                    
                    instance = Instantiate(movementHighlight, possibleMovement[i], Quaternion.identity) as GameObject;
                    instance.transform.SetParent(movementParent);
                }
            }

            finishedMoving = false;
        }

        public void Move()
        {            
            Vector3[] pathToFollow = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                pathToFollow[i] = new Vector3(path[i].position.x, path[i].position.y);
            }

            isMoving = true;

            if (pathToFollow.Length > 0) {
                explorationManager.boardManager.board[(int)transform.position.x, (int)transform.position.y].isOccupied = false;
                explorationManager.boardManager.board[(int)pathToFollow[pathToFollow.Length - 1].x, (int)pathToFollow[pathToFollow.Length - 1].y].isOccupied = true;
                transform.DOPath(pathToFollow, path.Count / (movementSpeed == 0 ? 1 : movementSpeed), PathType.Linear, PathMode.Sidescroller2D, 0).OnComplete(() => EndTurn());
            }
            else
            {
                Debug.Log("Would've bugged");
                EndTurn();
            }					
        }
        
        void EndTurn()
        {
            isMoving = false;
            finishedMoving = true;
            if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }
        }        
    }
}

