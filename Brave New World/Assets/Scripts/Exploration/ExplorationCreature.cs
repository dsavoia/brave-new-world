using UnityEngine;
using System.Collections;
using System;

namespace BraveNewWorld
{
    public class ExplorationCreature : ExplorationMovableObject
    {        
        public int healthQuantity = 3;


        new void Awake()
        {
            base.Awake();
            showMyPossibleMovement = true;
        }

        public new void Move()
        {
            Vector2 nextPos = new Vector2();          

            path.Clear();
            PossibleMovement();

            if (objectsArroundMe.Count > 0)
            {
                Debug.Log("There are objects arround " + gameObject.name);
                foreach (GameObject go in objectsArroundMe)
                {
                    if (go.tag == "Character")
                    {
                        Debug.Log("Character arround " + gameObject.name);
                        nextPos = go.transform.position;
                        path = pathFinding.FindPath(transform.position, nextPos);//possibleMovement[UnityEngine.Random.Range(0, possibleMovement.Count)]);
                        path.Remove(path[path.Count - 1]);

                        if(path.Count == 0)
                        {
                            path.Add(ExplorationSceneManager.instance.dungeonManager.dungeon.map[(int)transform.position.x, (int)transform.position.y]);
                        }

                        break;
                    }
                }                
            } 
            else
            {
                nextPos = possibleMovement[UnityEngine.Random.Range(0, possibleMovement.Count)];
                path = pathFinding.FindPath(transform.position, nextPos);
            }           

            base.Move();          
            
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
            throw new NotImplementedException();
        }

        public override float TakeDamage(int damage)
        {
            float animationTime = 1.0f;

            animator.SetTrigger("EnemyAttack");
            healthQuantity -= damage;

            if (healthQuantity <= 0)
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
