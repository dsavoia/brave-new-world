using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationPlayer : ExplorationMovableObject
    {
        // Use this for initialization
        new void Awake()
        {
            base.Awake();
            transform.position = new Vector3(1, 1, transform.position.z);            
            showMyPossibleMovement = true;
            possibleMovement = new List<Vector2>();

        }

        //TODO Take this out from update so the manager gains more control over the turn
        void Update()
        {
            if (!explorationManager.playersTurn)
            {
                if (movementParent != null)
                {
                    Destroy(movementParent.gameObject);
                }
                return;
            }
            else
            {
                if (!isMoving)
                {
                    if (!showedPossibleMovements)
                    {
                        showedPossibleMovements = true;                        
                        PossibleMovement();
                    }

                    if (Input.GetMouseButtonDown(0))
                    {

                        GameObject clickedObj = ClickSelect();

                        if (clickedObj != null && clickedObj.tag == "MovableArea")
                        {
                            path.Clear();
                            path = pathFinding.FindPath(transform.position, clickedObj.transform.position);
                            //Debug.Log("Should be moving");
                            Move();
                            Destroy(movementParent.gameObject);
                            showedPossibleMovements = false;                            
                            //explorationManager.boardManager.ShowPath();                    
                        }

                    }
                }
                else
                {
                    explorationManager.playersTurn = false;
                    Debug.Log("Finished moving");
                }
            }
        }

        //This method returns the game object that was clicked using Raycast 2D
        GameObject ClickSelect()
        {
            //Converting Mouse Pos to 2D (vector2) World Pos
            Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);

            if (hit)
            {
                //Debug.Log(hit.transform.name);
                return hit.transform.gameObject;
            }
            else return null;
        }
    }
}