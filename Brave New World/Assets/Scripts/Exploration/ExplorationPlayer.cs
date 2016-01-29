using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationPlayer : ExplorationMovableObject
    {
        new void Awake()
        {
            base.Awake();            
            //showMyPossibleMovement = true;
            finishedMoving = false;
            possibleMovement = new List<Vector2>();

        }
        
        void Update()
        {
            if (ExplorationSceneManager.instance.explorationState == ExplorationStateEnum.PlayersTurn)            
            {
                if (!isMoving && !finishedMoving)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject clickedObj = ClickSelect();

                        if (clickedObj != null && clickedObj.tag == "MovableArea")
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
      
        GameObject ClickSelect()
        {
            //Converting Mouse Pos to 2D (vector2) World Pos
            Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

            if (hit)
            {                
                return hit.transform.gameObject;
            }
            else return null;
        }
    }
}