using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class Player : MovingObject {

    public int wallDamage = 1;
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;

    public float restartLevelDelay = 1f;

    private Text foodText;

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;
    public int food;
    private Vector2 touchOrigin = -Vector2.one;

    //int horizontal;
    //int vertical;

    // Use this for initialization
    protected override void Start () {
        animator = GetComponent<Animator>();

        food = GameManager1.instance.playerFoodPoints;
        //Debug.Log("Food: " + GameManager.instance.playerFoodPoints); 
        foodText = GameObject.Find("FoodText").GetComponent<Text>();
        foodText.text = "Food: " + food;

        base.Start();
	}

    void Awake()
    {
        if (GameManager1.instance != null)
        {
            food = GameManager1.instance.playerFoodPoints;
        }        
    }


    private void OnDisable()
    {
        GameManager1.instance.playerFoodPoints = food;
    }

    /*private void OnEnable()
    {        
        food = GameManager.instance.playerFoodPoints;        
    }*/

    // Update is called once per frame
    void Update () {       

        if (!GameManager1.instance.playersTurn)
        {            
            return;
        }

        int horizontal = 0;
        int vertical = 0;

        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER

            horizontal = (int)Input.GetAxisRaw("Horizontal");
            vertical = (int)Input.GetAxisRaw("Vertical");

            if(horizontal != 0)
            {
                vertical = 0;
            }

        #else

            if (Input.touchCount > 0)
            {
                Touch myTouch = Input.touches[0];

                if(myTouch.phase == TouchPhase.Began)
                {
                    touchOrigin = myTouch.position;
                }
                else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x > 0)
                {
                    Vector2 touchEnd = myTouch.position;
                    float x = touchEnd.x - touchOrigin.x;
                    float y = touchEnd.y - touchOrigin.y;
                    touchOrigin.x = -1;

                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        horizontal = x > 0 ? 1 : -1;
                    }
                    else
                        vertical = y > 0 ? 1 : -1;
                
                }

            }
        #endif

        if(horizontal != 0 || vertical != 0)
        {
            AttemptMove<Wall>(horizontal, vertical);
        }        

    }

    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        food--;
        foodText.text = "Food: " + food;


        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;

        if(Move(xDir, yDir, out hit))
        {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        CheckIfGameOver();

        GameManager1.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Exit")
        {
            GameManager1.instance.playerFoodPoints = food;
            Invoke("LoadNextLevel", restartLevelDelay);
            enabled = false;
        }
        else if (other.tag == "Food")
        {
            food += pointsPerFood;
            foodText.text = "+" + pointsPerFood + " Food: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda")
        {
            food += pointsPerSoda;
            foodText.text = "+" + pointsPerSoda + " Food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }
    }

    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;
        hitWall.DamagedWall(wallDamage);
        animator.SetTrigger("PlayerChop");
    }

    private void LoadNextLevel()
    {
        GameManager1.instance.LoadNextLevel();        
    }

    public void LoseFood (int loss)
    {
        animator.SetTrigger("PlayerHit");
        food -= loss;
        foodText.text = "-" + loss + " Food: " + food;
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if (food <=0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            //SoundManager.instance.musicSource.Stop();
            GameManager1.instance.GameOver();
        }
    }

}
