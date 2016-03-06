using System.Collections.Generic;
using UnityEngine;
namespace BraveNewWorld
{
    public class DungeonManager : MonoBehaviour
    {
        public GameObject[] floorPrefab;
        public GameObject[] wallPrefab;
        public GameObject[] indestructibleWallPrefab;
        public GameObject[] doorPrefab;      
        public DungeonGenerator generator;            
        public Dungeon dungeon;
        [HideInInspector] public GameObject map;             
                
        public void BuildMap(int mapWidth, int mapHeigth, int removeLoneWallIterations, int wallLayersQty)
        {
            map = new GameObject("Map");
            dungeon = new Dungeon();
            generator = new DungeonGenerator();           

            GameObject floorParent;
            GameObject wallParent;
            GameObject doorParent;
            GameObject indestructibleWallParent;

            GameObject floorPB;
            GameObject wallPB;
            GameObject doorPB;
            GameObject indestructibleWallPB;

            dungeon = generator.BuildDungeon(mapWidth, mapHeigth, removeLoneWallIterations, wallLayersQty);
            
            floorParent = new GameObject("floorParent");
            wallParent = new GameObject("wallParent");
            doorParent = new GameObject("doorParent");
            indestructibleWallParent = new GameObject("indWallParent");

            floorParent.transform.parent = map.transform;
            wallParent.transform.parent = map.transform;
            doorParent.transform.parent = map.transform;
            indestructibleWallParent.transform.parent = map.transform;

            //Placing floor tiles
            foreach (Vector2 floor in dungeon.FloorCoords)
            {
                floorPB = GameObject.Instantiate(floorPrefab[Random.Range(0, floorPrefab.Length)], new Vector3(floor.x, floor.y, 0), Quaternion.identity) as GameObject;
                floorPB.transform.parent = floorParent.transform;
            }

            //Placing wall tiles
            foreach (Vector2 wall in dungeon.WallCoords)
            {
                wallPB = GameObject.Instantiate(wallPrefab[Random.Range(0, wallPrefab.Length)], new Vector3(wall.x, wall.y, 0), Quaternion.identity) as GameObject;
                wallPB.transform.parent = wallParent.transform;                
            }

            //Placing door tiles
            /*foreach (Vector2 door in dungeon.DoorCoords)
            {
                doorPB = GameObject.Instantiate(doorPrefab[Random.Range(0, doorPrefab.Length)], new Vector3(door.x, door.y, 0), Quaternion.identity) as GameObject;
                doorPB.transform.parent = doorParent.transform;                
            }*/

            //Placing Indestructible wall tiles
            foreach (Vector2 indestructibleWall in dungeon.IndestructibleWallCoords)
            {
                indestructibleWallPB = GameObject.Instantiate(indestructibleWallPrefab[Random.Range(0, indestructibleWallPrefab.Length)], new Vector3(indestructibleWall.x, indestructibleWall.y, 0), Quaternion.identity) as GameObject;
                indestructibleWallPB.transform.parent = indestructibleWallParent.transform;                                
            }            
        }        
    }
}
