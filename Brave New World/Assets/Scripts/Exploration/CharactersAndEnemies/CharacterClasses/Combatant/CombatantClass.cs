using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace BraveNewWorld
{

    public class CombatantClass : ExplorationCharacter
    {
        public GameObject RangedAttackHighlightPB;
        public GameObject projectilePB;
        protected Transform rangedAttackParent;
        public int rangedAttackRange = 1;
        public int ignoredRangedAttackRange = 1;
        public float projectileSpeed = 1;
        public int rangedAttackDamage = 1;
        public Sprite rangedAttackActionButton;


        // Use this for initialization
        void Start()
        {

        }

        public new void Update()
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
                                if (lastClickedObjectTag == "RangedAttackIcon")
                                {
                                    ExecuteRangedAttackAction();
                                }
                                else
                                {
                                    path = new List<Tile>();
                                    path = pathFinding.FindPath(transform.position, clickedObj.transform.position);

                                    ExecuteOrder(clickedObj);
                                }
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
                    /*case (CharacterState.SelectingRangedTarget):
                        if (Input.GetMouseButtonDown(0))
                        {
                            int orderLayerMask = LayerMask.NameToLayer("Clickable");
                            orderLayerMask = 1 << orderLayerMask;

                            int actionLayerMask = LayerMask.NameToLayer("ActionsIcon");
                            actionLayerMask = 1 << actionLayerMask;

                            if (clickedObj != null)
                            {
                                ExecuteAction(clickedObj);
                            }
                        }
                        break;*/
                }
            }
        }

        //TODO: Temporary, only while Combatant has a different color. For the next 2 methods
        public override void SetCharacterStateToWaitingNextTurn()
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
            characterState = CharacterState.WaitingNextTurn;
        }
        public override void SetCharacterToEndTurnColor()
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.4f);
        }

        public override void ExecuteAction(GameObject clickedObj)
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
                case ("RangedAttackIcon"):
                    DestroyHighLights();
                    CloseActionsHUD();
                    PossibleActionRange(RangedAttackHighlightPB, rangedAttackParent, rangedAttackRange, ignoredRangedAttackRange, "RangedAttack");
                    ShowRangedCancelAction();
                    lastClickedObjectTag = "RangedAttackIcon";
                    characterState = CharacterState.WaitingOrder;
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

        public override void ExecuteOrder(GameObject clickedObj)
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
                    else if (clickedCharacter.characterState != CharacterState.EndTurn)
                    {
                        HoldTurn();
                        ExplorationSceneManager.instance.SetCurrentCharacterScript(clickedObj.GetComponent<ExplorationCharacter>());
                        clickedObj.GetComponent<ExplorationCharacter>().BeginTurn();
                    }

                }
                else if (clickedCharacter == this && ExplorationSceneManager.instance.previousCharacterScript == this)
                {
                    ShowRangedAttackActions();
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

        public void ExecuteRangedAttackAction()
        {
            switch (clickedObj.tag)
            {               
                case ("Enemy"):                   

                    path = new List<Tile>();
                    path = pathFinding.FindPath(transform.position, clickedObj.transform.position);

                    if (path.Count > ignoredRangedAttackRange)
                    {
                        if (VerifyIfOnRange(rangedAttackRange, path.Count))
                        {
                            DestroyHighLights();
                            CloseActionsHUD();
                            lastClickedObjectTag = "";
                            StartCoroutine(RangedAttackAnimation(clickedObj));
                        }
                    }
                    break;
                case ("CancelIcon"):
                    DestroyHighLights();
                    CloseActionsHUD();
                    lastClickedObjectTag = "";
                    BeginTurn();
                    break;
                default:
                    break;
            }
        }


        public IEnumerator RangedAttackAnimation(GameObject target)
        {        
            characterState = CharacterState.WaitingAnimation;

            GameObject projectile = Instantiate(projectilePB, transform.position, Quaternion.identity) as GameObject;

            ExplorationSceneManager.instance.SetCameraFocus(projectile.transform);

            projectile.transform.DOMove(target.transform.position, projectileSpeed).OnComplete(() => {                
                target.GetComponent<ExplorationCreature>().TakeDamage(rangedAttackDamage);
                characterState = CharacterState.MovedToTarget;
            });

            while (characterState == CharacterState.WaitingAnimation)
            {
                yield return null;
            }

            Destroy(projectile.gameObject);
            EndMovement();
        }


        public void ShowRangedAttackActions()
        {
            OpenActionsHUD();

            skillsPlaceHolder[1].GetComponent<Image>().sprite = rangedAttackActionButton;
            skillsPlaceHolder[1].tag = "RangedAttackIcon";
            skillsPlaceHolder[1].SetActive(true);

            AddCancelOption();
            characterState = CharacterState.ChoosingAction;
        }

        public void ShowRangedCancelAction()
        {
            OpenActionsHUD();
            AddCancelOption();
        }

    }
}
