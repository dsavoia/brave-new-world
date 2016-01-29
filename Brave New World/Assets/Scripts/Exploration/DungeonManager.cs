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
        //public GameObject playerPrefab;
        //public GameObject zoombiePrefab;
        public DungeonGenerator generator;

        public int mapWidth;
        public int mapHeigth;
        public int removeLoneWallIterations;
        public int wallLayersQty;       

        //public int masterMapWidth;
        //public int masterMapHeigth;
        //public int mapQuantity;
        //public int zoombieQuantity = 1;
        //public int touchCount = 0;
        //public Dungeon[,] dungeonsMap;

        public Dungeon dungeon;

        [HideInInspector]
        public GameObject map;             

        //Print only one map
        public void BuildMap()
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
                dungeon.map[(int)floor.x, (int)floor.y].isOccupied = false;

            }

            //Placing wall tiles
            foreach (Vector2 wall in dungeon.WallCoords)
            {
                wallPB = GameObject.Instantiate(wallPrefab[Random.Range(0, wallPrefab.Length)], new Vector3(wall.x, wall.y, 0), Quaternion.identity) as GameObject;
                wallPB.transform.parent = wallParent.transform;                
                dungeon.map[(int)wall.x, (int)wall.y].isOccupied = true;
            }

            //Placing door tiles
            foreach (Vector2 door in dungeon.DoorCoords)
            {
                doorPB = GameObject.Instantiate(doorPrefab[Random.Range(0, doorPrefab.Length)], new Vector3(door.x, door.y, 0), Quaternion.identity) as GameObject;
                doorPB.transform.parent = doorParent.transform;                
                dungeon.map[(int)door.x, (int)door.y].isOccupied = false;
            }

            //Placing Indestructible wall tiles
            foreach (Vector2 wall in dungeon.IndestructibleWallCoords)
            {
                indestructibleWallPB = GameObject.Instantiate(indestructibleWallPrefab[Random.Range(0, indestructibleWallPrefab.Length)], new Vector3(wall.x, wall.y, 0), Quaternion.identity) as GameObject;
                indestructibleWallPB.transform.parent = indestructibleWallParent.transform;                
                dungeon.map[(int)wall.x, (int)wall.y].isOccupied = true;
            }

            //GameObject player = GameObject.Instantiate(playerPrefab, new Vector3(dungeonsMap[0, 0].InitialPos.x, dungeonsMap[0, 0].InitialPos.y, 0), Quaternion.identity) as GameObject;
            //Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, Camera.main.transform.position.z);
        }

        /*public void GenerateZoombies()
        {

            GameObject zoombieParent;
            zoombieParent = new GameObject("zoombieParent");
            zoombieParent.transform.parent = currentMap.transform;

            List<Vector3> zoombiePosList = new List<Vector3>();
            Vector3 zoombiePos = Vector3.zero;

            GameObject zoombiePB;
            int randFloorIndex;
            bool zoombiePositioned;

            while (zoombieQuantity > 0)
            {
                zoombiePositioned = false;
                while (!zoombiePositioned)
                {
                    randFloorIndex = Random.Range(0, dungeon.FloorCoords.Count);
                    zoombiePos = new Vector3(dungeon.FloorCoords[randFloorIndex].x, dungeon.FloorCoords[randFloorIndex].y, 0);

                    if (!zoombiePosList.Exists(zp => zp == zoombiePos))
                        zoombiePositioned = true;
                }

                zoombiePosList.Add(zoombiePos);

                zoombiePB = GameObject.Instantiate(zoombiePrefab,
                                                   zoombiePos,
                                                      Quaternion.identity) as GameObject;

                zoombiePB.transform.parent = zoombieParent.transform;
                zoombieQuantity--;
            }
        }*/
    }
}
