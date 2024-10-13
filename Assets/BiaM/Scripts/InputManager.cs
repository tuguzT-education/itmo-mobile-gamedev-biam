using System.Linq;
using Mirror;
using Terresquall;
using UnityEngine;

namespace BiaM
{
    public class InputManager : NetworkBehaviour
    {
        [SerializeField, ReadOnly] private Vector2 localInputs;
        [SerializeField, ReadOnly, SyncVar] private Vector2 combinedInputs;

        private readonly SyncDictionary<uint, Vector2> _networkInputs = new();

        public Vector2 CombinedInputs => combinedInputs;

        private void Update()
        {
            if (!isClient || VirtualJoystick.GetAxisDelta() == Vector2.zero) return;

            var playerId = netIdentity.netId;
            localInputs = VirtualJoystick.GetAxis();
            CmdUpdateInput(playerId, localInputs);
        }

        [Command(requiresAuthority = false)]
        private void CmdUpdateInput(uint playerId, Vector2 input)
        {
            _networkInputs[playerId] = input;

            var result = _networkInputs.Values.Aggregate(Vector2.zero, (curr, item) => curr + item);
            result.x = Mathf.Clamp(result.x, -1f, 1f);
            result.y = Mathf.Clamp(result.y, -1f, 1f);
            combinedInputs = result;
        }
    }
}