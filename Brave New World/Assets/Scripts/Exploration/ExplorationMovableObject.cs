using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace BraveNewWorld
{
    public class ExplorationMovableObject : MonoBehaviour
    {
        protected ExplorationSceneManager explorationManager;
        protected List<Vector2> possibleMovement;
        protected Transform movementParent;

        public GameObject movementHighlight;
        protected Pathfinding pathFinding;

        protected bool moving = false;

        public float movementDuration;

        protected void Awake()
        {
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

        protected void ShowPossibleMovement(int movementRange)
        {
            movementParent = new GameObject(gameObject.name + " MovementParent").transform;
            movementParent.transform.SetParent(explorationManager.boardManager.boardParent);

            possibleMovement.Clear();

            CalculatePossiblePosition(movementRange, transform.position);

            GameObject instance;

            for (int i = 0; i < possibleMovement.Count; i++)
            {
                instance = Instantiate(movementHighlight, possibleMovement[i], Quaternion.identity) as GameObject;
                instance.transform.SetParent(movementParent);
            }
        }

        protected void Move(List<Tile> path)
        {
            /*moving = true;
            for (int i = 0; i < path.Count; i++)
            {
                Debug.Log(path[i].position);
                Vector2.MoveTowards(transform.position, path[i].position, speed*Time.deltaTime);
            } */

            Vector3[] pathToFollow = new Vector3[path.Count];

            for (int i = 0; i < path.Count; i++)
                pathToFollow[i] = new Vector3(path[i].position.x, path[i].position.y);

            transform.DOLocalPath(pathToFollow, movementDuration);
        }
    }
}