using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class CaptainClass : ExplorationCharacter
    {

        public List<ExplorationCharacter> explorationGroup;
        public List<GameObject> ExplorationGroupPlaceHolder;

        public GameObject explorationGroupHUD;

        new void Awake()
        {
            base.Awake();
            explorationGroup = new List<ExplorationCharacter>();
            SetCharacterStateToWaitingNextTurn();
            //characterState = CharacterState.WaitingNextTurn;
        }

        public override void ExecuteOrder(GameObject clickedObj)
        {
            if (clickedObj.tag == "Character")
            {
                ExplorationCharacter clickedCharacter = clickedObj.GetComponent<ExplorationCharacter>();

                if (clickedCharacter.name == "Captain")
                {
                    if (explorationGroup.Count > 0)
                    {
                        ShowExplorationGroupHUD();
                    }
                }
                else if (clickedCharacter.characterState != CharacterState.EndTurn)
                {
                    HoldTurn();
                    ExplorationSceneManager.instance.SetCurrentCharacterScript(clickedObj.GetComponent<ExplorationCharacter>());
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
                        characterState = CharacterState.ChoosingAction;
                        ShowMovementPath();
                        ShowMovementActions();
                        break;
                    case ("Exit"):                        
                        if (explorationGroup.Count == ExplorationSceneManager.instance.explorationGroup.Count)
                        {
                            characterState = CharacterState.ChoosingAction;
                            ShowMovementPath();
                            ShowMovementActions();
                        }
                        else
                        {
                            //TODO: Show feedback message "I can't leave without my group"
                        }                        
                        break;
                    case ("Enemy"):
                        DestroyHighLights();
                        characterState = CharacterState.ChoosingAction;
                        ShowMovementPath();
                        ShowAttackActions();
                        break;                    
                    default:
                        break;
                }
            }
        }

        public override void ExecuteAction(GameObject clickedObj)
        {
            switch (clickedObj.tag)
            {
                case ("CancelIcon"):
                    DestroyHighLights();
                    HideExplorationGroupHUD();
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
                case ("CharacterIcon"):                    
                    if (clickedObj.GetComponent<ExplorationGroupCharacterHolder>().character.characterState != CharacterState.EndTurn)
                    {
                        HideExplorationGroupHUD();
                        StartSelectedCharacterTurn(clickedObj.GetComponent<ExplorationGroupCharacterHolder>().character);                        
                    }
                    break;                
                default:
                    break;
            }
        }

        public override void BeginTurn()
        {
            if (characterState == CharacterState.WaitingNextTurn || characterState == CharacterState.OnHold || characterState == CharacterState.ChoosingAction)
            {
                characterState = CharacterState.WaitingOrder;                
                PossibleActionRange(movementHighlightPB, movementParent, movementRange, 0, "Movement");
            }
            else if(explorationGroup.Count > 0)
            {
                ShowExplorationGroupHUD();                
            }
            
            ExplorationSceneManager.instance.SetCameraFocus(transform);

        }

        //TODO: Temporary, only while Captain has a different color. For the next 2 methods
        public override void SetCharacterStateToWaitingNextTurn()
        {
            GetComponent<SpriteRenderer>().color = new Color(0, 1, 1, 1);
            characterState = CharacterState.WaitingNextTurn;
        }
        public override void SetCharacterToEndTurnColor()
        {
            GetComponent<SpriteRenderer>().color = new Color(0, 1, 1, 0.4f);
        }

        void StartSelectedCharacterTurn(ExplorationCharacter characterScript)
        {
            HoldTurn();
            characterScript.gameObject.SetActive(true);
            characterScript.transform.position = transform.position;
            ExplorationSceneManager.instance.SetCurrentCharacterScript(characterScript);
            RemoveCharacterFromGroup(characterScript);
            characterScript.BeginTurn();
        }

        public void AddCharacterToGroup(ExplorationCharacter characterScript)
        {
            explorationGroup.Add(characterScript);            
            characterScript.wasInGroup = true;            

            ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)characterScript.transform.position.x, (int)characterScript.transform.position.y].isOccupied = false;
            ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)characterScript.transform.position.x, (int)characterScript.transform.position.y].OccupyingObject = null;
            
            characterScript.gameObject.SetActive(false);

            for (int i = 0; i < ExplorationGroupPlaceHolder.Count; i++)
            {
                if (!ExplorationGroupPlaceHolder[i].activeSelf)
                {
                    ExplorationGroupCharacterHolder characterHolder = ExplorationGroupPlaceHolder[i].GetComponent<ExplorationGroupCharacterHolder>();
                    characterHolder.character = characterScript;
                    ExplorationGroupPlaceHolder[i].GetComponent<Image>().sprite = characterScript.characterIcon;
                    ExplorationGroupPlaceHolder[i].SetActive(true);
                    break;
                }
            }            
        }

        public void RemoveCharacterFromGroup(ExplorationCharacter characterScript)
        {
            explorationGroup.Remove(characterScript);            

            for (int i = 0; i < ExplorationGroupPlaceHolder.Count; i++)
            {
                if (ExplorationGroupPlaceHolder[i].GetComponent<ExplorationGroupCharacterHolder>().character == characterScript)
                {                    
                    ExplorationGroupPlaceHolder[i].SetActive(false);
                    break;
                }
            }
        }       

        public bool CharacterIsOnExplorationGroup(ExplorationCharacter character)
        {
            return explorationGroup.Contains(character);            
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Exit" && (explorationGroup.Count == ExplorationSceneManager.instance.explorationGroup.Count))
            {
                characterState = CharacterState.WaitingAnimation;
                StartCoroutine(WaitAndExit());
            }
        }        

        public void ShowExplorationGroupHUD()
        {
            if (explorationGroup.Count == 0)
            {
                return;
            }

            explorationGroupHUD.SetActive(true);

            for (int i = 0; i < ExplorationGroupPlaceHolder.Count; i++)
            {
                if (ExplorationGroupPlaceHolder[i].activeInHierarchy)
                {
                    ExplorationCharacter groupCharacter = ExplorationGroupPlaceHolder[i].GetComponent<ExplorationGroupCharacterHolder>().character;                                     

                    if (groupCharacter.characterState == CharacterState.EndTurn)
                    {
                        ExplorationGroupPlaceHolder[i].GetComponent<Image>().color = Color.gray;
                    }
                    else
                    {
                        ExplorationGroupPlaceHolder[i].GetComponent<Image>().color = Color.white;
                    }

                }
            }

            
            characterState = CharacterState.ChoosingAction;
        }

        public void HideExplorationGroupHUD()
        {
            explorationGroupHUD.SetActive(false);
        }

        public void CleanExplorationGroupHUD()
        {
            for (int i = 0; i < ExplorationGroupPlaceHolder.Count; i++)
            {
                ExplorationGroupPlaceHolder[i].SetActive(false);
            }
        }

        public override void HoldTurn()
        {
            DestroyHighLights();
            characterState = CharacterState.OnHold;
        }
        
        IEnumerator WaitAndExit()
        {
            while (characterState == CharacterState.WaitingAnimation)
            {
                yield return null;
            }
            ExplorationSceneManager.instance.NextLevel();
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