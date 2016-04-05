using UnityEngine;
using System.Collections;

namespace Generator
{
    public enum RoomType
    {
        Start,
        Corridor,
        End,
        //PosibleHiddenRoom// or bonus room
    };

    //[System.Flags]
    public enum DoorPosition
    {
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8
    };

    public class RoomGenerator
    {
        private GameObject floor;
        
        public Vector3 coord;

        public RoomType Type;

        public GameObject[] roomItems;

        public GameObject[] roomWalls;

        public Material floorMaterial;

        public int size = 10;

        public float wallHeight = 3.0f;

        public int rowsCount = 4;

        public int collumnsCount = 4;

        // TODO: rename?
        public float roomItemsPadding = 0.2f;

        public int seed = -1;

        // TODO: Rename!
        //public DoorPosition doorsPositions;
        public int doorsPositions = 0;

        public void Generate(GameObject parent, Vector2 seedOffset)
        {
            floor = CreateSimplePlane(size, size, floorMaterial, "RoomFloor");
            floor.transform.parent = parent.transform;
            floor.transform.position = coord * size;

            GameObject obj;
            Vector3 position, direction;
            Quaternion rotation;
            switch (Type)
            {
                case RoomType.Start:
                    // spawn player
                    break;
                case RoomType.Corridor:
                    // spawn zombies?
                    break;
                case RoomType.End:
                    // spawn end point
                    break;
                default:
                    break;
            }

            Vector3 wallScale = new Vector3(size, wallHeight, 1.0f);
            if (((int)doorsPositions & (int)DoorPosition.Top) == 0)
            {
                direction = new Vector3(0.0f, 0.0f, 1.0f);

                rotation = Quaternion.LookRotation(direction, Vector3.up);
                obj = (GameObject)Object.Instantiate(roomWalls[0], Vector2.zero, rotation);
                obj.transform.localScale = wallScale;
                obj.transform.parent = floor.transform;
                position = (direction + new Vector3(1.0f, 0.0f, 1.0f)) * (size / 2);
                obj.transform.localPosition = position;
                obj.SetActive(true);
            }

            if (((int)doorsPositions & (int)DoorPosition.Bottom) == 0)
            {
                direction = new Vector3(0.0f, 0.0f, -1.0f);

                rotation = Quaternion.LookRotation(direction, Vector3.up);
                obj = (GameObject)Object.Instantiate(roomWalls[0], Vector2.zero, rotation);
                obj.transform.localScale = wallScale;
                obj.transform.parent = floor.transform;
                position = (direction + new Vector3(1.0f, 0.0f, 1.0f)) * (size / 2);
                obj.transform.localPosition = position;
                obj.SetActive(true);
            }

            if (((int)doorsPositions & (int)DoorPosition.Left) == 0)
            {
                direction = new Vector3(-1.0f, 0.0f, 0.0f);

                rotation = Quaternion.LookRotation(direction, Vector3.up);
                obj = (GameObject)Object.Instantiate(roomWalls[0], Vector2.zero, rotation);
                obj.transform.localScale = wallScale;
                obj.transform.parent = floor.transform;
                position = (direction + new Vector3(1.0f, 0.0f, 1.0f)) * (size / 2);
                obj.transform.localPosition = position;
                obj.SetActive(true);
            }

            if (((int)doorsPositions & (int)DoorPosition.Right) == 0)
            {
                direction = new Vector3(1.0f, 0.0f, 0.0f);

                rotation = Quaternion.LookRotation(direction, Vector3.up);
                obj = (GameObject)Object.Instantiate(roomWalls[0], Vector2.zero, rotation);
                obj.transform.localScale = wallScale;
                obj.transform.parent = floor.transform;
                position = (direction + new Vector3(1.0f, 0.0f, 1.0f)) * (size / 2);
                obj.transform.localPosition = position;
                obj.SetActive(true);
            }

            //*
            float cellSizeX = size / (float)collumnsCount;
            float cellSizeY = size / (float)rowsCount;

            for (int m = 0; m < rowsCount; m++)
            {
                float yMin = cellSizeY * m;
                float yMax = cellSizeY * (m + 1);

                yMin += cellSizeY * roomItemsPadding;
                yMax -= cellSizeY * roomItemsPadding;
                for (int n = 0; n < collumnsCount; n++)
                {
                    float xMin = cellSizeX * n;
                    float xMax = cellSizeX * (n + 1);

                    xMin += cellSizeX * roomItemsPadding;
                    xMax -= cellSizeX * roomItemsPadding;

                    float x = Random.Range(xMin, xMax);
                    float y = Random.Range(yMin, yMax);
                    float val = Mathf.PerlinNoise(seedOffset.x + x, seedOffset.y + y);

                    if (val >= 0.0f && val < 1.0f)
                    {
                        var item = roomItems[(int)(val * roomItems.Length)];
                        obj = (GameObject)Object.Instantiate(item, Vector2.zero, Quaternion.identity);
                        obj.transform.parent = floor.transform;
                        position = new Vector3(x, 0.0f, y);
                        obj.transform.localPosition = position;
                        obj.SetActive(true);
                    }
                }
            }//*/
        }

        public GameObject AddSpecialItem(GameObject item, Vector3 roomPosition)
        {
            var newObj = (GameObject)Object.Instantiate(item, Vector3.zero, Quaternion.identity);
            newObj.transform.parent = floor.transform;
            newObj.transform.localPosition = roomPosition;
            return newObj;
        }

        private GameObject CreateSimplePlane(float width, float height, Material material, string name)
        {
            //var plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            //plane.name = name;
            // how to scale it????
            //return plane;
            Mesh mesh = new Mesh()
            {
                name = "ScriptedPlane",
                vertices = new Vector3[]
                {
                    new Vector3(0.0f, 0.0f, 0.0f),
                    new Vector3(width, 0.0f, 0.0f),
                    new Vector3(width, 0.0f, height),
                    new Vector3(0.0f, 0.0f, height)
                },
                uv = new Vector2[]
                {
                    new Vector2 (0, 0),
                    new Vector2 (0, 1),
                    new Vector2(1, 1),
                    new Vector2 (1, 0)
                },
                triangles = new int[]
                {
                    0, 2, 1,
                    0, 3, 2
                }
            };
            mesh.RecalculateNormals();

            GameObject plane = new GameObject(name);
            MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            MeshCollider meshCollider = plane.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            return plane;
        }
    }
}