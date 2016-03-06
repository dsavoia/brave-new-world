using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BraveNewWorld
{
    public class DungeonGenerator
    {
        int mapWidth;
        int mapHeigth;
        int maxFloorQty;
        int removeLoneWallIterations;
        int wallLayersQty;

        public Tile[,] map;

        public List<Vector2> floorCoords;
        public List<Vector2> wallCoords;
        public List<Vector2> indestructibleWallCoords;
        public List<Vector2> doorCoords;
        public Vector2 initialPos;

        int placedFloorQty;

        public Dungeon BuildDungeon(int width, int heigth, int removeWallIterations, int wallLayers)
        {
            mapWidth = width;
            mapHeigth = heigth;
            maxFloorQty = (int)((mapWidth * mapHeigth) * 0.40f);

            floorCoords = new List<Vector2>();
            wallCoords = new List<Vector2>();
            doorCoords = new List<Vector2>();
            indestructibleWallCoords = new List<Vector2>();
            initialPos = Vector2.zero;

            map = new Tile[mapWidth, mapHeigth];

            removeLoneWallIterations = removeWallIterations;
            wallLayersQty = wallLayers;

            placedFloorQty = 0;

            GenerateMap();

            Dungeon dungeon = new Dungeon();

            dungeon.map = map;
            dungeon.MapWidth = width;
            dungeon.MapHeigth = heigth;
            dungeon.FloorCoords = floorCoords;
            dungeon.WallCoords = wallCoords;
            dungeon.DoorCoords = doorCoords;
            dungeon.IndestructibleWallCoords = indestructibleWallCoords;
            dungeon.InitialPos = initialPos;
            //dungeon.MapNumber = currentMap;

            return dungeon;
        }

        void GenerateMap()
        {

            CreateEmptyMap();
            GenerateMapFloor();
            GenerateMapWall();
            EliminateWallSurroundedByFloor();
            EliminateSurroundedFloorFormation();
            AddDoor();
            AddWallLayer();
            AddIndestructibleWallLayer();
            /*Debug.Log("Floor Count: " + floorCoords.Count);
            Debug.Log("Wall Count: " + wallCoords.Count);
            Debug.Log("Indestructable Wall Count: " + indestructibleWallCoords.Count);*/
        }

        void CreateEmptyMap()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    map[i, j] = new Tile(TileTypeEnum.Empty, new Vector2(i, j));
                    map[i, j].isOccupied = false;
                }
            }
        }


        void GenerateMapFloor()
        {
            //Starting at the central point	
            Vector2 actualPos = new Vector2(Mathf.Abs(map.GetLength(0) / 2), Mathf.Abs(map.GetLength(1) / 2));
            initialPos = actualPos;
            List<Vector2> prevPos = new List<Vector2>();
            //InicialPos is always a floor
            map[(int)actualPos.x, (int)actualPos.y].tileType = TileTypeEnum.Floor;
            map[(int)actualPos.x, (int)actualPos.y].isOccupied = false;
            floorCoords.Add(actualPos);
            prevPos.Add(actualPos);
            int x = 0, y = 0;
            int nextPos = -1;
            bool possiblePos;

            while (placedFloorQty < maxFloorQty)
            {
                //do{
                possiblePos = true;
                nextPos = DefineNextPosition(actualPos);

                switch (nextPos)
                {
                    //up
                    case 0:
                        if (actualPos.y == mapHeigth - 3)
                        {
                            possiblePos = false;
                        }
                        else {
                            x = 0; y = 1;
                        }
                        break;
                    //right
                    case 1:
                        if (actualPos.x == mapWidth - 3)
                            possiblePos = false;
                        else {
                            x = 1; y = 0;
                        }
                        break;
                    //down
                    case 2:
                        if (actualPos.y == 2)
                            possiblePos = false;
                        else {
                            x = 0; y = -1;
                        }
                        break;
                    //left
                    case 3:
                        if (actualPos.x == 2)
                            possiblePos = false;
                        else {
                            x = -1; y = 0;
                        }
                        break;
                    default:
                        x = y = 0;
                        possiblePos = false;
                        break;
                }
                //if(actualPos.x == x && actualPos.y == y){
                //	possiblePos = false;						
                //}			
                //}while(!possiblePos);

                if (possiblePos)
                {
                    actualPos = new Vector2(actualPos.x + x, actualPos.y + y);
                    if (prevPos.Count >= maxFloorQty / 6)
                    {
                        prevPos.RemoveAt(0);
                    }
                    prevPos.Add(actualPos);
                    if (!floorCoords.Exists(pos => pos == actualPos))
                    {
                        map[(int)actualPos.x, (int)actualPos.y].tileType = TileTypeEnum.Floor;
                        map[(int)actualPos.x, (int)actualPos.y].isOccupied = false;
                        floorCoords.Add(actualPos);
                        placedFloorQty++;
                    }
                }
                else {
                    actualPos = initialPos;
                }
            }
        }

        int DefineNextPosition(Vector2 actualPos)
        {
            int rand = Random.Range(0, 4);         
            return rand;
        }

        void GenerateMapWall()
        {
            //foreach floor, verify its surroudings and change the value from -1 (empty) to 0 (wall)		
            for (int i = 0; i < floorCoords.Count; i++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (map[(int)floorCoords[i].x + x, (int)floorCoords[i].y + y].tileType == TileTypeEnum.Empty)
                        {
                            wallCoords.Add(new Vector2(floorCoords[i].x + x, floorCoords[i].y + y));
                            map[(int)floorCoords[i].x + x, (int)floorCoords[i].y + y].tileType = TileTypeEnum.Wall;
                            map[(int)floorCoords[i].x + x, (int)floorCoords[i].y + y].isOccupied = true;
                        }
                    }
                }
            }
        }

        void EliminateWallSurroundedByFloor()
        {
            int iterations = 0;
            Vector2 wall;
            do
            {
                for (int i = 0; i < wallCoords.Count; i++)
                {
                    wall = new Vector2((int)wallCoords[i].x, (int)wallCoords[i].y);
                    //If the wall tile is surrounded by floor tiles it will became a floor tile									
                    if (WallSurroundedByFloor(wall))
                    {
                        floorCoords.Add(wall);
                        wallCoords.Remove(wall);
                        map[(int)wall.x, (int)wall.y].tileType = TileTypeEnum.Floor;
                        map[(int)wall.x, (int)wall.y].isOccupied = false;
                    }
                }
                iterations++;
            } while (iterations < removeLoneWallIterations);
        }

        //Removing walls that are surrounded by floor tiles
        bool WallSurroundedByFloor(Vector2 wall)
        {
            int notFloor = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 0 && y != 0)
                    {
                        if (map[(int)wall.x + x, (int)wall.y + y].tileType != TileTypeEnum.Floor)
                        {
                            notFloor++;
                            if (notFloor > 1)
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        void EliminateSurroundedFloorFormation()
        {
            Vector2[] surroundingWalls = new Vector2[4];
            int currentSurroundWall;
            bool surrounded;
            for (int i = 0; i < floorCoords.Count; i++)
            {
                surroundingWalls[0] = new Vector2();
                surroundingWalls[1] = new Vector2();
                surroundingWalls[2] = new Vector2();
                surroundingWalls[3] = new Vector2();
                surrounded = true;
                currentSurroundWall = 0;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if ((x == 0 || y == 0) && (x != y))
                        {
                            if (map[(int)floorCoords[i].x + x, (int)floorCoords[i].y + y].tileType == TileTypeEnum.Wall)
                            {
                                surroundingWalls[currentSurroundWall] = new Vector2(floorCoords[i].x + x, floorCoords[i].y + y);
                                currentSurroundWall++;
                            }
                            else {
                                surrounded = false;
                                break;
                            }
                        }
                    }

                    if (!surrounded)
                        break;
                }

                bool loneWall;
                int wallQty;
                if (surrounded)
                {
                    for (int j = 0; j < surroundingWalls.GetLength(0); j++)
                    {
                        loneWall = true;
                        wallQty = 0;
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                if (x != 0 && y != 0)
                                {
                                    if (map[(int)surroundingWalls[j].x + x, (int)surroundingWalls[j].y + y].tileType != TileTypeEnum.Floor)
                                    {
                                        wallQty++;
                                        if (wallQty > 2)
                                        {
                                            loneWall = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (!loneWall)
                                break;
                        }
                        if (loneWall)
                        {
                            floorCoords.Add(surroundingWalls[j]);
                            wallCoords.Remove(surroundingWalls[j]);
                            map[(int)surroundingWalls[j].x, (int)surroundingWalls[j].y].tileType = TileTypeEnum.Floor;
                            map[(int)surroundingWalls[j].x, (int)surroundingWalls[j].y].isOccupied = false;
                        }
                    }
                }
            }
        }       

        void AddDoor()
        {
            bool goodDoor = false;
            int wallIndex = 0;

            while (!goodDoor)
            {
                wallIndex = Random.Range(0, wallCoords.Count);

                if (map[(int)wallCoords[wallIndex].x + 1, (int)wallCoords[wallIndex].y].tileType == TileTypeEnum.Floor)
                {
                    goodDoor = true;
                }
                else if (map[(int)wallCoords[wallIndex].x - 1, (int)wallCoords[wallIndex].y].tileType == TileTypeEnum.Floor)
                {
                    goodDoor = true;
                }
                else if (map[(int)wallCoords[wallIndex].x, (int)wallCoords[wallIndex].y + 1].tileType == TileTypeEnum.Floor)
                {
                    goodDoor = true;
                }
                else if (map[(int)wallCoords[wallIndex].x, (int)wallCoords[wallIndex].y - 1].tileType == TileTypeEnum.Floor)
                {
                    goodDoor = true;
                }
            }

            doorCoords.Add(new Vector2(wallCoords[wallIndex].x, wallCoords[wallIndex].y));

            map[(int)wallCoords[wallIndex].x, (int)wallCoords[wallIndex].y].tileType = TileTypeEnum.Floor;
            map[(int)wallCoords[wallIndex].x, (int)wallCoords[wallIndex].y].isOccupied = true;
            wallCoords.RemoveAt(wallIndex);
        }

        void AddWallLayer()
        {
            List<Vector2> newWallLayer = new List<Vector2>();
            int iterarion = 0;
            while (iterarion < wallLayersQty)
            {
                for (int i = 0; i < wallCoords.Count; i++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            if (x != 0 && y != 0)
                            {
                                Vector2 newWall = new Vector2(wallCoords[i].x + x, wallCoords[i].y + y);
                                //Verifying the map Boundaries
                                if (newWall.x < mapWidth - 1 && newWall.x >= 1 && newWall.y < mapHeigth - 1 && newWall.y >= 1)
                                {
                                    if (map[(int)newWall.x, (int)newWall.y].tileType == TileTypeEnum.Empty)
                                    {
                                        if (!newWallLayer.Exists(nw => nw == newWall))
                                        {
                                            newWallLayer.Add(newWall);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Vector2 wall in newWallLayer)
                {
                    if (!wallCoords.Exists(w => w == wall))
                    {
                        wallCoords.Add(wall);
                        map[(int)wall.x, (int)wall.y].tileType = TileTypeEnum.Wall;
                        map[(int)wall.x, (int)wall.y].isOccupied = true;
                    }
                }

                iterarion++;
            }
        }

        void AddIndestructibleWallLayer()
        {
            for (int i = 0; i < wallCoords.Count; i++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x != 0 && y != 0)
                        {
                            Vector2 newWall = new Vector2(wallCoords[i].x + x, wallCoords[i].y + y);
                            //Verifying the map Boundaries
                            if (newWall.x < mapWidth && newWall.x >= 0 && newWall.y < mapHeigth && newWall.y >= 0)
                            {
                                if (map[(int)newWall.x, (int)newWall.y].tileType == TileTypeEnum.Empty)
                                {
                                    if (!indestructibleWallCoords.Exists(nw => nw == newWall))
                                    {
                                        indestructibleWallCoords.Add(newWall);
                                        map[(int)newWall.x, (int)newWall.y].tileType = TileTypeEnum.IndestructibleWall;
                                        map[(int)newWall.x, (int)newWall.y].isOccupied = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }        
    }
}