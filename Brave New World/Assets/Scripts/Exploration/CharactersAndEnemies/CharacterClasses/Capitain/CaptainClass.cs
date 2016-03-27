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
        }

        new void Update()
        {
            base.Update();

            switch (characterState)
            {
                case (CharacterState.ChoosingAction):
                    if (Input.GetMouseButtonDown(0))
                    {
                        int actionLayerMask = LayerMask.NameToLayer("ActionsIcon");
                        actionLayerMask = 1 << actionLayerMask;
                        clickedObj = ClickSelect(actionLayerMask);

                        if (clickedObj != null)
                        {
                            switch (clickedObj.tag)
                            {
                                case ("CharacterIcon"):



                                    break;
                            }
                        }
                    }
                    break;
            }

        }

        public override void BeginTurn()
        {
            if (characterState == CharacterState.WaitingNextTurn || characterState == CharacterState.ChoosingAction)
            {
                characterState = CharacterState.WaitingOrder;                
                PossibleMovement();
            }
            else
            {
                ShowExplorationGroupHUD();
            }

            ExplorationSceneManager.instance.SetCameraFocus(transform);

        }

        //TODO: Continue from here, member not being added properly
        public void AddCharacterToGroup(ExplorationCharacter characterScript)
        {
            explorationGroup.Add(characterScript);           

            for(int i = 0; i < ExplorationGroupPlaceHolder.Count; i++)
            {
                if (!ExplorationGroupPlaceHolder[i].activeInHierarchy)
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

        /*public ExplorationCharacter ChooseCharacterFromGroup(int index)
        {
            if(index < explorationGroup.Count)
                return explorationGroup[index];

            return null; 
        }*/

        public bool CharacterIsOnExplorationGroup(ExplorationCharacter character)
        {
            if (explorationGroup.Contains(character))
                return true;

            return false;
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
            if (characterState == CharacterState.ChoosingAction)
            {
                base.HoldTurn();
            }
        }

    }
}
