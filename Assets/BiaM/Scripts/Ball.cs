using Mirror;
using ProceduralToolkit;
using UnityEngine;

namespace BiaM
{
    [RequireComponent(typeof(PredictedRigidbody))]
    public class Ball : NetworkBehaviour
    {
        [SerializeField, Header("General"), Min(0f)]
        private float forceMultiplier = 1f;

        private PredictedRigidbody _predictedRigidbody;
        private InputManager _inputManager;
        private Material _material;

        private void Awake()
        {
            _predictedRigidbody = GetComponent<PredictedRigidbody>();
            _inputManager = FindObjectOfType<InputManager>();
            _material = GetComponentInChildren<Renderer>().material;
        }

        private void Start()
        {
            _material.color = Color.red;
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
    }
}