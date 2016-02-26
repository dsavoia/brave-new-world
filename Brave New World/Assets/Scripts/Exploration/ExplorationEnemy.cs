using UnityEngine;
using System.Collections;
using System;

namespace BraveNewWorld
{
    public class ExplorationEnemy : ExplorationMovableObject
    {
        new void Awake()
        {
            base.Awake();
            showMyPossibleMovement = true;

        }

        public new void Move()
        {
            if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }

            path.Clear();
            PossibleMovement();
            path = pathFinding.FindPath(transform.position, transform.position);//possibleMovement[UnityEngine.Random.Range(0, possibleMovement.Count)]);
            base.Move();

            //DEBUG REASON ONLY
            foreach (GameObject go in ObjectsArroundMe)
            {
            //    Debug.Log(go.name);
            }
        }
        
                
    }
}
