using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;


public class PassOneHour : MonoBehaviour {   

    Text text;
    Transform playerTransform;
    HourText hourText;
    Transform hourTextTransform;

    public float animationSpeed;
    public int yDistance;

    // Use this for initialization
    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        hourTextTransform = GameObject.Find("Hours").GetComponent<Transform>();
        hourText = GameObject.Find("Hours").GetComponent<HourText>();
        text = GetComponent<Text>();
    }

    void OnEnable()
    {
        //Debug.Log("Enable");
        transform.position = playerTransform.position;
        transform.DOMove(new Vector3(transform.position.x, transform.position.y + yDistance, 0), animationSpeed).OnComplete(() => AnimateHour());
    }

    void AnimateHour()
    {        
        hourText.AddHour();
        gameObject.SetActive(false);
    }
   
}
