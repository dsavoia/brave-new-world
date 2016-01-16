using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationPlayer : ExplorationMovableObject
    {
        public int movementRange = 1;
        public bool showedPossibleMovements;
        List<Tile> path;
        Rigidbody2D rb2D;

        // Use this for initialization
        void Awake()
        {
            base.Awake();
            transform.position = new Vector3(1, 1, transform.position.z);
            possibleMovement = new List<Vector2>();
            path = new List<Tile>();
            showedPossibleMovements = false;
            rb2D = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
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

            if (!showedPossibleMovements)
            {
                showedPossibleMovements = true;
                ShowPossibleMovement(movementRange);
            }


            if (Input.GetMouseButtonDown(0))
            {
                GameObject clickedObj = ClickSelect();

                if (clickedObj != null && clickedObj.tag == "MovableArea")
                { 
                    path.Clear();
                    path = pathFinding.FindPath(transform.position, clickedObj.transform.position);
                    //Debug.Log("Should be moving");
                    //rb2D.MovePosition(clickedObj.transform.position);                    
                    Move(path);
                    Destroy(movementParent.gameObject);
                    showedPossibleMovements = false;
                    explorationManager.playersTurn = false;                    
                    //explorationManager.boardManager.ShowPath();                    
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