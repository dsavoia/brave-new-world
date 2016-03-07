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
            MovingToTarget,
            WalkedtoTarget,
            WaitingAnimation,
            WaitingNextTurn,            
            EndTurn,
            Dead
        }

        public int meleeAttackRange = 1;

        public LayerMask layerMask;

        public CharacterState characterState;

        public GameObject exitHighLightPB;
        protected Transform exitHighLightParent;

        public GameObject cancelActionButton;

        new void Awake()
        {
            base.Awake();            
            //showMyPossibleMovement = true;
            finishedMoving = false;
            possibleMovement = new List<Vector2>();
            characterState = CharacterState.WaitingNextTurn;            
        }

        void Update()
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
                                
                                if (clickedObj.tag == "MovableArea")
                                {
                                    path = new List<Tile>();
                                    path = pathFinding.FindPath(transform.position, clickedObj.transform.position);
                                    characterState = CharacterState.Moving;
                                    Move();
                                }
                                else if (clickedObj.tag == "Enemy")
                                {                                    
                                    if (exitHighLightParent != null)
                                    {
                                        Destroy(exitHighLightParent.gameObject);
                                    }

                                    MoveToTarget(clickedObj);
                                    
                                }
                                else if (clickedObj.tag == "Exit")
                                {
                                    if (enemiesHighLightParent != null)
                                    {
                                        Destroy(enemiesHighLightParent.gameObject);
                                    }

                                    MoveToTarget(clickedObj);
                                }                                
                            }
                        }
                    break;                   
                    case (CharacterState.WalkedtoTarget):                        
                        if (Input.GetMouseButtonDown(0))
                        {
                            GameObject clickedObj = ClickSelect();

                            if (clickedObj != null)
                            {
                                if (clickedObj.tag == "CancelAction")
                                {
                                    if (enemiesHighLightParent != null)
                                    {
                                        Destroy(enemiesHighLightParent.gameObject);
                                    }
                                    if (exitHighLightParent != null)
                                    {
                                        Destroy(exitHighLightParent.gameObject);
                                    }
                                    Debug.Log("Canceling action");
                                    HideActionsOptions();
                                    characterState = CharacterState.EndTurn;

                                }
                                else if(clickedObj.tag == "Enemy")
                                {
                                    if (VerifyIfOnRange(meleeAttackRange, clickedObj.transform.position))
                                    {
                                        characterState = CharacterState.WaitingAnimation;                                        
                                        StartCoroutine(Attack(clickedObj));                                        
                                    }
                                } 
                                else if (clickedObj.tag == "Exit")
                                {                                    
                                    if (VerifyIfOnRange(1, clickedObj.transform.position))
                                    {
                                        characterState = CharacterState.WaitingAnimation;
                                        HideActionsOptions();
                                        ExplorationSceneManager.instance.NextLevel();
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

        void MoveToTarget(GameObject target)
        {
            if (occupiedPosList.Contains(target.transform.position))
            {
                path = new List<Tile>();
                path = pathFinding.FindPath(transform.position, target.transform.position);
                characterState = CharacterState.MovingToTarget;
                path.RemoveAt(path.Count - 1);
                Move();
            }
        }

        bool VerifyIfOnRange(int range, Vector2 targetPosition)
        {
            float distance = Mathf.Sqrt(Mathf.Pow((transform.position.x - targetPosition.x),2) + Mathf.Pow((transform.position.y - targetPosition.y),2));            
            return (distance <= range);            
        }

        public override void EndMovement()
        {
            base.EndMovement();
            switch (characterState)            
            {                
                case CharacterState.WaitingAnimation:
                case CharacterState.Moving:
                    characterState = CharacterState.EndTurn;
                    if (exitHighLightParent != null)
                    {
                        Destroy(exitHighLightParent.gameObject);
                    }
                    if (enemiesHighLightParent != null)
                    {
                        Destroy(enemiesHighLightParent.gameObject);
                    }                    
                    break;
                case CharacterState.MovingToTarget:
                    ShowActionsOptions();
                    characterState = CharacterState.WalkedtoTarget;
                    break;

            }      

        }

        public void ShowActionsOptions()
        {            
            cancelActionButton.SetActive(true);
        }

        public void HideActionsOptions()
        {
            cancelActionButton.SetActive(false);
        }

        GameObject ClickSelect()
        {
            //Converting Mouse Pos to 2D (vector2) World Pos
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, layerMask);

            /*if (hit.Length > 0)
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
            }*/

            
            if (hit.Length > 0)
            {
                //Debug.Log(hit[0].collider.gameObject.name);
                return hit[0].collider.gameObject;
            }
            else
            {
                return null;
            }
        }

        public override void HighLightObjectsArroundMe()
        {
            //Vector2 checkPos = new Vector2();
            enemiesHighLightParent = new GameObject(gameObject.name + " EnemiesHightlightParent").transform;
            enemiesHighLightParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);            

            foreach (GameObject go in objectsArroundMe)
            {
                if (go.tag == "Enemy")
                {
                    GameObject instance = Instantiate(enemiesHighLightPB, go.transform.position, Quaternion.identity) as GameObject;
                    instance.transform.SetParent(enemiesHighLightParent.transform);
                }
                if (go.tag == "Exit")
                {
                    exitHighLightParent = new GameObject(gameObject.name + " ExitHightlightParent").transform;
                    exitHighLightParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);

                    GameObject instance = Instantiate(exitHighLightPB, go.transform.position, Quaternion.identity) as GameObject;
                    instance.transform.SetParent(exitHighLightParent.transform);

                }
            }
        }        

        public override IEnumerator Attack(GameObject target)
        {
            HideActionsOptions();
            animator.SetTrigger("PlayerChop");
            yield return new WaitForSeconds(target.GetComponent<ExplorationMovableObject>().TakeDamage(1));
            EndMovement();
        }

        public override float TakeDamage(int damage)
        {
            float animationTime = 1.0f;
            Debug.Log("Taking Damage");
            actualHP -= damage;

            healthBar.value = actualHP;

            animator.SetTrigger("PlayerHit");

            if (actualHP <= 0)
            {
                StartCoroutine(Die());
                return 0;
            }

            return animationTime;
        }    

        public override IEnumerator Die()
        {
            base.Die();
            float animationTime = 1.0f;
            characterState = CharacterState.Dead;
            yield return new WaitForSeconds(animationTime);
            ExplorationSceneManager.instance.GameOver();             
        }
    }
}