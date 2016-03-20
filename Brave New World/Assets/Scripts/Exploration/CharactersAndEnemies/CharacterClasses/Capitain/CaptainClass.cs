using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class CaptainClass : ExplorationCharacter
    {

        List<ExplorationCharacter> explorationGroup;

        public GameObject explorationGroupHUD;

        new void Awake()
        {
            base.Awake();
            explorationGroup = new List<ExplorationCharacter>();
        }

        public void AddCharacterToGroup(ExplorationCharacter characterScript)
        {
            explorationGroup.Add(characterScript);
        }

        public void RemoveCharacterFromGroup(ExplorationCharacter characterScript)
        {
            explorationGroup.Remove(characterScript);
        }

        public ExplorationCharacter ChooseCharacterFromGroup(int index)
        {
            if(index < explorationGroup.Count)
                return explorationGroup[index];

            return null; 
        }

        public void ShowExplorationGroupHUD()
        {
            explorationGroupHUD.SetActive(true);
        }

        public void HideExplorationGroupHUD()
        {
            explorationGroupHUD.SetActive(false);
        }
    }
}
