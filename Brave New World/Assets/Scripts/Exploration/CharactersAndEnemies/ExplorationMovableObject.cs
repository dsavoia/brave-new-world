using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//using UnityEditor;

namespace BraveNewWorld
{
    public abstract class ExplorationMovableObject : MonoBehaviour
    {        
        protected List<Vector2> possibleMovement;
        protected List<Vector2> occupiedPosList;
        protected List<GameObject> objectsArroundMe;

        protected Transform movementParent;
        protected List<Tile> path;

        public bool showMyPossibleMovement;
        protected bool showedPossibleMovements;

        public GameObject movementHighlightPB;
        public GameObject enemiesHighLightPB;
        protected Transform enemiesHighLightParent;        

        protected Animator animator;

        public int movementRange = 1;

        protected Pathfinding pathFinding;

        public bool isMoving = false;
        public bool finishedMoving = true;

        public float movementSpeed;

        public Slider healthBar;
        public int maxHP;
        public int actualHP;

        protected void Awake()
        {
            path = new List<Tile>();            
            possibleMovement = new List<Vector2>();
            occupiedPosList = new List<Vector2>();
            objectsArroundMe = new List<GameObject>();
            pathFinding =  GameObject.Find("Pathfinding").GetComponent<Pathfinding>();
            animator = GetComponent<Animator>();
            actualHP = maxHP;
            healthBar.maxValue = maxHP;
            healthBar.minValue = 0;
            healthBar.value = actualHP;
        }
        
        //TODO: HIGLIGHT PREFAB CLASS SO I CAN DIFFERENTIATE BETWEEN HIGHLIGHTS
        protected void CalculatePossiblePosition(int range, Vector2 pos)
        {
            Tile actualPos = ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)pos.x, (int)pos.y];            
            List<Vector2> openSet = new List<Vector2>();
            List<Vector2> auxiliarSet = new List<Vector2>();           

            //Adding first neighbours
            foreach (Tile neighbour in ExplorationSceneManager.instance.dungeonManager.dungeon.GetNeighbours(actualPos))
            {
                if (neighbour.tileType == TileTypeEnum.Floor)
                {
                    if (!neighbour.isOccupied)
                    {
                        openSet.Add(neighbour.position);
                    }
                    else
                    {
                        occupiedPosList.Add(neighbour.position);
                    }
                }
            }

            for (int i = 0; i < range; i++)
            {
                foreach (Vector2 openSetPos in openSet)
                {                    
                    foreach (Tile neighbour in ExplorationSceneManager.instance.dungeonManager.dungeon.GetNeighbours(ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)openSetPos.x, (int)openSetPos.y]))
                    {
                        if (neighbour.tileType == TileTypeEnum.Floor && !auxiliarSet.Contains(neighbour.position))
                        {
                            if (!neighbour.isOccupied)
                            {
                                auxiliarSet.Add(neighbour.position);
                            }
                            else if (!occupiedPosList.Contains(neighbour.position))
                            {
                                occupiedPosList.Add(neighbour.position);
                            }
                        }
                    }

                    if (!possibleMovement.Contains(openSetPos))
                    {
                        possibleMovement.Add(openSetPos);
                    }
                }

                openSet.Clear();

                foreach (Vector2 auxiliarPos in auxiliarSet)
                {
                    openSet.Add(auxiliarPos);
                }

                auxiliarSet.Clear();                                
            }

            occupiedPosList.Remove(pos);

            foreach (Vector2 occupiedPos in occupiedPosList)
            {
                objectsArroundMe.Add(ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)occupiedPos.x, (int)occupiedPos.y].OccupyingObject);
            }

            possibleMovement.Add(pos);
        }

        protected void PossibleMovement()
        {
            possibleMovement = new List<Vector2>();
            objectsArroundMe = new List<GameObject>();
            occupiedPosList = new List<Vector2>();

            CalculatePossiblePosition(movementRange, transform.position);

            if (showMyPossibleMovement)
            {   
                movementParent = new GameObject(gameObject.name + " MovementParent").transform;
                movementParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);
             
                GameObject instance;

                if (occupiedPosList.Count > 0)
                {   
                    HighLightObjectsArroundMe();
                }

                for (int i = 0; i < possibleMovement.Count; i++)
                {                    
                    instance = Instantiate(movementHighlightPB, possibleMovement[i], Quaternion.identity) as GameObject;
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

        public virtual void EndMovement()
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

        public abstract IEnumerator Attack(GameObject target);        
        public abstract float TakeDamage(int damage);
        
        public virtual IEnumerator Die()
        {
            ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)transform.position.x, (int)transform.position.y].isOccupied = false;
            ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)transform.position.x, (int)transform.position.y].OccupyingObject = null;

            return null;
        }

        public void RecoverHealth(int recoverQuantity)
        {
            actualHP += recoverQuantity;

            if(actualHP > maxHP)
            {
                actualHP = maxHP;
            }

            healthBar.value = actualHP;
        }

        public abstract void HighLightObjectsArroundMe();        
    }
}

