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
            OnHold,           
            WaitingAnimation,
            WaitingNextTurn,
            EndTurn,
            Dead
        }

        public int meleeAttackRange = 1;       

        public CharacterState characterState;
        protected CharacterState previousCharacterState;

        public GameObject exitHighLightPB;
        protected Transform exitHighLightParent;

        protected Transform pathHighlightParent;
        public GameObject pathHighlightPB;

        public Sprite cancelActionButton;
        public Sprite movementActionButton;
        public Sprite attackActionButton;
        public Sprite regroupActionButton;

        public Sprite characterIcon;


        public List<GameObject> skillsPlaceHolder;
        public GameObject skillsPlaceHolderParent;
                
        protected GameObject clickedObj;        
        
        protected string lastClickedObjectTag;
        protected Transform lastClickedObjectTransform;
        
        new public void Awake()
        {
            base.Awake();
            //showMyPossibleMovement = true;
            finishedMoving = false;
            possibleMovement = new List<Vector2>();
            characterState = CharacterState.WaitingNextTurn;
            //HoldTurn();
        }

        public void Update()
        {
            if (ExplorationSceneManager.instance.explorationState == ExplorationStateEnum.PlayersTurn)
            {                
                switch (characterState)
                {
                    case (CharacterState.WaitingOrder):
                        if (Input.GetMouseButtonDown(0))
                        {
                            int orderLayerMask = LayerMask.NameToLayer("Clickable");
                            orderLayerMask = 1 << orderLayerMask;
                            clickedObj = ClickSelect(orderLayerMask);

                            if (clickedObj != null)
                            {
                                path = new List<Tile>();
                                path = pathFinding.FindPath(transform.position, clickedObj.transform.position);

                                ExecuteOrder(clickedObj);
                            }
                        }
                        break;
                    case (CharacterState.ChoosingAction):
                        if (Input.GetMouseButtonDown(0))
                        {
                            int actionLayerMask = LayerMask.NameToLayer("ActionsIcon");
                            actionLayerMask = 1 << actionLayerMask;
                            clickedObj = ClickSelect(actionLayerMask);

                            if (clickedObj != null)
                            {
                                ExecuteAction(clickedObj);
                            }
                        }
                        break;
                }
            }
        }

        public virtual void ExecuteOrder(GameObject clickedObj)
        {
            if (clickedObj.tag == "Character")
            {
                if (clickedObj.GetComponent<ExplorationCharacter>().characterState != CharacterState.EndTurn)
                {
                    characterState = CharacterState.WaitingNextTurn;
                    if (this.name != "Captain")
                    {
                        HoldTurn();
                    }
                    clickedObj.GetComponent<ExplorationCharacter>().BeginTurn();
                }
            }
            else if (VerifyIfOnRange(movementRange, path.Count))
            {

                lastClickedObjectTag = clickedObj.tag;
                lastClickedObjectTransform = clickedObj.transform;

                switch (clickedObj.tag)
                {
                    case ("MovableArea"):
                    case ("Exit"):
                        characterState = CharacterState.ChoosingAction;
                        ShowMovementPath();
                        ShowMovementActions();
                        break;
                    case ("Enemy"):
                        if (exitHighLightParent != null)
                        {
                            Destroy(exitHighLightParent.gameObject);
                        }
                        characterState = CharacterState.ChoosingAction;
                        ShowMovementPath();
                        ShowAttackActions();
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void ExecuteAction(GameObject clickedObj)
        {
            switch (clickedObj.tag)
            {
                case ("CancelIcon"):
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
                    break;
                case ("MovementIcon"):
                    if (pathHighlightParent != null)
                    {
                        Destroy(pathHighlightParent.gameObject);
                    }

                    CloseActionsHUD();

                    if (lastClickedObjectTag == "Enemy")
                    {
                        characterState = CharacterState.MovingToTarget;
                        StartCoroutine(MoveToTarget(lastClickedObjectTransform.gameObject));
                    }
                    else
                    {
                        characterState = CharacterState.Moving;
                        Move();
                    }
                    break;
                case ("AttackIcon"):
                    if (pathHighlightParent != null)
                    {
                        Destroy(pathHighlightParent.gameObject);
                    }

                    CloseActionsHUD();

                    StartCoroutine(MoveToTargetAndAttack(lastClickedObjectTransform.gameObject));
                    break;
                case ("RegroupIcon"):
                    lastClickedObjectTransform.gameObject.GetComponent<CaptainClass>().AddCharacterToGroup(this);
                    break;
                default:
                    break;
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

        public virtual void BeginTurn()
        {
            characterState = CharacterState.WaitingOrder;
            ExplorationSceneManager.instance.SetCameraFocus(transform);
            //ExplorationSceneManager.instance.currentCharacterScript = GetComponent<ExplorationCharacter>();
            PossibleMovement();
        }

        public virtual void HoldTurn()
        {
            characterState = CharacterState.WaitingNextTurn;
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
            if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }
        }

        //TODO: This, probably, should be in the Captain
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


        //TODO: MAKE BETTER ACTION ICONS PLACEMENT!!
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

            skillsPlaceHolder[skillsPlaceHolder.Count-1].GetComponent<Image>().sprite = cancelActionButton;
            skillsPlaceHolder[skillsPlaceHolder.Count - 1].tag = "CancelIcon";
            skillsPlaceHolder[skillsPlaceHolder.Count - 1].SetActive(true);

        }
        
        public virtual void ShowAttackActions()
        {
            ShowMovementActions();

            skillsPlaceHolder[1].GetComponent<Image>().sprite = attackActionButton;
            skillsPlaceHolder[1].tag = "AttackIcon";
            skillsPlaceHolder[1].SetActive(true);
        }

        public virtual void ShowRegroupAction()
        {
            ShowMovementActions();

            skillsPlaceHolder[1].GetComponent<Image>().sprite = regroupActionButton;
            skillsPlaceHolder[1].tag = "RegroupIcon";
            skillsPlaceHolder[1].SetActive(true);
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

        public IEnumerator MoveToTarget(GameObject target)
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

        public IEnumerator MoveToTargetAndAttack(GameObject target)
        {
            StartCoroutine(MoveToTarget(target));

            while (characterState == CharacterState.MovingToTarget)
            {
                yield return null;
            }

            if (VerifyIfOnRange(meleeAttackRange, path.Count))
            {
                characterState = CharacterState.WaitingAnimation;
                StartCoroutine(Attack(lastClickedObjectTransform.gameObject));
            }
        }        

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

        protected GameObject ClickSelect(LayerMask layerMask)
        {
            //Converting Mouse Pos to 2D (vector2) World Pos
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, layerMask);
                                   
            if (hit.Length > 0)
            {
                //Debug.Log(hit[0].collider.gameObject.name);
                //return hit[0].collider.gameObject;

                //GAMBS, thanks Unity
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].collider.tag == "Character")
                        return hit[i].collider.gameObject;
                }

                return hit[hit.Length-1].collider.gameObject;
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
            //Debug.Log("Taking Damage");
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