using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BraveNewWorld
{
    public class ExplorationCharacter : ExplorationMovableObject
    {

        public enum CharacterState
        {
            WaitingOrder,
            Moving,
            MovingToAttack,
            Attacking,
            WaitingAnimation,
            WaitingNextTurn,            
            EndTurn
        }

        public int meleeAttackRange = 1;
        public int HealthQty = 10;

        public CharacterState characterState;
        
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
                switch(characterState)
                {
                    case (CharacterState.WaitingOrder):
                        if (Input.GetMouseButtonDown(0))
                        {
                            GameObject clickedObj = ClickSelect();

                            if (clickedObj != null)
                            {
                                //Debug.Log(clickedObj.tag);
                                if (clickedObj.tag == "Enemy")
                                {
                                    if (occupiedPosList.Contains(clickedObj.transform.position))
                                    {
                                        path = new List<Tile>();
                                        path = pathFinding.FindPath(transform.position, clickedObj.transform.position);
                                        characterState = CharacterState.MovingToAttack;
                                        path.RemoveAt(path.Count - 1);
                                        Move();                                        
                                    }
                                }
                                else if (clickedObj.tag == "MovableArea")
                                {
                                    path = new List<Tile>();
                                    path = pathFinding.FindPath(transform.position, clickedObj.transform.position);
                                    characterState = CharacterState.Moving;
                                    Move();                                    
                                }
                            }
                        }
                    break;
                    //Debug.Log("waiting for orders");
                    case (CharacterState.Attacking):
                        if (Input.GetMouseButtonDown(0))
                        {
                            GameObject clickedObj = ClickSelect();

                            if (clickedObj != null)
                            {
                                if (clickedObj.tag == "Enemy")
                                {
                                    if (VerifyIfOnRange(meleeAttackRange, clickedObj.transform.position))
                                    {
                                        characterState = CharacterState.WaitingAnimation;
                                        StartCoroutine(Attack(clickedObj));                                        
                                    }
                                }
                            }
                        }
                    break;
                }        
            }
        }

        public void BeginTurn()
        {
            characterState = CharacterState.WaitingOrder;
            PossibleMovement();            
        }

        bool VerifyIfOnRange(int range, Vector2 targetPosition)
        {
            float distance = (Mathf.Abs(transform.position.x) - Mathf.Abs(targetPosition.x) + (Mathf.Abs(transform.position.y) - Mathf.Abs(targetPosition.y)));
            return (distance <= range);            
        }

        public override void EndMovement()
        {
            base.EndMovement();
            switch (characterState)            
            {
                case CharacterState.Moving:
                    characterState = CharacterState.EndTurn;
                    break;
                case CharacterState.MovingToAttack:
                    characterState = CharacterState.Attacking;
                    break;

            }            
        }        

        GameObject ClickSelect()
        {
            //Converting Mouse Pos to 2D (vector2) World Pos
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity);

            if (hit.Length > 0)
            {
                for (int i = 0; i < hit.Length; i++)
                {
                    ExplorationMovableObject go = hit[i].collider.gameObject.GetComponent<ExplorationMovableObject>() as ExplorationMovableObject;

                    if (go != null)
                    {
                        return hit[i].collider.gameObject;
                    }
                }

                return hit[0].collider.gameObject;
            }

            else return null;
        }

        public override void HighLightEnemies()
        {
            //Vector2 checkPos = new Vector2();
            enemiesHighLightParent = new GameObject(gameObject.name + " EnemiesHightlightParent").transform;
            enemiesHighLightParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);

            if (occupiedPosList.Count > 0)
                foreach (Vector2 pos in occupiedPosList)
                {
                    GameObject instance = Instantiate(enemiesHighLightPB, pos, Quaternion.identity) as GameObject;
                    instance.transform.SetParent(enemiesHighLightParent.transform);
                }

        }

        public override void HighLightAllies()
        {
            throw new NotImplementedException();
        }

        public override void HighLightNeutrals()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator Attack(GameObject target)
        {
            animator.SetTrigger("PlayerChop");

            yield return new WaitForSeconds(target.GetComponent<ExplorationMovableObject>().TakeDamage(1));

            characterState = CharacterState.EndTurn;           
        }

        public override float TakeDamage(int damage)
        {
            float animationTime = 1.0f;
            Debug.Log("Taking Damage");
            HealthQty -= damage;

            animator.SetTrigger("PlayerHit");

            if (HealthQty <= 0)
            {
                return Die();
            }

            return animationTime;
        }

        public override float Die()
        {
            base.Die();

            float animationTime = 1.0f;
            return animationTime;            
        }
    }
}