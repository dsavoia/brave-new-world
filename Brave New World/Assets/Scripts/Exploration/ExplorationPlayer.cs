using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BraveNewWorld
{
    public class ExplorationPlayer : ExplorationMovableObject
    {

        public GameObject attackHighLightPB;
        protected Transform attackHighLightParent;


        new void Awake()
        {
            base.Awake();            
            //showMyPossibleMovement = true;
            finishedMoving = false;
            possibleMovement = new List<Vector2>();

        }

        void FixedUpdate()
        {
            if (ExplorationSceneManager.instance.explorationState == ExplorationStateEnum.PlayersTurn)            
            {
                if (!isMoving && !finishedMoving)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject clickedObj = ClickSelect();

                        if (clickedObj != null && clickedObj.tag == "Enemy")
                        {
                            Debug.Log("Clicked on: " + clickedObj.name);
                            //path = new List<Tile>();
                            HighLightObjectSurroudingArea(clickedObj.transform.position);
                            //path = pathFinding.FindPath(transform.position, clickedObj.transform.position);
                        } else if (clickedObj != null && clickedObj.tag == "MovableArea")
                        {                            
                            path = new List<Tile>();
                            path = pathFinding.FindPath(transform.position, clickedObj.transform.position);                                                       
                            Move();                            
                            Destroy(movementParent.gameObject);                            
                        }                       
                    }
                }
            }
        }

        public void BeginTurn()
        {            
            PossibleMovement();
        }

        void HighLightObjectSurroudingArea(Vector2 targetPos)
        {

            Vector2 checkPos = new Vector2();
            attackHighLightParent = new GameObject(gameObject.name + " AttackHightlightParent").transform;
            attackHighLightParent.transform.SetParent(movementParent.transform);

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    checkPos.x = targetPos.x + i;
                    checkPos.y = targetPos.y + j;

                    if (!ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)checkPos.x, (int)checkPos.y].isOccupied && possibleMovement.Contains(checkPos))
                    {
                        GameObject instance = Instantiate(attackHighLightPB, checkPos, Quaternion.identity) as GameObject;
                        instance.transform.SetParent(attackHighLightParent.transform);
                    }
                                            
                }
            }           
        }

        GameObject ClickSelect()
        {
            //Converting Mouse Pos to 2D (vector2) World Pos
            Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 100);

            if (hit)
            {                
                return hit.transform.gameObject;
            }
            else return null;
        }
    }
}