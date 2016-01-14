using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

    public GameObject gameManager;

	// Use this for initialization
	void Awake ()
    {
        if(GameManager1.instance == null)
        {
            Instantiate(gameManager);
        }	
	}	
}
