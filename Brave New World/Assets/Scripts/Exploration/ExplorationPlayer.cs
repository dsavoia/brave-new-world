using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationPlayer : MonoBehaviour
    {


        public int movementRange = 1;
        public GameObject movementHighlight;

        ExplorationSceneManager explorationManager;

        private List<Vector2> possibleMovement;

        private Transform movementParent;
        private bool showedPossibleMovements;

        Rigidbody2D rb2D;

        // Use this for initialization
        void Awake()
        {
            transform.position = new Vector3(1, 1, transform.position.z);
            possibleMovement = new List<Vector2>();
            explorationManager = GameObject.Find("ExplorationManager").GetComponent<ExplorationSceneManager>();
            showedPossibleMovements = false;
            rb2D = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!explorationManager.playersTurn)
            {
                return;
            }

            if (!showedPossibleMovements)
                ShowPossibleMovement();

            //#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
            if (Input.GetMouseButtonDown(0))
            {
                GameObject clickedObj = ClickSelect();

                if (clickedObj != null && clickedObj.tag == "MovableArea")
                {
                    //Debug.Log("Should be moving");
                    rb2D.MovePosition(clickedObj.transform.position);
                    showedPossibleMovements = false;
                    explorationManager.playersTurn = false;
                    //explorationManager.boardManager.ShowPath();
                    Destroy(movementParent.gameObject);
                }

            }
            //#else

            /* if (Input.touchCount > 0)
             {                
                 GameObject clickedObj = ClickSelect();

                 if (clickedObj.tag == "MovementPosition")
                 {
                     rb2D.MovePosition(clickedObj.transform.position);
                     showedPossibleMovements = false;
                     explorationManager.playersTurn = false;
                     Destroy(movementParent.gameObject);
                 }
             }*/

            //#endif
        }

        /*void ShowPossibleMovement()
        {

            movementParent = new GameObject("MovementParent").transform;
            movementParent.transform.SetParent(explorationManager.boardManager.boardParent);

            possibleMovement.Clear();
            int currentRangeQty = movementRange;

            for (int i = -movementRange; i <= movementRange; i++)
            {
                for (int j = -movementRange; j <= movementRange; j++)
                {
                    if (((int)transform.position.x + i > 0 && (int)transform.position.x + i < explorationManager.boardManager.boardSize.x) &&
                        ((int)transform.position.y + j > 0 && (int)transform.position.y + j < explorationManager.boardManager.boardSize.y))
                    {
                        if ((Mathf.Abs(i) + Mathf.Abs(j)) <= movementRange)
                        {
                            if (explorationManager.boardManager.board[(int)transform.position.x + i, (int)transform.position.y + j].isOccupied)
                            {
                                //continue;
                            }
                            else
                            {
                                //Debug.Log("X: " + (int)transform.position.x + i + " Y: " + (int)transform.position.y + j + " gcost: " + explorationManager.boardManager.board[(int)transform.position.x + i, (int)transform.position.y + j].gCost);
                                possibleMovement.Add(new Vector2(transform.position.x + i, transform.position.y + j));
                            }
                        }
                    }
                }
            }

            GameObject instance;

            for (int i = 0; i < possibleMovement.Count; i++)
            {
                instance = Instantiate(movementHighlight, possibleMovement[i], Quaternion.identity) as GameObject;
                instance.transform.SetParent(movementParent);
            }

            showedPossibleMovements = true;
        }*/

        void ShowPossibleMovement()
        {

            movementParent = new GameObject("MovementParent").transform;
            movementParent.transform.SetParent(explorationManager.boardManager.boardParent);

            possibleMovement.Clear();

            CalculatePossiblePosition(movementRange, transform.position);

            GameObject instance;

            for (int i = 0; i < possibleMovement.Count; i++)
            {
                instance = Instantiate(movementHighlight, possibleMovement[i], Quaternion.identity) as GameObject;
                instance.transform.SetParent(movementParent);
            }

            showedPossibleMovements = true;
        }

        void CalculatePossiblePosition(int steps, Vector2 pos)
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