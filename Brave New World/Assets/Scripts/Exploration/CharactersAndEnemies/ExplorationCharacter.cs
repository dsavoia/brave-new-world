using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DG.Tweening;

namespace BraveNewWorld
{
    public class ExplorationCharacter : ExplorationMovableObject
    {

        //TODO: Make it a separate enum file
        public enum CharacterState
        {
            WaitingOrder,
            ChoosingAction,
            Moving,
            MovingToTarget,
            MovedToTarget, 
            OnHold,
            SelectingRangedTarget,
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

        public GameObject friendsHighLightPB;
        protected Transform friendsHighlightParent;

        protected Transform pathHighlightParent;
        public GameObject pathHighlightPB;

        public Sprite cancelActionButton;
        public Sprite movementActionButton;
        public Sprite attackActionButton;
        public Sprite regroupActionButton;

        public Sprite characterIcon;


        public List<GameObject> skillsPlaceHolder;
        public GameObject skillsPlaceHolderParent;
        public bool wasInGroup;

        protected GameObject clickedObj;
        


        protected string lastClickedObjectTag;
        protected Transform lastClickedObjectTransform;
        
        new public void Awake()
        {
            base.Awake();
            //showMyPossibleMovement = true;
            finishedMoving = false;
            wasInGroup = false;
            possibleActionRange = new List<Vector2>();
            SetCharacterStateToWaitingNextTurn();
            //characterState = CharacterState.WaitingNextTurn;            
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

                            int actionLayerMask = LayerMask.NameToLayer("ActionsIcon");
                            actionLayerMask = 1 << actionLayerMask;

                            orderLayerMask = orderLayerMask | actionLayerMask;

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
                ExplorationCharacter clickedCharacter = clickedObj.GetComponent<ExplorationCharacter>();

                if (clickedCharacter.name == "Captain")
                {
                    if (VerifyIfOnRange(movementRange, path.Count))
                    {
                        ShowMovementPath();
                        lastClickedObjectTag = clickedObj.tag;
                        lastClickedObjectTransform = clickedObj.transform;
                        ShowRegroupAction();
                        characterState = CharacterState.ChoosingAction;
                    }
                    else if(clickedCharacter.characterState != CharacterState.EndTurn)
                    {
                        HoldTurn();
                        ExplorationSceneManager.instance.SetCurrentCharacterScript(clickedObj.GetComponent<ExplorationCharacter>());
                        clickedObj.GetComponent<ExplorationCharacter>().BeginTurn();
                    }
                    
                }
                else if (clickedCharacter.characterState != CharacterState.EndTurn)
                {
                    HoldTurn();
                    ExplorationSceneManager.instance.SetCurrentCharacterScript(clickedObj.GetComponent<ExplorationCharacter>());
                    clickedObj.GetComponent<ExplorationCharacter>().BeginTurn();
                }
            }
            else if (clickedObj.tag == "CancelIcon")
            {
                DestroyHighLights();
                CloseActionsHUD();
                ExplorationSceneManager.instance.captainScript.AddCharacterToGroup(this);
                ExplorationSceneManager.instance.SetCurrentCharacterScript(ExplorationSceneManager.instance.captainScript);
                ExplorationSceneManager.instance.captainScript.BeginTurn();
            }
            else if (VerifyIfOnRange(movementRange, path.Count))
            {
                lastClickedObjectTag = clickedObj.tag;
                lastClickedObjectTransform = clickedObj.transform;

                switch (clickedObj.tag)
                {
                    case ("MovableArea"):
                        characterState = CharacterState.ChoosingAction;
                        ShowMovementPath();
                        ShowMovementActions();
                        break;
                    case ("Exit"):                        

                        //TODO: Show Feedback Message "I can't leave without my Captain"
                        //MAYBE: Put reference to "The Dead Poets Society" movie

                        break;
                    case ("Enemy"):                        
                        DestroyHighLights();
                        characterState = CharacterState.ChoosingAction;
                        ShowMovementPath();
                        ShowAttackActions();
                        break;
                    case ("CancelIcon"):                        
                        DestroyHighLights();
                        CloseActionsHUD();
                        ExplorationSceneManager.instance.captainScript.AddCharacterToGroup(this);
                        ExplorationSceneManager.instance.SetCurrentCharacterScript(ExplorationSceneManager.instance.captainScript);
                        ExplorationSceneManager.instance.captainScript.BeginTurn();
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void ExecuteAction(GameObject clickedObj)
        {

            CaptainClass captain = null;

            if (lastClickedObjectTransform != null)
            {
                captain = lastClickedObjectTransform.gameObject.GetComponent<CaptainClass>();
            }

            switch (clickedObj.tag)
            {
                case ("CancelIcon"):
                        DestroyHighLights();
                        CloseActionsHUD();
                        BeginTurn();
                    break;

                case ("MovementIcon"):
                    
                    DestroyHighLights();

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
                    DestroyHighLights();
                    CloseActionsHUD();
                    StartCoroutine(MoveToTargetAndAttack(lastClickedObjectTransform.gameObject));
                    break;
                case ("RegroupIcon"):
                    CloseActionsHUD();
                    StartCoroutine(MovingToCaptain(captain));
                    break;
                case ("CharacterIcon"):
                    if (captain.characterState != CharacterState.EndTurn)
                    {
                        DestroyHighLights();
                        CloseActionsHUD();
                        HoldTurn();
                        ExplorationSceneManager.instance.SetCurrentCharacterScript(captain);
                        captain.BeginTurn();
                    }
                    break;
                default:
                    break;
            }
        }        

        public virtual void BeginTurn()
        {
            characterState = CharacterState.WaitingOrder;
            ExplorationSceneManager.instance.SetCameraFocus(transform);            
            DestroyHighLights();
            PossibleActionRange(movementHighlightPB, movementParent, movementRange, 0, "Movement");            

            if (wasInGroup)
            {
                ShowTurnCancelOption();                
            }

        }

        public virtual void SetCharacterStateToWaitingNextTurn()
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            characterState = CharacterState.WaitingNextTurn;
        }

        //Gives the option of canceling the group exit
        public void ShowTurnCancelOption()
        {
            OpenActionsHUD();
            AddCancelOption();            
        }

        public void DestroyHighLights()
        {

            if (actionRangeParent != null)
            {
                Destroy(actionRangeParent.gameObject);
            }
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
            if (friendsHighlightParent != null)
            {
                Destroy(friendsHighlightParent.gameObject);
            }
            /*if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }*/
        }

        public virtual void HoldTurn()
        {
            //characterState = CharacterState.WaitingNextTurn;
            SetCharacterStateToWaitingNextTurn();
            DestroyHighLights();
        }

        protected IEnumerator MovingToCaptain(CaptainClass captain)
        {
            
            path = new List<Tile>();
            path = pathFinding.FindPath(transform.position, captain.transform.position);
            characterState = CharacterState.MovingToTarget;            
            Move();            

            while (characterState == CharacterState.MovingToTarget)
            {
                yield return null;
            }           
                        
            DestroyHighLights();
            captain.AddCharacterToGroup(this);
            EndMovement();            
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

            if (!(path.Count == 1 && lastClickedObjectTag == "Enemy"))
            {
                skillsPlaceHolder[0].GetComponent<Image>().sprite = movementActionButton;
                skillsPlaceHolder[0].GetComponent<Image>().color = Color.white;
                skillsPlaceHolder[0].tag = "MovementIcon";
                skillsPlaceHolder[0].SetActive(true);
            }

            AddCancelOption();
        }
        
        protected void AddCancelOption()
        {
            skillsPlaceHolder[skillsPlaceHolder.Count - 1].GetComponent<Image>().sprite = cancelActionButton;
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
            if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }

            OpenActionsHUD();

            skillsPlaceHolder[0].GetComponent<Image>().sprite = ExplorationSceneManager.instance.captainScript.characterIcon;

            if (ExplorationSceneManager.instance.captainScript.characterState == CharacterState.EndTurn)
            {
                skillsPlaceHolder[0].GetComponent<Image>().color = Color.gray;
            }
            else
            {
                skillsPlaceHolder[0].GetComponent<Image>().color = Color.white;
            }
                        
            skillsPlaceHolder[0].tag = "CharacterIcon";
            skillsPlaceHolder[0].SetActive(true);

            skillsPlaceHolder[1].GetComponent<Image>().sprite = regroupActionButton;
            skillsPlaceHolder[1].tag = "RegroupIcon";
            skillsPlaceHolder[1].SetActive(true);

            AddCancelOption();
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

        protected bool VerifyIfOnRange(int range, int pathCount)
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
                    SetCharacterToEndTurnColor();                    
                    wasInGroup = false;                 
                    break;
                case CharacterState.MovingToTarget:
                    characterState = CharacterState.MovedToTarget;
                    break;
            }      

        }    
        
