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
            else if(explorationGroup.Count > 0)
            {
                ShowExplorationGroupHUD();                
            }
            
            ExplorationSceneManager.instance.SetCameraFocus(transform);

        }

        void StartSelectedCharacterTurn(ExplorationCharacter characterScript)
        {
            HoldTurn();
            characterScript.gameObject.SetActive(true);
            characterScript.transform.position = transform.position;
            ExplorationSceneManager.instance.currentCharacterScript = characterScript;            
            RemoveCharacterFromGroup(characterScript);
            characterScript.BeginTurn();
        }

        public void AddCharacterToGroup(ExplorationCharacter characterScript)
        {
            explorationGroup.Add(characterScript);            
            characterScript.wasInGroup = true;
            characterScript.characterState = CharacterState.WaitingNextTurn;

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
            if (other.tag == "Exit")
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

    }
}