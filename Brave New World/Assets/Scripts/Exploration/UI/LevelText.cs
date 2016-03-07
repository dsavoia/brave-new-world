using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace BraveNewWorld
{
    public class LevelText : MonoBehaviour
    {
        Text levelText;        

        public void UpdateLevel()
        {
            levelText = GetComponent<Text>();
            levelText.text = "Level: " + ExplorationSceneManager.instance.level;
        }
    }
}
