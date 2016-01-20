using UnityEngine;
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

        protected bool isMoving = false;
        public bool finishedMoving = true;

        public float movementDuration;

        protected void Awake()
        {
            path = new List<Tile>();
            showedPossibleMovements = false;
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
            possibleMovement.Clear();
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
        }

        public void Move()
        {
            
            Vector3[] pathToFollow = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                pathToFollow[i] = new Vector3(path[i].position.x, path[i].position.y);
                /*if (explorationManager.boardManager.board[(int)pathToFollow[i].x, (int)pathToFollow[i].y].isOccupied)
                {
                    Debug.Log("errr");
                }*/
            }

            isMoving = true;
            finishedMoving = false;
            explorationManager.boardManager.board[(int)transform.position.x, (int)transform.position.y].isOccupied = false;
            explorationManager.boardManager.board[(int)path[path.Count-1].position.x, (int)path[path.Count-1].position.y].isOccupied = true;

            transform.DOLocalPath(pathToFollow, path.Count/1.5f).OnComplete(() => FinishMovement());
        }

        void FinishMovement()
        {
            //Debug.Log("finished moving");
            isMoving = false;
            finishedMoving = true;            
        }
    }
}

