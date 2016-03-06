using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace BraveNewWorld
{
    public class MenuButtons : MonoBehaviour
    {

        public void StartGame()
        {
            SceneManager.LoadScene("ExplorationScene");
        }

        public void MainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}