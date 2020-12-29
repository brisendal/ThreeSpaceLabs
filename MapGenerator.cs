using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public int width;
    public int height;
    
    public int wallThresholdSize = 50;
    public int roomThresholdSize = 50;

    public string seed;
    public bool randomSeed;

    [Range(45,55)]
    public int randomFillPercent;

   int[,] map;

    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        int borderSize = 5;
        int[,] borderMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderMap.GetLength(1); y++){
                if(x>= borderSize && x< width + borderSize && y>= borderSize && y < height + borderSize){
                    borderMap[x, y] = map[x - borderSize, y - borderSize];
                 }
                else
                {
                    borderMap[x, y] = 1;
                }
            }
        }

                MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(map, 1);
    }


    List<List<Coord>> getRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFalgs = new int[width, height];

        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                if(mapFalgs[x,y]== 0 && map[x,y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach(Coord tile in newRegion)
                    {
                        mapFalgs[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        return regions;
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = getRegions(1);

        //int wallThresholdSize = 50;
        foreach(List<Coord> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallThresholdSize)
            {
                foreach(Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = getRegions(0);
        List<Room> survivingRooms = new List<Room>();

       // int roomThresholdSize = 50;
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAcessableFromMainRoom = true;
        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAcessableFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            { 
                possibleConnectionFound = false;
                if(roomA.connectRooms.Count > 0)
                {
                    continue;
                }
            }
            
            foreach(Room roomB in roomListB)
            {
                if(roomA == roomB || roomA.isConnected(roomB))
                {
                    continue;
                }
               
                for(int tileIndexA = 0; tileIndexA<roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if(distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }
        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
      // Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);


        List<Coord> line = GetLine(tileA, tileB);
        foreach(Coord c in line)
        {
            DrawCircle(c, 1);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for(int x = -r; x<= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if(x*x + y*y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;

                    if(IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();
        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if(longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for(int i=0; i<longest; i++)
        {
            line.Add(new Coord(x, y));
            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if(gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else { y += gradientStep; }

                gradientAccumulation -= longest;
            }
            
        }
        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 0, -height / 2 + .5f + tile.tileY);
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFalgs = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));

        mapFalgs[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for(int x= tile.tileX -1; x<= tile.tileX+1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y==tile.tileY || x== tile.tileX))
                    {
                        if(mapFalgs[x,y] == 0 && map[x,y] == tileType)
                        {
                            mapFalgs[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    void RandomFillMap()
    {
        if (randomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for(int x=0; x<width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(x==0 || x== width-1 || y==0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else { map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0; }
                
            }
        }
    }


    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neigborWallTiles = GetSurroundingCount(x, y);

                if(neigborWallTiles > 4)
                {
                    map[x, y] = 1;

                }
                else if(neigborWallTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }

    }

    int GetSurroundingCount(int gridX, int gridY)
    {
        int WallCount = 0;
        for (int neigborX = gridX -1; neigborX <= gridX+1; neigborX++)
        {
            for (int neigborY = gridY - 1; neigborY <= gridY + 1; neigborY++)
            {
                if(neigborX >= 0 && neigborX < width && neigborY >= 0 && neigborY < height) {
                    if (neigborX != gridX || neigborY != gridY)
                    {

                        WallCount += map[neigborX, neigborY];
                    }
                }
                else
                {
                    WallCount++;
                }
            }
        }

        return WallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectRooms;
        public int roomSize;
        public bool isAcessableFromMainRoom;
        public bool isMainRoom;

        public Room()
        {

        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectRooms = new List<Room>();

            edgeTiles = new List<Coord>();

            foreach(Coord tile in tiles)
            {
                for(int x = tile.tileX -1; x<= tile.tileX +1; x++)
                {
                    for(int y = tile.tileY -1; y<=tile.tileY+1; y++)
                    {
                        if(x==tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessableFromMainRoom()
        {
            if (!isAcessableFromMainRoom)
            {
                isAcessableFromMainRoom = true;
                foreach(Room connectedRoom in connectRooms)
                {
                    connectedRoom.SetAccessableFromMainRoom ();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAcessableFromMainRoom)
            {
                roomB.SetAccessableFromMainRoom();
            }
            else if(roomB.isAcessableFromMainRoom)
            {
                roomA.SetAccessableFromMainRoom();
            }
            roomA.connectRooms.Add(roomB);
            roomB.connectRooms.Add(roomA);
        }

        public bool isConnected(Room otherRoom)
        {
            return connectRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
}
