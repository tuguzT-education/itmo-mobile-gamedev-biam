using System.Collections;
using ProceduralToolkit;
using UnityEngine;
using PT = ProceduralToolkit.Samples;

namespace BiaM
{
    public class Maze : MonoBehaviour
    {
        [SerializeField] private Room roomPrefab;
        [SerializeField, Space] private PT.MazeGenerator.Config config = new();

        private PT.MazeGenerator _mazeGenerator;
        private Room[,] _rooms;

        private void Awake()
        {
            config.drawEdge = DrawEdge;

            Generate();
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

                _rooms[x, y] = newRoom;
            }
        }

        private IEnumerator GenerateCoroutine()
        {
            while (_mazeGenerator.Generate(steps: 200))
                yield return null;
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

            var exit = _rooms[exitPosition.x, exitPosition.y];
            exit.Walls = exit.Walls.RemoveFlag(exitConnections);
            exit.Walls = exit.Walls.RemoveFlag(originConnections.Inverse());
        }
    }
}