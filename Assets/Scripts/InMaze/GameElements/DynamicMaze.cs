using System;
using System.Collections;
using Assets.Scripts.InMaze.Networking;
using Assets.Scripts.InMaze.Networking.Jsonify;
using Assets.Scripts.StartupScreens.MenuScreen;
using SimpleJSON;
using UnityEngine;

namespace Assets.Scripts.InMaze.GameElements
{
    public class DynamicMaze : MonoBehaviour
    {
        // Wall extra padding for better alignment
        const float WALL_EXTENSION = 0.002f;

        // RenderSize per 10 units
        [Range(1f, 10f)]
        public float renderSize;
        [NonSerialized]
        // Scale between each pair of walls
        public float wallScale;
        // Number of players per wall (Assume diameter of player = 1m)
        public int playerPerWall = 4;
        // Wall Spreading factor
        [Range(0f, 1f)]
        public float wallExtraSpread;
        // Current center ground
        public Vector3 centerPosition { private set; get; }
        public bool isMapReady { get { return _isMapReady == 0; } }

        private GameObject[,] grounds, ceilings;
        private int _isMapReady = 0;

        public static ArrayList NineGrid(float x, float z, float renderSize, MazeData.Coordinate[][] coords)
        {
            ArrayList retn = new ArrayList();

            foreach (MazeData.Coordinate[] wall in coords)
            {
                ArrayList diagCoords = new ArrayList();
                RetnWall retnElement = new RetnWall();

                float lowerX = (wall[0].X < wall[1].X) ? wall[0].X : wall[1].X;
                float lowerZ = (wall[0].Y < wall[1].Y) ? wall[0].Y : wall[1].Y;
                float upperX = (wall[0].X > wall[1].X) ? wall[0].X : wall[1].X;
                float upperZ = (wall[0].Y > wall[1].Y) ? wall[0].Y : wall[1].Y;

                for (int j = -1; j < 2 && diagCoords.Count < 4; j += 2)
                {
                    float a = (float)(wall[1].X * (wall[0].Y - (z + (renderSize * 5 * j))) - wall[0].X * (wall[1].Y - (z + (renderSize * 5 * j)))) / (wall[0].Y - wall[1].Y);
                    float b = (float)(wall[1].Y * (wall[0].X - (x + (renderSize * 5 * j))) - wall[0].Y * (wall[1].X - (x + (renderSize * 5 * j)))) / (wall[0].X - wall[1].X);

                    if (a >= x - renderSize * 5 && a <= x + renderSize * 5 &&
                        a > lowerX && a < upperX &&
                        z + renderSize * 5 * j > lowerZ && z + renderSize * 5 * j < upperZ &&
                        !(diagCoords.Count >= 2 && (float)diagCoords.ToArray()[diagCoords.Count - 2] == a && (float)diagCoords.ToArray()[diagCoords.Count - 1] == z + (renderSize * 5 * j)))
                    {

                        diagCoords.Add(a);
                        diagCoords.Add(z + (renderSize * 5 * j));
                    }

                    if (b >= z - renderSize * 5 && b <= z + renderSize * 5 &&
                        b > lowerZ && b < upperZ &&
                        x + renderSize * 5 * j > lowerX && x + renderSize * 5 * j < upperX
                        && !(diagCoords.Count >= 2 && (float)diagCoords.ToArray()[diagCoords.Count - 2] == x + (renderSize * 5 * j) && (float)diagCoords.ToArray()[diagCoords.Count - 1] == b))
                    {

                        diagCoords.Add(x + (renderSize * 5 * j));
                        diagCoords.Add(b);
                    }
                }

                //Embedded coords with diagonal
                if (wall[0].X != wall[1].X && wall[0].Y != wall[1].Y)
                {
                    if (wall[0].X >= x - renderSize * 5 && wall[0].X <= x + renderSize * 5 &&
                        wall[0].Y >= z - renderSize * 5 && wall[0].Y <= z + renderSize * 5)
                    {

                        diagCoords.Add(wall[0].X);
                        diagCoords.Add(wall[0].Y);
                    }

                    if (wall[1].X >= x - renderSize * 5 && wall[1].X <= x + renderSize * 5 &&
                        wall[1].Y >= z - renderSize * 5 && wall[1].Y <= z + renderSize * 5)
                    {

                        diagCoords.Add(wall[1].X);
                        diagCoords.Add(wall[1].Y);
                    }
                }

                // Horizontal Vertical
                if ((upperX >= x && lowerX <= x && lowerZ >= z - renderSize * 5 && lowerZ <= z + renderSize * 5 && lowerZ == upperZ ||
                     upperZ >= z && lowerZ <= z && lowerX >= x - renderSize * 5 && lowerX <= x + renderSize * 5 && lowerX == upperX) ||
                    // Embedded coord(s)
                    (wall[0].X >= x - renderSize * 5 && wall[0].X <= x + renderSize * 5 &&
                     wall[0].Y >= z - renderSize * 5 && wall[0].Y <= z + renderSize * 5 ||
                     wall[1].X >= x - renderSize * 5 && wall[1].X <= x + renderSize * 5 &&
                     wall[1].Y >= z - renderSize * 5 && wall[1].Y <= z + renderSize * 5) ||
                    // Diagonal with point touches
                    (diagCoords.Count == 4))
                {

                    if (wall[0].X == wall[1].X)
                    {
                        //Vertical Wall

                        //LocalScale
                        retnElement.localScale = new Vector3(
                            0.1f,
                            5,
                            WALL_EXTENSION +
                            Mathf.Abs(((lowerZ < z - renderSize * 5) ? z - renderSize * 5 : lowerZ) - ((upperZ > z + renderSize * 5) ? z + renderSize * 5 : upperZ))
                            );
                        //Position
                        retnElement.position = new Vector3(
                            (wall[0].X + wall[1].X) / 2,
                            2.5f,
                            (((lowerZ < z - renderSize * 5) ? z - renderSize * 5 : lowerZ) + ((upperZ > z + renderSize * 5) ? z + renderSize * 5 : upperZ)) / 2
                            );

                    }
                    else if (wall[0].Y == wall[1].Y)
                    {
                        //Horizontal Wall

                        //LocalScale
                        retnElement.localScale = new Vector3(
                            WALL_EXTENSION +
                            Mathf.Abs(((lowerX < x - renderSize * 5) ? x - renderSize * 5 : lowerX) - ((upperX > x + renderSize * 5) ? x + renderSize * 5 : upperX)),
                            5,
                            0.1f
                            );
                        //Position
                        retnElement.position = new Vector3(
                            (((lowerX < x - renderSize * 5) ? x - renderSize * 5 : lowerX) + ((upperX > x + renderSize * 5) ? x + renderSize * 5 : upperX)) / 2,
                            2.5f,
                            (wall[0].Y + wall[1].Y) / 2
                            );

                    }
                    else if (diagCoords.Count == 4)
                    {
                        //Diagonal Wall
                        float y1 = ((lowerZ < (((float)diagCoords.ToArray()[1] > (float)diagCoords.ToArray()[3]) ? (float)diagCoords.ToArray()[3] : (float)diagCoords.ToArray()[1])) ? ((float)diagCoords.ToArray()[1] > (float)diagCoords.ToArray()[3]) ? (float)diagCoords.ToArray()[3] : (float)diagCoords.ToArray()[1] : lowerZ);
                        float y2 = ((upperZ > (((float)diagCoords.ToArray()[1] < (float)diagCoords.ToArray()[3]) ? (float)diagCoords.ToArray()[3] : (float)diagCoords.ToArray()[1])) ? ((float)diagCoords.ToArray()[1] < (float)diagCoords.ToArray()[3]) ? (float)diagCoords.ToArray()[3] : (float)diagCoords.ToArray()[1] : upperZ);

                        float x1 = ((lowerX < (((float)diagCoords.ToArray()[0] > (float)diagCoords.ToArray()[2]) ? (float)diagCoords.ToArray()[2] : (float)diagCoords.ToArray()[0])) ? ((float)diagCoords.ToArray()[0] > (float)diagCoords.ToArray()[2]) ? (float)diagCoords.ToArray()[2] : (float)diagCoords.ToArray()[0] : lowerX);
                        float x2 = ((upperX > (((float)diagCoords.ToArray()[0] < (float)diagCoords.ToArray()[2]) ? (float)diagCoords.ToArray()[2] : (float)diagCoords.ToArray()[0])) ? ((float)diagCoords.ToArray()[0] < (float)diagCoords.ToArray()[2]) ? (float)diagCoords.ToArray()[2] : (float)diagCoords.ToArray()[0] : upperX);

                        //LocalScale
                        retnElement.localScale = new Vector3(
                            0.1f,
                            5,
                            WALL_EXTENSION +
                            Mathf.Sqrt(Mathf.Pow(y1 - y2, 2) + Mathf.Pow(x1 - x2, 2))
                            );
                        //Position
                        retnElement.position = new Vector3(
                            ((float)diagCoords.ToArray()[0] + (float)diagCoords.ToArray()[2]) / 2,
                            2.5f,
                            ((float)diagCoords.ToArray()[1] + (float)diagCoords.ToArray()[3]) / 2
                            );
                        //Rotation
                        retnElement.rotation = Quaternion.Euler(
                            0,
                            90 - Mathf.Rad2Deg * Mathf.Atan((float)(wall[1].Y - wall[0].Y) / (wall[1].X - wall[0].X)),
                            0
                            );
                    }
                    retn.Add(retnElement);
                }
            }
            return retn;
        }

