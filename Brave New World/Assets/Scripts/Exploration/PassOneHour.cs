using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace BraveNewWorld
{

    public class PassOneHour : MonoBehaviour
    {

        Text text;
        Transform playerTransform;
        DayAndNight dayAndNight;
        Transform hourTextTransform;

        public float animationSpeed;
        public int yDistance;
        
        void Awake()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            hourTextTransform = GameObject.Find("Hours").GetComponent<Transform>();
            dayAndNight = GameObject.Find("DayAndNightMask").GetComponent<DayAndNight>();
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
            dayAndNight.AddHour();
            gameObject.SetActive(false);
        }

    }
}