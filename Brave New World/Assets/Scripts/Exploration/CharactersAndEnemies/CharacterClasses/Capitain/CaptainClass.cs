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
            characterState = CharacterState.WaitingNextTurn;
        }

        public override void ExecuteAction(GameObject clickedObj)
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
                    if (movementParent != null)
                    {
                        Destroy(movementParent.gameObject);
                    }

                    HideExplorationGroupHUD();
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
                case ("CharacterIcon"):
                    HideExplorationGroupHUD();
                    StartSelectedCharacterTurn(clickedObj.GetComponent<ExplorationGroupCharacterHolder>().character);                    
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
                PossibleMovement();
            }
            else
            {
                ShowExplorationGroupHUD();
                characterState = CharacterState.ChoosingAction;
            }

            //ExplorationSceneManager.instance.currentCharacterScript = this;
            ExplorationSceneManager.instance.SetCameraFocus(transform);

        }

        void StartSelectedCharacterTurn(ExplorationCharacter characterScript)
        {
            characterScript.gameObject.SetActive(true);
            characterScript.transform.position = transform.position;
            ExplorationSceneManager.instance.currentCharacterScript = characterScript;
            HoldTurn();
            RemoveCharacterFromGroup(characterScript);
            characterScript.BeginTurn();
        }

        public void AddCharacterToGroup(ExplorationCharacter characterScript)
        {
            explorationGroup.Add(characterScript);
            characterScript.gameObject.SetActive(false);
            characterScript.characterState = CharacterState.WaitingNextTurn;
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


            /*for (int i = 0; i < explorationGroup.Count; i++)
            {
                if (explorationGroup[i] == character)
                {
                    return true;
                }
            }*/

            return explorationGroup.Contains(character);

            
        }

        public void ShowExplorationGroupHUD()
        {
            if (explorationGroup.Count == 0)
            {
                return;
            }

            explorationGroupHUD.SetActive(true);                        
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

            characterState = CharacterState.OnHold;
        }

    }
}
