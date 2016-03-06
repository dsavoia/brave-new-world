using UnityEngine;
using System.Collections;

namespace BraveNewWorld
{
    public class ExplorationPauseMenuButtons : MonoBehaviour
    {

        public void ResumeGame()
        {
            ExplorationSceneManager.instance.ResumeGame();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
