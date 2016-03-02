using UnityEngine;
using System.Collections;
using System;


namespace BraveNewWorld
{
    public class ExplorationCreature : ExplorationMovableObject
    {

        public enum CreatureState
        {            
            Moving,            
            Attacking,
            WaitingAnimation,
            WaitingNextTurn,
            EndTurn
        }        

        CreatureState creatureState;        
        Vector2 nextPos;        

        new void Awake()
        {
            base.Awake();
            creatureState = CreatureState.WaitingNextTurn;            
        }

        public new void Move()
        {
            
                nextPos = new Vector2();

                path.Clear();
                PossibleMovement();

                if (objectsArroundMe.Count > 0)
                {                    
                    foreach (GameObject go in objectsArroundMe)
                    {
                        if (go.tag == "Character")
                        {                            
                            nextPos = go.transform.position;
                            path = pathFinding.FindPath(transform.position, nextPos);//possibleMovement[UnityEngine.Random.Range(0, possibleMovement.Count)]);
                                                    
                            path.Remove(path[path.Count - 1]);
                        
                            if (path.Count == 0)
                            {
                                path.Add(ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)transform.position.x, (int)transform.position.y]);
                            }

                        creatureState = CreatureState.Attacking;

                        break;
                        }
                    }                    
                }
                else
                {
                    do {
                        nextPos = possibleMovement[UnityEngine.Random.Range(0, possibleMovement.Count)];
                    } while (nextPos == new Vector2(transform.position.x, transform.position.y));

                    creatureState = CreatureState.Moving;
                    path = pathFinding.FindPath(transform.position, nextPos);
                }


            if (path.Count == 0)
            {
                do { 
                 path = pathFinding.FindPath(transform.position, possibleMovement[UnityEngine.Random.Range(0, possibleMovement.Count)]);
                } while(path.Count == 0);
                creatureState = CreatureState.Moving;
            }

            base.Move();          
            
        }

        public override void EndMovement()
        {
            base.EndMovement();
            switch (creatureState)
            {
                case CreatureState.Moving:
                    creatureState = CreatureState.EndTurn;
                    break;                
                case CreatureState.Attacking:
                    Debug.Log(gameObject.name +" is attacking " + ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)nextPos.x, (int)nextPos.y].OccupyingObject.name);
                    StartCoroutine(Attack(ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)nextPos.x, (int)nextPos.y].OccupyingObject));
                    break;

            }
        }

        public override void HighLightEnemies()
        {
            //Vector2 checkPos = new Vector2();
            enemiesHighLightParent = new GameObject(gameObject.name + " EnemiesHightlightParent").transform;
            enemiesHighLightParent.transform.SetParent(ExplorationSceneManager.instance.dungeonManager.map.transform);

            if (occupiedPosList.Count > 0)
            {
                foreach (Vector2 pos in occupiedPosList)
                {
                    GameObject instance = Instantiate(enemiesHighLightPB, pos, Quaternion.identity) as GameObject;
                    instance.transform.SetParent(enemiesHighLightParent.transform);
                }
            }
        }

        public override void HighLightAllies()
        {
            throw new NotImplementedException();
        }

        public override void HighLightNeutrals()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator Attack(GameObject target)
        {          
            animator.SetTrigger("EnemyAttack");

            yield return new WaitForSeconds(target.GetComponent<ExplorationMovableObject>().TakeDamage(1));

            creatureState = CreatureState.EndTurn;
        }

        public override float TakeDamage(int damage)
        {
            float animationTime = 1.0f;            

            animator.SetTrigger("EnemyAttack");
            actualHP -= damage;

            healthBar.value = actualHP;

            if (actualHP <= 0)
            {
                return Die();
            }

            return animationTime;
        }

        public override float Die()
        {
            base.Die();
            float animationTime = 1.0f;
            ExplorationSceneManager.instance.RemoveCreatureFromList(GetComponent<ExplorationCreature>());
            Destroy(gameObject);
            return animationTime;
        }
    }
}
