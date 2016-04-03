using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace BraveNewWorld
{
    public class ExplorationSceneManager : MonoBehaviour
    {

        private static ExplorationSceneManager _instance;
        private List<ExplorationCreature> enemiesList;        
        private bool enemiesMoving;
        private Transform enemiesParent;
        private Transform exitParent;
        private GameObject captainCharacter;
        private int enemiesQuantity;

        [HideInInspector] public ExplorationCharacter currentCharacterScript;
        [HideInInspector] public ExplorationStateEnum explorationState;
        [HideInInspector] public ExplorationStateEnum previousExplorationState;
        [HideInInspector] public DungeonManager dungeonManager;
        [HideInInspector] public int hours;
        [HideInInspector] public PassOneHour passOneHour;        
        
        public int initialMapWidth;
        public int initialMapHeigth;
        public int removeLoneWallIterations;
        public int wallLayersQty;
        public int initialEnemiesQty;
        public CaptainClass captainScript;

        public int level;        
        public GameObject addHourText;
        public GameObject levelText;
        public GameObject enemiesTurnText;
        public GameObject[] enemyPrefab;        
        public GameObject captainPrefab;        
        public GameObject pauseMenu;
        public List<GameObject> explorationGroup;


        public static ExplorationSceneManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<ExplorationSceneManager>();

                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }
        
        void Awake()
        {

            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                if (this != _instance)
                    Destroy(gameObject);
            }

            level = 1;
            hours = 8;
            dungeonManager = GetComponent<DungeonManager>();               
            enemiesList = new List<ExplorationCreature>();
            enemiesMoving = false;
            enemiesQuantity = initialEnemiesQty;            

            InitExploration(initialMapWidth, initialMapHeigth, removeLoneWallIterations, wallLayersQty, enemiesQuantity);
            passOneHour = addHourText.GetComponent<PassOneHour>();            
        }                

        void Update()
        {
            switch(explorationState)
            {                
                case (ExplorationStateEnum.PlayersTurn):
                    if(captainScript.characterState == ExplorationCharacter.CharacterState.WaitingNextTurn)
                    {
                        currentCharacterScript = captainScript;
                        captainScript.BeginTurn();
                    }                    
                    else if (currentCharacterScript.characterState == ExplorationCharacter.CharacterState.EndTurn)
                    {
                        if (captainScript.characterState == ExplorationCharacter.CharacterState.OnHold)
                        {
                            currentCharacterScript = captainScript;
                            captainScript.BeginTurn();                            
                        }
                        else
                        {
                            int currentCharacterIndex;

                            for (currentCharacterIndex = 0; currentCharacterIndex < explorationGroup.Count; currentCharacterIndex++)
                            {
                                ExplorationCharacter currentCharacter = explorationGroup[currentCharacterIndex].GetComponent<ExplorationCharacter>();

                                if (!captainScript.CharacterIsOnExplorationGroup(currentCharacter) && currentCharacter.characterState == ExplorationCharacter.CharacterState.WaitingNextTurn)
                                {
                                    currentCharacterScript = currentCharacter;
                                    currentCharacter.BeginTurn();
                                    break;
                                }
                            }

                            if (currentCharacterIndex == explorationGroup.Count)
                            {                                
                                NextTurn();
                            }
                        }
                        
                    }
                    
                    break;
                case (ExplorationStateEnum.EnemiesTurn):
                    if (!enemiesMoving) {                        
						enemiesMoving = true;    
						StartCoroutine(MoveEnemies());                        
                    }
                    break;                
                default:
                    break;               
            }

            if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
            {

                if (explorationState != ExplorationStateEnum.PausedGame)
                {
                    PauseGame();
                }
                else
                {
                    ResumeGame();                    
                }
            }
        }

        void InitExploration(int initialMapWidth, int initialMapHeigth, int removeLoneWallIterations, int wallLayersQty, int enemiesQty)
        {
            dungeonManager.BuildMap(initialMapWidth, initialMapHeigth, removeLoneWallIterations, wallLayersQty);

            Vector3 playerInitialPos = dungeonManager.dungeon.FloorCoords[0];

            if (captainCharacter == null)
            {
                captainCharacter = Instantiate(captainPrefab, playerInitialPos, Quaternion.identity) as GameObject;
                captainCharacter.name = "Captain";
                captainScript = captainCharacter.GetComponent<CaptainClass>();

                for (int i = 0; i < explorationGroup.Count; i++)
                {
                    explorationGroup[i] = Instantiate(explorationGroup[i], captainCharacter.transform.position, Quaternion.identity) as GameObject;
                    explorationGroup[i].name = "Character " + i;
                    captainScript.AddCharacterToGroup(explorationGroup[i].GetComponent<ExplorationCharacter>());
                    explorationGroup[i].SetActive(false);
                }
            }
            else
            {
                captainCharacter.transform.position = playerInitialPos;
                captainScript.explorationGroup.Clear();
                captainScript.CleanExplorationGroupHUD();

                foreach (GameObject explorationCharacter in explorationGroup)
                {
                    if (!captainScript.explorationGroup.Contains(explorationCharacter.GetComponent<ExplorationCharacter>()))
                    {
                        captainScript.AddCharacterToGroup(explorationCharacter.GetComponent<ExplorationCharacter>());
                    }
                }

                captainScript.characterState = ExplorationCharacter.CharacterState.WaitingNextTurn;
            }

            dungeonManager.dungeon.map[(int)playerInitialPos.x, (int)playerInitialPos.y].isOccupied = true;
            dungeonManager.dungeon.map[(int)playerInitialPos.x, (int)playerInitialPos.y].OccupyingObject = captainCharacter;

            SetCameraFocus(captainCharacter.transform);
            SetEnemies(enemiesQty);
            SetExit();

            explorationState = ExplorationStateEnum.PlayersTurn;
            levelText.GetComponent<LevelText>().UpdateLevel();            
        }

        void SetEnemies(int quantity)
        {
            Vector2 randomPos;

            enemiesParent = new GameObject("EnemiesParent").transform;
            enemiesParent.parent = dungeonManager.map.transform;

            while (quantity > 0)
            {
                randomPos = dungeonManager.dungeon.FloorCoords[Random.Range(0, dungeonManager.dungeon.FloorCoords.Count)];
                if ((randomPos.x != 1 && randomPos.y != 1) && !dungeonManager.dungeon.map[(int)randomPos.x, (int)randomPos.y].isOccupied)
                {
                    GameObject enemy = Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], new Vector2((int)randomPos.x, (int)randomPos.y), Quaternion.identity) as GameObject;
                    enemy.name = "Enemy" + quantity;
                    enemy.transform.parent = enemiesParent;
                    dungeonManager.dungeon.map[(int)randomPos.x, (int)randomPos.y].isOccupied = true;
                    dungeonManager.dungeon.map[(int)randomPos.x, (int)randomPos.y].OccupyingObject = enemy;
                    enemiesList.Add(enemy.GetComponent<ExplorationCreature>());
                    quantity--;
                }
            }
        }

        void SetExit()
        {
            exitParent = new GameObject("ExitParent").transform;
            exitParent.parent = dungeonManager.map.transform;

            Vector2 exitPos = dungeonManager.dungeon.DoorCoords[0];
            GameObject exit = Instantiate(dungeonManager.doorPrefab[0], exitPos, Quaternion.identity) as GameObject;

            exit.name = "Exit";
            exit.transform.parent = exitParent;
            dungeonManager.dungeon.map[(int)exitPos.x, (int)exitPos.y].isOccupied = true;
            dungeonManager.dungeon.map[(int)exitPos.x, (int)exitPos.y].OccupyingObject = exit;
        }

        public void ResumeGame()
        {
            explorationState = previousExplorationState;
            Time.timeScale = 1;
            pauseMenu.SetActive(false);
        }

        public void PauseGame()
        {
            previousExplorationState = explorationState;
            explorationState = ExplorationStateEnum.PausedGame;
            pauseMenu.SetActive(true);
            Time.timeScale = 0;            
        }

        IEnumerator MoveEnemies()
        {
            int enemyIndex = 0;
            while (enemyIndex < enemiesList.Count)
            {
                enemiesList[enemyIndex].Move();
                while (!enemiesList[enemyIndex].finishedMoving)
                {
                    yield return null;
                }

                enemyIndex++;
            }

            NextTurn();                        
        }        

        public void NextTurn()
        {
            switch (explorationState)
            {
                case (ExplorationStateEnum.PlayersTurn):                    
                    explorationState = ExplorationStateEnum.EnemiesTurn;
                    enemiesTurnText.gameObject.SetActive(true);
                    captainScript.characterState = ExplorationCharacter.CharacterState.WaitingNextTurn;
                    for (int i = 0; i < explorationGroup.Count; i++)
                    {
                        explorationGroup[i].GetComponent<ExplorationCharacter>().characterState = ExplorationCharacter.CharacterState.WaitingNextTurn;
                    }
                    break;
                case (ExplorationStateEnum.EnemiesTurn):
                    enemiesMoving = false;
                    explorationState = ExplorationStateEnum.PlayersTurn;
                    addHourText.gameObject.SetActive(true);
                    enemiesTurnText.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        public void RemoveCreatureFromList(ExplorationCreature creature)
        {
            enemiesList.Remove(creature);
        }

        public void NextLevel()
        {
            level++;

            enemiesList.Clear();
            Destroy(GameObject.Find("Map").gameObject);

            int newMapWidth = (int)(initialMapWidth + (level * 1.5f));
            int newMapHeigth = (int)(initialMapHeigth + (level * 1.5f));            

            if (level == 2)
            {
                enemiesQuantity = 1;
            }
            else if (level % 3 == 0)
            {
                enemiesQuantity += 1;
            }

            InitExploration(newMapWidth, newMapHeigth, removeLoneWallIterations, wallLayersQty, enemiesQuantity);
        }

        public void SetCameraFocus(Transform target)
        {
            Camera.main.GetComponent<CameraMovement>().target = target.transform;
        }

        public void GameOver()
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}