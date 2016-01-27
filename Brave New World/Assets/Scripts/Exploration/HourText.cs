using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

namespace BraveNewWorld
{
    public class HourText : MonoBehaviour
    {

        Text hourText;

        // Use this for initialization
        void Awake()
        {
            hourText = GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            hourText.text = ExplorationSceneManager.instance.hours + ":00";            
        }

        public void AnimateHourText()
        {
            transform.DOScaleX(1.3f, 0.3f).OnComplete(() => { transform.DOScaleX(1.0f, 0.3f); });
        }
        
    }
}