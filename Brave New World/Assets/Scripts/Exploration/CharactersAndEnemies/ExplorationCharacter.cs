using UnityEngine;
using UnityEngine.UI;
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
            ChoosingAction,
            Moving,
            MovingToTarget,
            MovedToTarget,
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

        protected Transform pathHighlightParent;
        public GameObject pathHighlightPB;

        public Sprite cancelActionButton;
        public Sprite movementActionButton;
        public Sprite attackActionButton;


        public List<GameObject> skillsPlaceHolder;
        public GameObject skillsPlaceHolderParent;

        string lastClickedObjectTag;
        Transform lastClickedObjectPosition;

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
                switch (characterState)
                {
                    case (CharacterState.WaitingOrder):
                        if (Input.GetMouseButtonDown(0))
                        {
                            GameObject clickedObj = ClickSelect();

                            if (clickedObj != null)
                            {
                                path = new List<Tile>();
                                path = pathFinding.FindPath(transform.position, clickedObj.transform.position);

                                if (VerifyIfOnRange(movementRange, path.Count)) {
                                    lastClickedObjectTag = clickedObj.tag;
                                    lastClickedObjectPosition = clickedObj.transform;
                                    
                                    if (clickedObj.tag == "MovableArea" || clickedObj.tag == "Exit")
                                    {
                                        characterState = CharacterState.ChoosingAction;                                       
                                        ShowMovementPath();
                                        ShowMovementActions();
                                    }
                                    else if (clickedObj.tag == "Enemy")
                                    {
                                        if (exitHighLightParent != null)
                                        {
                                            Destroy(exitHighLightParent.gameObject);
                                        }
                                        characterState = CharacterState.ChoosingAction;                                       
                                        ShowMovementPath();
                                        ShowAttackActions();
                                    }
                                }
                            }
                        }
                        break;
                    case (CharacterState.ChoosingAction):
                        if (Input.GetMouseButtonDown(0))
                        {
                            GameObject clickedObj = ClickSelect();

                            if (clickedObj != null)
                            {
                                if (clickedObj.tag == "CancelIcon")
                                {
                                    if (enemiesHighLightParent != null)
                                    {
                                        Destroy(enemiesHighLightParent.gameObject);
                                    }
                                    if (exitHighLightParent != null)
                                    {
                                        Destroy(exitHighLightParent.gameObject);
                                    }
                                    if (pathHighlightParent != null)
                                    {
                                        Destroy(pathHighlightParent.gameObject);
                                    }

                                    CloseActionsHUD();
                                    BeginTurn();
                                }
                                else if (clickedObj.tag == "MovementIcon")
                                {

                                    if (pathHighlightParent != null)
                                    {
                                        Destroy(pathHighlightParent.gameObject);
                                    }

                                    CloseActionsHUD();

                                    if (lastClickedObjectTag == "Enemy")
                                    {
                                        characterState = CharacterState.MovingToTarget;
                                        StartCoroutine(MoveToTarget(lastClickedObjectPosition.gameObject));
                                    }
                                    else
                                    {
                                        characterState = CharacterState.Moving;
                                        Move();
                                    }
                                }
                                else if (clickedObj.tag == "AttackIcon")
                                {
                                    if (pathHighlightParent != null)
                                    {
                                        Destroy(pathHighlightParent.gameObject);
                                    }

                                    CloseActionsHUD();

                                    StartCoroutine(MoveToTargetAndAttack(lastClickedObjectPosition.gameObject));
                                }
                                /*else if (clickedObj.tag == "Exit")
                                {                                    
                                    if (VerifyIfOnRange(1, clickedObj.transform.position))
                                    {
                                        characterState = CharacterState.WaitingAnimation;
                                        //HideActionsOptions();
                                        ExplorationSceneManager.instance.NextLevel();
                                    }
                                }*/

                            }
                        }
                        break;
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Exit")
            {
                characterState = CharacterState.WaitingAnimation;
                StartCoroutine(WaitAndExit());
            }
        }

        public void BeginTurn()
        {
            characterState = CharacterState.WaitingOrder;
            PossibleMovement();
        }

        IEnumerator WaitAndExit()
        {
            while (characterState == CharacterState.WaitingAnimation)
            {
                yield return null;
            }

            ExplorationSceneManager.instance.NextLevel();
        }

        public void ShowMovementPath()
        {

            pathHighlightParent = new GameObject(gameObject.name + " PathHighlightParent").transform;
            pathHighlightParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);

            GameObject pathTile;

            pathTile = Instantiate(pathHighlightPB, transform.position, Quaternion.identity) as GameObject;
            pathTile.transform.parent = pathHighlightParent;

            foreach (Tile p in path)
            {
                pathTile = Instantiate(pathHighlightPB, new Vector3(p.position.x, p.position.y, 0), Quaternion.identity) as GameObject;
                pathTile.transform.parent = pathHighlightParent;
            }
        }

        public virtual void ShowMovementActions()
        {
            if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }

            OpenActionsHUD();

            skillsPlaceHolder[0].GetComponent<Image>().sprite = movementActionButton;
            skillsPlaceHolder[0].tag = "MovementIcon";
            skillsPlaceHolder[0].SetActive(true);

            skillsPlaceHolder[1].GetComponent<Image>().sprite = cancelActionButton;
            skillsPlaceHolder[1].tag = "CancelIcon";
            skillsPlaceHolder[1].SetActive(true);

        }

        public virtual void ShowAttackActions()
        {
            if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }

            OpenActionsHUD();

            skillsPlaceHolder[0].GetComponent<Image>().sprite = attackActionButton;
            skillsPlaceHolder[0].tag = "AttackIcon";
            skillsPlaceHolder[0].SetActive(true);

            skillsPlaceHolder[1].GetComponent<Image>().sprite = movementActionButton;
            skillsPlaceHolder[1].tag = "MovementIcon";
            skillsPlaceHolder[1].SetActive(true);

            skillsPlaceHolder[2].GetComponent<Image>().sprite = cancelActionButton;
            skillsPlaceHolder[2].tag = "CancelIcon";
            skillsPlaceHolder[2].SetActive(true);

        }

        public void CleanActionsHUD()
        {
            for (int i = 0; i < skillsPlaceHolder.Count; i++)
            {
                skillsPlaceHolder[i].SetActive(false);
            }
        }

        public void OpenActionsHUD()
        {
            skillsPlaceHolderParent.SetActive(true);
            CleanActionsHUD();
        }

        public void CloseActionsHUD()
        {
            skillsPlaceHolderParent.SetActive(false);
        }

        IEnumerator MoveToTarget(GameObject target)
        {
            if (occupiedPosList.Contains(target.transform.position))
            {
                path = new List<Tile>();
                path = pathFinding.FindPath(transform.position, target.transform.position);
                characterState = CharacterState.MovingToTarget;
                path.RemoveAt(path.Count - 1);
                Move();
            }

            while (characterState == CharacterState.MovingToTarget)
            {
                yield return null;
            }

            EndMovement();

        }
        
        IEnumerator MoveToTargetAndAttack(GameObject target)
        {
            StartCoroutine(MoveToTarget(target));

            while (characterState == CharacterState.MovingToTarget)
            {
                yield return null;
            }

            if (VerifyIfOnRange(meleeAttackRange, path.Count))
            {
                characterState = CharacterState.WaitingAnimation;
                StartCoroutine(Attack(lastClickedObjectPosition.gameObject));
            }
        }

        /*
        bool VerifyIfOnRange(int range, Vector2 targetPosition)
        {
            float distance = Mathf.Sqrt(Mathf.Pow((targetPosition.x - transform.position.x),2) + Mathf.Pow((targetPosition.y - transform.position.y),2));            
            return (distance <= range);            
        }*/

        bool VerifyIfOnRange(int range, int pathCount)
        {            
            return (pathCount <= range);
        }

        public override void EndMovement()
        {
            base.EndMovement();
            switch (characterState)            
            {                
                case CharacterState.WaitingAnimation:
                case CharacterState.MovedToTarget:
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
                    characterState = CharacterState.MovedToTarget;
                    break;
            }      

        }       

        GameObject ClickSelect()
        {
            //Converting Mouse Pos to 2D (vector2) World Pos
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, layerMask);
                                   
            if (hit.Length > 0)
            {
                Debug.Log(hit[0].collider.gameObject.name);
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
            //HideCancelOption();
            animator.SetTrigger("PlayerChop");
            yield return new WaitForSeconds(target.GetComponent<ExplorationMovableObject>().TakeDamage(1));
            characterState = CharacterState.Moving;
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