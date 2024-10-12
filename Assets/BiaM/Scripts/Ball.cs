using System.Linq;
using Mirror;
using ProceduralToolkit;
using Terresquall;
using UnityEngine;

namespace BiaM
{
    [RequireComponent(typeof(PredictedRigidbody))]
    public class Ball : NetworkBehaviour
    {
        [SerializeField, Header("General"), Min(0f)]
        private float forceMultiplier = 1f;

        [SerializeField, Header("Network"), ReadOnly, SyncVar]
        private Vector2 combinedNetworkInputs;

        private readonly SyncDictionary<int, Vector2> _networkInputs = new();

        private PredictedRigidbody _predictedRigidbody;

        private void Awake()
        {
            _predictedRigidbody = GetComponent<PredictedRigidbody>();
        }

        private void Update()
        {
            if (isClient && VirtualJoystick.GetAxisDelta() != Vector2.zero)
            {
                CmdUpdateInput(VirtualJoystick.GetAxis());
            }

            if (!isServer) return;

            var result = _networkInputs.Values.Aggregate(Vector2.zero, (curr, item) => curr + item);
            result.x = Mathf.Clamp(result.x, -1f, 1f);
            result.y = Mathf.Clamp(result.y, -1f, 1f);
            combinedNetworkInputs = result;
        }

        [Command(requiresAuthority = false)]
        private void CmdUpdateInput(Vector2 input)
        {
            var key = NetworkClient.connection.connectionId;
            _networkInputs[key] = input;
        }

        private void FixedUpdate()
        {
            var force = (combinedNetworkInputs.ToVector3XY() + Vector3.forward) * forceMultiplier;
            _predictedRigidbody.predictedRigidbody.AddForce(force);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;

            Debug.Log($"{gameObject.tag} entered trigger {other.gameObject.tag}");
        }
    }
}