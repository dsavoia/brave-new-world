using UnityEngine;
using System.Collections;

namespace BraveNewWorld
{

    public class GameOver : MenuButtons
    {

        // Use this for initialization
        void Start()
        {            
            Destroy(ExplorationSceneManager.instance.gameObject);
        }

    }
}
