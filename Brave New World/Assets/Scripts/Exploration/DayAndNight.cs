using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace BraveNewWorld
{
    public class DayAndNight : MonoBehaviour
    {
        HourText hourText;
        public float fadeSpeed;

        void Awake()
        {
            hourText = GameObject.Find("Hours").GetComponent<HourText>();
        }

        public void AddHour()
        {
            ExplorationSceneManager.instance.hours++;
            if (ExplorationSceneManager.instance.hours > 23)
                ExplorationSceneManager.instance.hours = 0;

            hourText.AnimateHourText();

            if (ExplorationSceneManager.instance.hours % 3 == 0)
            {
                StartCoroutine(AnimateDayTimeChange());
            }
        }

        IEnumerator AnimateDayTimeChange()
        {
            Color color = gameObject.GetComponent<SpriteRenderer>().color;

            switch (ExplorationSceneManager.instance.hours)
            {
                case 0:
                case 3:
                    gameObject.GetComponent<SpriteRenderer>().DOFade(0.58f, Time.deltaTime * fadeSpeed);
                    break;
                case 6:
                    gameObject.GetComponent<SpriteRenderer>().DOFade(0.35f, Time.deltaTime * fadeSpeed);
                    break;
                case 9:
                    gameObject.GetComponent<SpriteRenderer>().DOFade(0.18f, Time.deltaTime * fadeSpeed);
                    break;
                case 12:
                    gameObject.GetComponent<SpriteRenderer>().DOFade(0f, Time.deltaTime * fadeSpeed);
                    break;
                case 15:
                    gameObject.GetComponent<SpriteRenderer>().DOFade(0.15f, Time.deltaTime * fadeSpeed);
                    break;
                case 18:
                    gameObject.GetComponent<SpriteRenderer>().DOFade(0.30f, Time.deltaTime * fadeSpeed);
                    break;
                case 21:
                    gameObject.GetComponent<SpriteRenderer>().DOFade(0.43f, Time.deltaTime * fadeSpeed);
                    break;
                default:
                    break;
            }

            yield return null;
        }

    }
}
