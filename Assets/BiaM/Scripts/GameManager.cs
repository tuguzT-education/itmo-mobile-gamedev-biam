using System;
using System.Linq;
using Mirror;
using Terresquall;
using UnityEngine;
using UnityEngine.Events;

namespace BiaM
{
    [Serializable]
    public class FinishGameEvent : UnityEvent<string, bool>
    {
    }

    public class GameManager : NetworkBehaviour
    {
        [SerializeField, Header("General"), ReadOnly, SyncVar]
        private bool gameFinished;

        [SerializeField] private FinishGameEvent finishGameEvent;

        [SerializeField, Header("Inputs"), ReadOnly]
        private Vector2 localInputs;

        [SerializeField, ReadOnly, SyncVar] private Vector2 combinedInputs;

        private readonly SyncDictionary<uint, Vector2> _networkInputs = new();

        public Vector2 CombinedInputs => gameFinished ? Vector2.zero : combinedInputs;

        public override void OnStartServer()
        {
            base.OnStartServer();
            gameFinished = false;
        }

        [Server]
        public void FinishGame(uint winnerId)
        {
            if (gameFinished) return;

            gameFinished = true;
            RpcFinishGame(winnerId);
        }

        [ClientRpc]
        private void RpcFinishGame(uint winnerId)
        {
            var winnerName = $"Player {winnerId}";
            Debug.Log($"{winnerName} wins!");
            finishGameEvent.Invoke(winnerName, winnerId == NetworkClient.connection.identity.netId);
        }

        private void Update()
        {
            if (!isClient || VirtualJoystick.GetAxisDelta() == Vector2.zero) return;

            var playerId = NetworkClient.connection.identity.netId;
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