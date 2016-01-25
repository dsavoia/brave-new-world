using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class HourText : MonoBehaviour {

    Text hourText;
    int hour;

	// Use this for initialization
	void Awake () {
        hourText = GetComponent<Text>() ;
        hour = 8;
	}
	
	// Update is called once per frame
	void Update () {
        hourText.text = hour + ":00";
	}

    public void AddHour()
    {
        hour++;
        if (hour > 23)
            hour = 0;

        hourText.transform.DOScaleX(1.3f, 0.3f).OnComplete(()=> { hourText.transform.DOScaleX(1.0f, 0.3f); });
    }
}