        public virtual void SetCharacterToEndTurnColor()
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
        }

        protected GameObject ClickSelect(LayerMask layerMask)
        {            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hit = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, layerMask);
                                   
            if (hit.Length > 0)
            {
                //GAMBS, thanks Unity
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].collider.tag == "Character")
                        return hit[i].collider.gameObject;
                }

                //MOAR GAMBS, thanks Unity
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].collider.tag == "CancelIcon")
                        return hit[i].collider.gameObject;
                }

                return hit[hit.Length-1].collider.gameObject;
            }
            else
            {
                return null;
            }
        }

        public override void HighLightObjectsArround()
        {            
            enemiesHighLightParent = new GameObject(gameObject.name + " EnemiesHightlightParent").transform;
            enemiesHighLightParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);

            friendsHighlightParent = new GameObject(gameObject.name + " FriendsHightlightParent").transform;
            friendsHighlightParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);

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
                if (go.tag == "Character")
                {
                    GameObject instance = Instantiate(friendsHighLightPB, go.transform.position, Quaternion.identity) as GameObject;
                    instance.transform.SetParent(friendsHighlightParent.transform);
                }

            }
        }        

        public override IEnumerator Attack(GameObject target)
        {            
            animator.SetTrigger("PlayerChop");
            yield return new WaitForSeconds(target.GetComponent<ExplorationMovableObject>().TakeDamage(1));
            characterState = CharacterState.Moving;
            EndMovement();
        }

        public override float TakeDamage(int damage)
        {
            float animationTime = 1.0f;         
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
            gameObject.SetActive(false);
            ExplorationSceneManager.instance.explorationGroup.Remove(gameObject);            
        }
    }
}