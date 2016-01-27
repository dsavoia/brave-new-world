using UnityEngine;
using System.Collections;
using DG.Tweening;

namespace BraveNewWorld
{
    public class CreaturesTurnText : MonoBehaviour
    {

        Transform playerTransform;
        public float animationSpeed;
        public float xSize;

        void Awake()
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void OnEnable()
        {
            transform.position = playerTransform.position;
        }

        void Update()
        {
            transform.DOScaleX(xSize, animationSpeed).OnComplete(() => { transform.DOScaleX(1, animationSpeed); });
        }
    }
}

