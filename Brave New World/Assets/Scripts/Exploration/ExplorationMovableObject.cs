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

        protected List<GameObject> ObjectsArroundMe;

        protected void Awake()
        {
            path = new List<Tile>();            
            possibleMovement = new List<Vector2>();
            ObjectsArroundMe = new List<GameObject>();
            pathFinding =  GameObject.Find("Pathfinding").GetComponent<Pathfinding>();
        }


        //TODO: MAKE A BETTER SOLUTION
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

                    //Debug.Log(gameObject.name + " verificando x: " + pos.x + " y: " + pos.y);
                 
                    //if (((int)pos.x + i > 0 && (int)pos.x + i < ExplorationSceneManager.instance.dungeonManager.dungeon.MapWidth) &&
                    //((int)pos.y + j > 0 && (int)pos.y + j < ExplorationSceneManager.instance.dungeonManager.dungeon.MapHeigth))
                    //{
                    //if (!ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)pos.x + i, (int)pos.y + j].isOccupied)
                    if (ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)pos.x, (int)pos.y].tileType == TileTypeEnum.Floor)
                        {
                            if (ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)pos.x, (int)pos.y].isOccupied 
                                && ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)pos.x, (int)pos.y].OccupyingObject != gameObject
                                && !ObjectsArroundMe.Contains(ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)pos.x, (int)pos.y].OccupyingObject))
                            {
                                ObjectsArroundMe.Add(ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)pos.x, (int)pos.y].OccupyingObject);
                                //Debug.Log("Ta ocupado x: " + pos.x + " y: " + pos.y);
                            }

                            if (!possibleMovement.Contains(new Vector2(pos.x, pos.y)))
                            {
                                possibleMovement.Add(new Vector2(pos.x, pos.y));
                            }
                            CalculatePossiblePosition(steps - 1, new Vector2(pos.x + i, pos.y + j));
                        }
                    //}
                }
            }            
        }

        protected void PossibleMovement()
        {
            possibleMovement = new List<Vector2>();
            ObjectsArroundMe.Clear();
            CalculatePossiblePosition(movementRange, transform.position);
            //DEBUG REASON ONLY
            foreach (GameObject go in ObjectsArroundMe)
            {
                Debug.Log(" Inside " + gameObject.name + "'s area is: " + go.name);
            }

            if (showMyPossibleMovement)
            {   
                movementParent = new GameObject(gameObject.name + " MovementParent").transform;
                movementParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);
             
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

                ChangeOccupiedPosition(new Vector2(pathToFollow[pathToFollow.Length - 1].x, pathToFollow[pathToFollow.Length - 1].y));                
                transform.DOPath(pathToFollow, path.Count / (movementSpeed == 0 ? 1 : movementSpeed), PathType.Linear, PathMode.Sidescroller2D, 0).OnComplete(() => EndMovement());
            }
            else
            {
                //TODO: FIX THIS
                Debug.Log("Would've bugged");
                EndMovement();
            }					
        }
        
        void EndMovement()
        {
            isMoving = false;
            finishedMoving = true;
            if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }
        }

        public void ChangeOccupiedPosition(Vector2 posToOccupy)
        {
            ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)transform.position.x, (int)transform.position.y].isOccupied = false;
            ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)transform.position.x, (int)transform.position.y].OccupyingObject = null;
            ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)posToOccupy.x, (int)posToOccupy.y].isOccupied = true;
            ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)posToOccupy.x, (int)posToOccupy.y].OccupyingObject = gameObject;
        }        
    }
}

