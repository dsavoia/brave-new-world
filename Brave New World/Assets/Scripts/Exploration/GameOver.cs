using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace BraveNewWorld
{
    public class GameOver : MenuButtons
    {

        public Text levelText; 

        // Use this for initialization
        void Awake()
        {               
            levelText.text = "On Level " + ExplorationSceneManager.instance.level;
            Destroy(ExplorationSceneManager.instance.gameObject);
        }

    }
}
