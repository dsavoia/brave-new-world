using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BraveNewWorld
{
    public class ExplorationPlayer : ExplorationMovableObject
    {

        public enum CharacterState
        {
            WaitingOrder,
            Moving,
            Attacking,
            EndTurn,
            WaitingNextTurn,
        }

        public int meleeAttackRange = 1;

        public CharacterState characterState;
        public GameObject attackHighLightPB;
       
        protected Transform attackHighLightParent;

        new void Awake()
        {
            base.Awake();            
            //showMyPossibleMovement = true;
            finishedMoving = false;
            possibleMovement = new List<Vector2>();
            characterState = CharacterState.WaitingNextTurn;

        }

        void FixedUpdate()
        {
            if (ExplorationSceneManager.instance.explorationState == ExplorationStateEnum.PlayersTurn)
            {
                //if (!isMoving && !finishedMoving)
                if (characterState == CharacterState.WaitingOrder)
                {
                    //Debug.Log("waiting for orders");
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject clickedObj = ClickSelect();

                        if (clickedObj != null)
                        {
                            if (clickedObj.tag == "Enemy")
                            {
                                HighLightObjectMeleeAttackArea(clickedObj.transform.position);
                            }
                            else if (clickedObj.tag == "MeleeAttackArea")
                            {
                                path = new List<Tile>();
                                path = pathFinding.FindPath(transform.position, clickedObj.transform.position);
                                characterState = CharacterState.Attacking;
                                Move();
                                Destroy(movementParent.gameObject);
                            }
                            else if (clickedObj.tag == "MovableArea")
                            {
                                path = new List<Tile>();
                                path = pathFinding.FindPath(transform.position, clickedObj.transform.position);
                                characterState = CharacterState.Moving;
                                Move();
                                Destroy(movementParent.gameObject);
                            }
                        }
                    }
                }

                if (characterState == CharacterState.Attacking)
                {                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        GameObject clickedObj = ClickSelect();

                        if (clickedObj != null)
                        {
                            if (clickedObj.tag == "Enemy")
                            {
                                if (VerifyIfOnRange(meleeAttackRange, clickedObj.gameObject.transform.position))
                                {
                                    Debug.Log("Attacked!");
                                    Destroy(attackHighLightParent.gameObject);
                                    characterState = CharacterState.EndTurn;
                                }
                            }
                        }
                    }
                }          
            }
        }

        bool VerifyIfOnRange(int range, Vector2 targetPosition)
        {
            float distance = (Mathf.Abs(transform.position.x) - Mathf.Abs(targetPosition.x) + (Mathf.Abs(transform.position.y) - Mathf.Abs(targetPosition.y)));
            return (distance <= range);            
        }

        public override void EndMovement()
        {
            base.EndMovement();
            if (characterState == CharacterState.Moving)
            {
                characterState = CharacterState.EndTurn;
            }            
        }

        public void BeginTurn()
        {
            characterState = CharacterState.WaitingOrder;
            PossibleMovement();
        }

        void HighLightObjectMeleeAttackArea(Vector2 targetPos)
        {
            if(attackHighLightParent != null)
            {
                Destroy(attackHighLightParent.gameObject);
            }

            Vector2 checkPos = new Vector2();
            attackHighLightParent = new GameObject(gameObject.name + " AttackHightlightParent").transform;
            attackHighLightParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (Mathf.Abs(i) == Mathf.Abs(j))
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);

            

            if (hit.Length > 0)
            {                
                for (int i = 0; i<hit.Length; i++)
                {
                    ExplorationMovableObject go = hit[i].collider.gameObject.GetComponent<ExplorationMovableObject>() as ExplorationMovableObject;

                    if (go != null)
                    {
                        Debug.DrawLine(ray.origin, hit[i].point);
                        Debug.Log("Hit object: " + hit[i].collider.gameObject.name);
                        return hit[i].collider.gameObject;
                    }
                }

                return hit[0].collider.gameObject;
            }

            else return null;
        }
    }
}