        IEnumerator disposeWalls()
        {
            foreach (GameObject wall in GameObject.FindGameObjectsWithTag("Wall"))
            {
                try
                {
                    if (wall.transform.position.x < grounds[0, 0].transform.position.x - renderSize * 5 ||
                        wall.transform.position.x > grounds[0, 2].transform.position.x + renderSize * 5 ||
                        wall.transform.position.z > grounds[0, 0].transform.position.z + renderSize * 5 ||
                        wall.transform.position.z < grounds[2, 0].transform.position.z - renderSize * 5)
                    {

                        Destroy(wall);
                    }
                }
                catch (MissingReferenceException ex)
                {
                    // Ignored MissingReferenceException
                    ex.ToString();
                }
                yield return null;
            }
        }

        IEnumerator disposeGrounds(GameObject[,] newGrounds)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (newGrounds[i, j] == null)
                    {

                        Destroy(grounds[2 - i, 2 - j]);
                        Destroy(ceilings[2 - i, 2 - j]);

                        yield return null;
                    }
                }
            }
        }

        IEnumerator maps()
        {
            _isMapReady++;
            GameObject player = GameObject.Find("Player");
            int shiftedCenterI = -1;
            int shiftedCenterJ = -1;
            for (int i = 0; i < 3 && shiftedCenterI == -1; i++)
            {
                for (int j = 0; j < 3 && shiftedCenterI == -1; j++)
                {
                    if (i == 1 && j == 1) continue;

                    if (grounds[i, j].transform.position.x - renderSize * 5f <= player.transform.position.x && grounds[i, j].transform.position.x + renderSize * 5f >= player.transform.position.x &&
                        grounds[i, j].transform.position.z - renderSize * 5f <= player.transform.position.z && grounds[i, j].transform.position.z + renderSize * 5f >= player.transform.position.z)
                    {

                        shiftedCenterI = i;
                        shiftedCenterJ = j;
                    }
                }
            }

            if (shiftedCenterI != -1)
            {
                GameObject[,] newGrounds = new GameObject[3, 3], newCeilings = new GameObject[3, 3];

                for (int i = shiftedCenterI - 1; i < 3; i++)
                {
                    for (int j = shiftedCenterJ - 1; j < 3; j++)
                    {

                        if (i >= 0 && i <= 2 && j >= 0 && j <= 2 &&
                            i - (shiftedCenterI - 1) >= 0 && i - (shiftedCenterI - 1) <= 2 && j - (shiftedCenterJ - 1) >= 0 && j - (shiftedCenterJ - 1) <= 2)
                        {

                            newGrounds[i - (shiftedCenterI - 1), j - (shiftedCenterJ - 1)] = grounds[i, j];
                            grounds[i, j].name = "Ground" + (i - (shiftedCenterI - 1)) + (j - (shiftedCenterJ - 1));

                            newCeilings[i - (shiftedCenterI - 1), j - (shiftedCenterJ - 1)] = ceilings[i, j];
                            ceilings[i, j].name = "Ceiling" + (i - (shiftedCenterI - 1)) + (j - (shiftedCenterJ - 1));
                        }
                    }
                }

                // Dispose unwanted walls and ceilings
                StartCoroutine(disposeGrounds(newGrounds));

                for (int i = 0; i < 9; i++)
                {
                    if (newGrounds[i / 3, i % 3] == null)
                    {
                        float x = (i % 3 - 1) * (renderSize * 10f) + newGrounds[1, 1].transform.position.x;
                        float z = (1 - i / 3) * (renderSize * 10f) + newGrounds[1, 1].transform.position.z;

                        newGrounds[i / 3, i % 3] = getNewPlane(
                            x,
                            z,
                            "Ground" + (i / 3) + (i % 3)
                            );

                        yield return null;

                        newCeilings[i / 3, i % 3] = getNewPlane(
                            x,
                            z,
                            "Ceiling" + (i / 3) + (i % 3),
                            5.1f
                            );

                        yield return null;

                        StartCoroutine(createNewWalls(x, z));

                        yield return null;
                    }
                }

                grounds = newGrounds;
                ceilings = newCeilings;

                // Dispose leftover walls
                StartCoroutine(disposeWalls());
            }
            _isMapReady--;
        }

        GameObject getNewPlane(float x, float z, string name, float y = 0)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = name;

            Material material = Instantiate(
                Resources.Load(
                    "Textures/" + (y == 0 ? "Ground" : "Ceiling") + "Plane",
                    typeof(Material)
                ) as Material
            );

            // Use texture from json
            if (MazeData.present.textureReady)
                if (MazeData.present.groundTexture != null && y == 0)
                    material.mainTexture = MazeData.present.groundTexture;
                else if (MazeData.present.ceilingTexture != null && y != 0)
                    material.mainTexture = MazeData.present.ceilingTexture;

            ground.GetComponent<Renderer>().material = material;

            ground.transform.localScale = new Vector3(
                renderSize,
                0.5f,
                renderSize
                );
            ground.transform.position = new Vector3(x, y, z);

            if (y != 0)
            {
                ground.transform.rotation = Quaternion.Euler(180f, 0f, 0f);
            }

            return ground;
        }

        IEnumerator createNewWalls(float x, float z)
        {
            _isMapReady++;

            //Construction of walls in-grid
            ArrayList al = NineGrid(x, z, renderSize, MazeData.present.coordinates);

            for (int i = 0; i < al.Count; i++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Wall" + x + "/" + z + "/" + i;
                cube.tag = "Wall";

                Material material = Instantiate(
                    Resources.Load(
                        "Textures/WallPlane",
                        typeof(Material)
                    ) as Material
                );
                // Use texture from json
                if (MazeData.present.textureReady &&
                    MazeData.present.wallTexture != null)
                    material.mainTexture = MazeData.present.wallTexture;
                cube.GetComponent<Renderer>().material = material;

                cube.transform.localScale = new Vector3(
                    ((RetnWall)al.ToArray()[i]).localScale.x + wallExtraSpread,
                    ((RetnWall)al.ToArray()[i]).localScale.y,
                    ((RetnWall)al.ToArray()[i]).localScale.z + wallExtraSpread
                    );

                cube.transform.position = ((RetnWall)al.ToArray()[i]).position;

                if (((RetnWall)al.ToArray()[i]).hasRotation())
                {
                    cube.transform.rotation = ((RetnWall)al.ToArray()[i]).rotation;
                }

                //Destroy object if one point in grid only
                if (((RetnWall)al.ToArray()[i]).localScale.x != 0.1f &&
                    ((RetnWall)al.ToArray()[i]).localScale.z != 0.1f)
                {

                    Destroy(cube);
                }

                yield return null;
            }

            _isMapReady--;
        }

        void initFacing()
        {
            GameObject.Find("Player").transform.rotation = Quaternion.Euler(
                0,
                MazeData.present.facing,
                0
                );
        }

        void initSpawn()
        {
            GameObject.Find("Player").transform.position = new Vector3(
                MazeData.present.spawn.X,
                0,
                MazeData.present.spawn.Y
            );
        }

        void initLighting()
        {
            Transform lightsTrans = GameObject.Find("Lights").transform;
            for (int i = 0; i < lightsTrans.childCount; i++)
            {
                lightsTrans.GetChild(i).GetComponent<Light>().intensity =
                    MazeData.present.lighting / 10f;
            }
        }

        // Initialization before Start()
        void Awake()
        {
            // Set wallScale
            wallScale =
                1f /* 1 unit in Web Editor */ / (40f /* Pixels */ / playerPerWall);

            // Load from TransScene
            if (TransScene.Present.MazeRenderSize > 0)
                renderSize = TransScene.Present.MazeRenderSize;

            // For debugging purpose
            if (MazeData.present == null)
                new MazeData(MazeData.EMPTY);

            // Setup universal JSON object
            MazeData.present.SetCoordsScale(wallScale);

            initFacing();
            initSpawn();
            initLighting();
        }

        // Use this for initialization
        void Start()
        {
            grounds = new GameObject[3, 3];
            ceilings = new GameObject[3, 3];

            //grounds initiate from -renderSize*10,renderSize*10
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float x = (j - 1) * renderSize * 10f + GameObject.Find("Player").transform.position.x;
                    float z = (i - 1) * -renderSize * 10f + GameObject.Find("Player").transform.position.z;

                    grounds[i, j] = getNewPlane(
                        x,
                        z,
                        "Ground" + i + j
                    );

                    ceilings[i, j] = getNewPlane(
                        x,
                        z,
                        "Ceiling" + i + j,
                        5.1f
                    );

                    StartCoroutine(createNewWalls(x, z));
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isMapReady)
            {
                StartCoroutine(maps());
            }

            // Update center
            if (grounds[1, 1] != null)
                centerPosition = grounds[1, 1].transform.localPosition;
        }
    }
}