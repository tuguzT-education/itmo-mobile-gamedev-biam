using NaughtyAttributes;
using UnityEngine;
using ProceduralToolkit;

namespace BiaM
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private GameObject leftWall;
        [SerializeField] private GameObject rightWall;
        [SerializeField] private GameObject upWall;
        [SerializeField] private GameObject downWall;
        [SerializeField] private GameObject forwardWall;
        [SerializeField] private GameObject backWall;

        [SerializeField, Space, Min(0f)] private float distanceBetweenRooms;
        [SerializeField, ReadOnly] private int mazeWidth;
        [SerializeField, ReadOnly] private int mazeHeight;
        [SerializeField, ReadOnly] private Vector2Int position;
        [SerializeField, ReadOnly] private Color color;
        [SerializeField, ReadOnly, EnumFlags] private Directions walls = Directions.All;

        public int MazeWidth
        {
            get => mazeWidth;
            set
            {
                mazeWidth = value;
                ResetPosition();
            }
        }

        public int MazeHeight
        {
            get => mazeHeight;
            set
            {
                mazeHeight = value;
                ResetPosition();
            }
        }

        public Vector2Int Position
        {
            get => position;
            set
            {
                position = value;
                ResetPosition();
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                ResetColor();
            }
        }

        public Directions Walls
        {
            get => walls;
            set
            {
                walls = value;
                ResetWalls();
            }
        }

        private void Awake()
        {
            ResetPosition();
            ResetColor();
            ResetWalls();
        }

        private void ResetPosition()
        {
            var x = (position.x - (mazeWidth - 1) / 2f) * distanceBetweenRooms;
            var y = (position.y - (mazeHeight - 1) / 2f) * distanceBetweenRooms;
            transform.position = new Vector3(x, y);
        }

        private void ResetColor()
        {
            var material = forwardWall.GetComponent<Renderer>().material;
            material.color = new Color(color.r, color.g, color.b, material.color.a);
        }

        private void ResetWalls()
        {
            leftWall.SetActive(walls.HasFlag(Directions.Left) || position.x == 0);
            rightWall.SetActive(walls.HasFlag(Directions.Right) || position.x == mazeWidth - 1);
            upWall.SetActive(walls.HasFlag(Directions.Up) || position.y == mazeHeight - 1);
            downWall.SetActive(walls.HasFlag(Directions.Down) || position.y == 0);
            forwardWall.SetActive(walls.HasFlag(Directions.Forward));
            backWall.SetActive(walls.HasFlag(Directions.Back));
        }
    }
}