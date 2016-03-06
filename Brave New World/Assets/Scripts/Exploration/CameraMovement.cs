﻿using UnityEngine;
using System.Collections;

namespace BraveNewWorld
{
    public class CameraMovement : MonoBehaviour
    {
        [HideInInspector]
        public Transform target;        

        Camera cam;
        float height;
        float width;

        void Start()
        {
            cam = Camera.main;
            height = 2f * cam.orthographicSize;
            width = height * cam.aspect;           
        }
                
        void Update()
        {
            transform.position = new Vector3(
                Mathf.Clamp(target.position.x, -1 + width / 2, ExplorationSceneManager.instance.dungeonManager.dungeon.MapWidth - width / 2),
                Mathf.Clamp(target.position.y, -1 + height / 2, ExplorationSceneManager.instance.dungeonManager.dungeon.MapHeigth - height / 2),
                -10f);
        }
    }
}