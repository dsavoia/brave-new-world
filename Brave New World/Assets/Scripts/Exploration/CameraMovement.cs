using UnityEngine;
using System.Collections;

namespace BraveNewWorld
{
    public class CameraMovement : MonoBehaviour
    {
        Camera camera;
        public float dampTime = 0.15f;
        private Vector3 velocity = Vector3.zero;

        [HideInInspector] public Transform target;

        void Start()
        {
            camera = Camera.main;
        }

            // Update is called once per frame
            void Update()
        {
            if (target)
            {
                Vector3 point = camera.WorldToViewportPoint(target.position);
                Vector3 delta = target.position - camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
                Vector3 destination = transform.position + delta;
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
            }

        }       
    }
}
