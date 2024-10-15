using Mirror;
using ProceduralToolkit;
using TMPro;
using UnityEngine;

namespace BiaM
{
    [RequireComponent(typeof(PredictedRigidbody))]
    public class Ball : NetworkBehaviour
    {
        [SerializeField, Header("General"), Min(0f)]
        private float forceMultiplier = 1f;

        [SerializeField] private TMP_Text textPrefab;

        [SerializeField, ReadOnly, Header("Network")]
        private string playerName;

        [SerializeField, ReadOnly] private Color playerColor;

        private PredictedRigidbody _predictedRigidbody;
        private GameManager _gameManager;
        private Material _material;
        private TMP_Text _text;
        private RectTransform _textRectTransform;

        private void Awake()
        {
            _predictedRigidbody = GetComponent<PredictedRigidbody>();
            _gameManager = FindObjectOfType<GameManager>();
            _material = GetComponentInChildren<Renderer>().material;

            var mainCanvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
            _text = Instantiate(textPrefab, mainCanvas.transform);
            _textRectTransform = _text.GetComponent<RectTransform>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            playerName = $"Player {netId}";
            playerColor = Color.red;
        }

        private void Start()
        {
            _material.color = playerColor;

            _text.text = playerName;
            _text.color = playerColor;
        }

        private void Update()
        {
            var worldPoint = _predictedRigidbody.transform.TransformPoint(Vector3.zero) + Vector3.up * 2f;
            _textRectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPoint);
        }

        private void FixedUpdate()
        {
            var inputs = _gameManager.CombinedInputs;
            var force = (inputs.ToVector3XY() + Vector3.forward) * forceMultiplier;
            _predictedRigidbody.predictedRigidbody.AddForce(force);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer || !other.CompareTag("Finish")) return;

            _gameManager.FinishGame(netId);
        }

        private void OnDestroy()
        {
            Destroy(_text);
        }
    }
}