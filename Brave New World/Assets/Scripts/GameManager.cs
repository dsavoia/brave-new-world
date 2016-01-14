using UnityEngine;
using System.Collections;

namespace BraveNewWorld
{
    public class GameManager : MonoBehaviour
    {

        public static GameManager instance = null;
        public GameStatesEnum gameState;
        public GameStatesEnum prevGameState;

        // Use this for initialization
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                //TODO: Temporary for exloration development and test
                gameState = GameStatesEnum.Exploration;
                prevGameState = GameStatesEnum.Exploration;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void Update()
        {

            switch (gameState)
            {
                case (GameStatesEnum.Exploration):
                    {
                        if (prevGameState != GameStatesEnum.Exploration)
                        {
                            prevGameState = GameStatesEnum.Exploration;
                            //TODO: Set up Exploration Scene    
                        }

                        break;
                    }
                default:
                    return;

            }
        }

        void ChageState(GameStatesEnum nextState)
        {
            prevGameState = gameState;
            gameState = nextState;
        }


    }
}
