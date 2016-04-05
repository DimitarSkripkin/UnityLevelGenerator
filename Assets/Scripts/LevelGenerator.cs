using UnityEngine;
using System.Collections;
using System.Linq;
using Generator;

public class LevelGenerator : MonoBehaviour
{
    public GameObject endOfLevelTrigger;

    public GameObject SpawnPoint;

    public GameObject levelItemsHolder;

    public GameObject levelWallsHolder;

    public GameObject levelFloorsHolder;

    public int roomSize = 10;

    public int roomRowsCount = 4;

    public int roomCollumnsCount = 4;

    // TODO: rename?
    public float roomItemsPadding = 0.2f;

    public int seed = -1;

    // Use this for initialization
    void Start()
    {
        Generate();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Generate()
    {
        var items = levelItemsHolder
            .GetComponentsInChildren<Transform>()
            .Select(i => i.gameObject)
            .Skip(1)
            .ToArray();
        //int itemsCount = items.Length;

        var walls = levelWallsHolder
            .GetComponentsInChildren<Transform>()
            .Select(i => i.gameObject)
            .Skip(1)
            .ToArray();

        var floors = levelFloorsHolder
            .GetComponentsInChildren<Transform>()
            .Select(i => i.gameObject)
            .Skip(1)
            .ToArray();

        foreach (var item in items)
        {
            item.SetActive(false);
        }

        Vector2 seedOffset;
        if (seed < 0)
        {
            seedOffset = new Vector2(Random.Range(0, 100), Random.Range(0, 100));
            seed = Random.Range(0, 100);
        }
        else
        {
            seedOffset = new Vector2(seed, seed);
        }

        int gridCount = (1 << 3);
        int dungeonSize = (1 << 2);

        Vector2 startDungeonOffset = new Vector2(Random.Range(0, gridCount - dungeonSize), Random.Range(0, gridCount - dungeonSize));
        //int dungeonSeed = seed % (gridCount - dungeonSize);
        //startDungeonOffset = new Vector2(dungeonSeed, dungeonSeed);
        Rect dungeonDimentions = new Rect(startDungeonOffset, new Vector2(dungeonSize, dungeonSize));

        int populatedCells = 0;
        RoomGenerator[] dungeonRooms = new RoomGenerator[dungeonSize * dungeonSize];
        for (int cell = 0; cell < gridCount * gridCount; cell++)
        {
            int x, y;
            d2xy(gridCount, cell, out x, out y);

            // if cell is in the dungeon and the previous cell is neighbour add it to the list
            if (dungeonDimentions.Contains(new Vector2(x, y)))
            {
                if (populatedCells > 0)
                {
                    var lastRoom = dungeonRooms[populatedCells - 1];
                    if (Vector3.Distance(lastRoom.coord, new Vector3(x, 0.0f, y)) <= 1.0f)
                    {
                        var newRoom = new RoomGenerator();
                        newRoom.Type = RoomType.Corridor;
                        newRoom.coord.Set(x, 0.0f, y);

                        if (lastRoom.coord.x < x)
                        {
                            lastRoom.doorsPositions |= (int)DoorPosition.Right;
                            newRoom.doorsPositions = (int)DoorPosition.Left;
                        }
                        else if (lastRoom.coord.x > x)
                        {
                            lastRoom.doorsPositions |= (int)DoorPosition.Left;
                            newRoom.doorsPositions = (int)DoorPosition.Right;
                        }
                        else if (lastRoom.coord.z < y)
                        {
                            lastRoom.doorsPositions |= (int)DoorPosition.Top;
                            newRoom.doorsPositions = (int)DoorPosition.Bottom;
                        }
                        else if (lastRoom.coord.z > y)
                        {
                            lastRoom.doorsPositions |= (int)DoorPosition.Bottom;
                            newRoom.doorsPositions = (int)DoorPosition.Top;
                        }
                        else
                        {
                            throw new UnityException("WTF?");
                        }

                        dungeonRooms[populatedCells] = newRoom;
                        ++populatedCells;
                    }
                }
                else
                {
                    dungeonRooms[populatedCells] = new RoomGenerator();
                    dungeonRooms[populatedCells].coord.Set(x, 0.0f, y);
                    dungeonRooms[populatedCells].Type = RoomType.Start;
                    ++populatedCells;
                }
            }
        }

        dungeonRooms[populatedCells - 1].Type = RoomType.End;

        int floorIndex = Random.Range(0, floors.Length);
        for (int i = 0; i < populatedCells; ++i)
        {
            //float val = Mathf.PerlinNoise(seedOffset.x + dungeonRooms[i].coord.x / gridCount,
            //    seedOffset.y + dungeonRooms[i].coord.z / gridCount);

            //int floorIndex = Random.Range(0, floors.Length);
            dungeonRooms[i].floorMaterial = floors[floorIndex]
                     .GetComponent<MeshRenderer>()
                     .sharedMaterial;

            dungeonRooms[i].roomItems = items;
            dungeonRooms[i].roomWalls = walls;
            dungeonRooms[i].size = roomSize;
            dungeonRooms[i].rowsCount = roomRowsCount;
            dungeonRooms[i].collumnsCount = roomCollumnsCount;
            dungeonRooms[i].roomItemsPadding = roomItemsPadding;
            dungeonRooms[i].seed = seed;
            dungeonRooms[i].Generate(this.gameObject, seedOffset);

            GameObject specialItem;
            if (i == populatedCells - 1)
            {
                specialItem = dungeonRooms[i].AddSpecialItem(endOfLevelTrigger, new Vector3(roomSize / 2.0f, 0.0f, roomSize / 2.0f));
            }
            else if (i == 0)
            {
                specialItem = dungeonRooms[i].AddSpecialItem(SpawnPoint, new Vector3(roomSize / 2.0f, 0.0f, roomSize / 2.0f));
            }
            else
            {
                specialItem = dungeonRooms[i].AddSpecialItem(SpawnPoint, new Vector3(roomSize / 2.0f, 0.0f, roomSize / 2.0f));
            }

            specialItem.SetActive(true);
        }

        //generated = true;
    }

    // taken from https://en.wikipedia.org/wiki/Hilbert_curve
    //convert (x,y) to d
    private int xy2d(int n, int x, int y)
    {
        int rx, ry, s, d = 0;
        for (s = n / 2; s > 0; s /= 2)
        {
            rx = (x & s) > 0 ? 1 : 0;
            ry = (y & s) > 0 ? 1 : 0;
            d += s * s * ((3 * rx) ^ ry);
            rot(s, ref x, ref y, rx, ry);
        }
        return d;
    }

    //convert d to (x,y)
    private void d2xy(int n, int d, out int x, out int y)
    {
        int rx, ry, s, t = d;
        x = y = 0;
        for (s = 1; s < n; s *= 2)
        {
            rx = 1 & (t / 2);
            ry = 1 & (t ^ rx);
            rot(s, ref x, ref y, rx, ry);
            x += s * rx;
            y += s * ry;
            t /= 4;
        }
    }

    //rotate/flip a quadrant appropriately
    private void rot(int n, ref int x, ref int y, int rx, int ry)
    {
        if (ry == 0)
        {
            if (rx == 1)
            {
                x = n - 1 - x;
                y = n - 1 - y;
            }

            //Swap x and y
            int t = x;
            x = y;
            y = t;
        }
    }
}
