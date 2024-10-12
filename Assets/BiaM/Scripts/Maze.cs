using System.Collections;
using System.Diagnostics.CodeAnalysis;
using NaughtyAttributes;
using ProceduralToolkit;
using UnityEngine;
using PT = ProceduralToolkit.Samples;

namespace BiaM
{
    public class Maze : Mirror.NetworkBehaviour
    {
        [SerializeField, Header("General")] private Room roomPrefab;
        [SerializeField, ReorderableList] private Vector2Int[] playerRooms;
        [SerializeField, Space] private PT.MazeGenerator.Config config = new();

        [SerializeField, Header("Color")] private Color mainColor = Color.yellow;
        [SerializeField] private bool useGradient = true;
        [SerializeField] private float gradientSaturation = 0.7f;
        [SerializeField] private float gradientSaturationOffset = 0.1f;
        [SerializeField] private float gradientValue = 0.7f;
        [SerializeField] private float gradientValueOffset = 0.1f;
        [SerializeField] private float gradientLength = 30;

        [SerializeField, Header("Network"), Mirror.ReadOnly, Mirror.SyncVar(hook = nameof(OnStateChanged))]
        private int seed;

        private PT.MazeGenerator _mazeGenerator;
        private Room[,] _rooms;

        private void Awake()
        {
            config.drawEdge = DrawEdge;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            seed = Random.Range(int.MinValue, int.MaxValue);
            Random.InitState(seed);
            Generate();

            Debug.Log($"Server started: {seed}");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            Random.InitState(seed);
            Generate();

            Debug.Log($"Client started: {seed}");
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private void OnStateChanged(int oldSeed, int newSeed)
        {
            Random.InitState(seed);
            Generate();

            Debug.Log($"On state changed: {seed}");
        }

        private void Generate()
        {
            StopAllCoroutines();

            DestroyRooms();
            CreateRooms();
            _mazeGenerator = new PT.MazeGenerator(config);

            StartCoroutine(GenerateCoroutine());
        }

        private void DestroyRooms()
        {
            if (_rooms == null) return;

            foreach (var room in _rooms)
                Destroy(room?.gameObject);
        }

        private void CreateRooms()
        {
            var width = config.width;
            var height = config.height;

            _rooms = new Room[width, height];
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var newRoom = Instantiate(roomPrefab, transform);
                newRoom.MazeWidth = width;
                newRoom.MazeHeight = height;
                newRoom.Position = new Vector2Int(x, y);
                newRoom.Walls = Directions.All;
                newRoom.HasHole = newRoom.Position == config.origin;

                _rooms[x, y] = newRoom;
            }
        }

        private IEnumerator GenerateCoroutine()
        {
            while (_mazeGenerator.Generate(steps: 200))
                yield return null;

            // foreach (var playerRoom in playerRooms)
            // {
            //     var playerPosition = _rooms[playerRoom.x, playerRoom.y].transform.position;
            //     Instantiate(ballPrefab, playerPosition, Quaternion.identity);
            // }
        }

        private void DrawEdge(PT.Maze.Edge edge)
        {
            var originPosition = edge.origin.position;
            var exitPosition = edge.exit.position;
            var originConnections = edge.origin.connections;
            var exitConnections = edge.exit.connections;

            var origin = _rooms[originPosition.x, originPosition.y];
            origin.Walls = origin.Walls.RemoveFlag(originConnections);
            origin.Walls = origin.Walls.RemoveFlag(exitConnections.Inverse());
            origin.Color = GetColor(edge.origin);

            var exit = _rooms[exitPosition.x, exitPosition.y];
            exit.Walls = exit.Walls.RemoveFlag(exitConnections);
            exit.Walls = exit.Walls.RemoveFlag(originConnections.Inverse());
            exit.Color = GetColor(edge.exit);
        }

        private Color GetColor(PT.Maze.Vertex vertex)
        {
            if (!useGradient)
                return GetColor(0.75f);

            if (vertex.position == config.origin)
                return GetColor(1f);

            var gradient01 = Mathf.Repeat(vertex.depth / gradientLength, 1);
            var gradient010 = Mathf.Abs((gradient01 - 0.5f) * 2);
            return GetColor(gradient010);
        }

        private Color GetColor(float gradientPosition)
        {
            var saturation = gradientPosition * gradientSaturation + gradientSaturationOffset;
            var value = gradientPosition * gradientValue + gradientValueOffset;

            var colorHSV = new ColorHSV(mainColor);
            return colorHSV.WithSV(saturation, value).ToColor();
        }
    }
}