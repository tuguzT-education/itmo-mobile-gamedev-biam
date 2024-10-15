using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;

namespace BiaM
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private NetworkDiscovery networkDiscovery;
        [SerializeField] private TMP_Text infoText;

        private readonly Dictionary<long, ServerResponse> _discoveredServers = new();

        public void Play()
        {
            infoText.text = "Starting game...";

            StartCoroutine(Waiter());
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            _discoveredServers[info.serverId] = info;

            StartCoroutine(Connect(info));
        }

        public void Quit()
        {
            Debug.Log("Quit");
            Application.Quit();
        }

        private IEnumerator Connect(ServerResponse info)
        {
            infoText.text = "Connecting to existing server...";
            networkDiscovery.StopDiscovery();

            yield return new WaitForSeconds(1.0f);
            NetworkManager.singleton.StartClient(info.uri);
        }

        private IEnumerator Waiter()
        {
            infoText.text = "Discovering servers...";
            _discoveredServers.Clear();

            networkDiscovery.StartDiscovery();
            yield return new WaitForSeconds(3.5f);

            if (_discoveredServers is { Count: > 0 }) yield break;
            infoText.text = "No server found, starting a new one...";
            yield return new WaitForSeconds(1.0f);

            _discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }
    }
}