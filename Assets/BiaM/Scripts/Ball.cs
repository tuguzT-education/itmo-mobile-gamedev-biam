using Mirror;
using Terresquall;
using UnityEngine;

namespace BiaM
{
    [RequireComponent(typeof(PredictedRigidbody))]
    public class Ball : NetworkBehaviour
    {
        [SerializeField, Min(0f)] private float forceMultiplier = 1f;

        private PredictedRigidbody _predictedRigidbody;

        private void Awake()
        {
            _predictedRigidbody = GetComponent<PredictedRigidbody>();
        }

        private void FixedUpdate()
        {
            var input = VirtualJoystick.GetAxis();
            var force = new Vector3(input.x, input.y, 1f) * forceMultiplier;
            _predictedRigidbody.predictedRigidbody.AddForce(force);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"{gameObject.tag} entered trigger {other.gameObject.tag}");
        }
    }
}