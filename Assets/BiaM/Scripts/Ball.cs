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

        private PredictedRigidbody _predictedRigidbody;
        private InputManager _inputManager;
        private Material _material;
        private TMP_Text _text;
        private RectTransform _textRectTransform;

        private void Awake()
        {
            _predictedRigidbody = GetComponent<PredictedRigidbody>();
            _inputManager = FindObjectOfType<InputManager>();
            _material = GetComponentInChildren<Renderer>().material;

            var mainCanvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();
            _text = Instantiate(textPrefab, mainCanvas.transform);
            _textRectTransform = _text.GetComponent<RectTransform>();
        }

        private void Start()
        {
            _material.color = Color.red;

            _text.text = gameObject.name;
            _text.color = Color.red;
        }

        private void Update()
        {
            var worldPoint = _predictedRigidbody.transform.TransformPoint(Vector3.zero) + Vector3.up * 2f;
            _textRectTransform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPoint);
        }

        private void FixedUpdate()
        {
            var inputs = _inputManager.CombinedInputs;
            var force = (inputs.ToVector3XY() + Vector3.forward) * forceMultiplier;
            _predictedRigidbody.predictedRigidbody.AddForce(force);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;

            Debug.Log($"{gameObject.tag} entered trigger {other.gameObject.tag}");
        }

        private void OnDestroy()
        {
            Destroy(_text);
        }
    }
}