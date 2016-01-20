using UnityEngine;
using System.Collections;
namespace BraveNewWorld
{
    public class ExplorationEnemy : ExplorationMovableObject
    {
        public new void Move()
        {
            if (movementParent != null)
            {
                Destroy(movementParent.gameObject);
            }
            path.Clear();
            PossibleMovement();            
            path = pathFinding.FindPath(transform.position, possibleMovement[Random.Range(0, possibleMovement.Count)]);
            base.Move();
        }
    }
}
