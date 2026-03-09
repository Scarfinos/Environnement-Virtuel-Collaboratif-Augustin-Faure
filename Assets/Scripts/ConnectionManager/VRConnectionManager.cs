using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class VRConnectionManager : MonoBehaviour
{
    private int _maxPlayers = 10;
    private ConnectionState _state = ConnectionState.Disconnected;
    private ISession _session;
    private NetworkManager m_NetworkManager;
    private bool _servicesReady = false;


    private enum ConnectionState
    {
       Disconnected,
       Connecting,
       Connected,
    }

    private async void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        await UnityServices.InitializeAsync();
        _servicesReady = true;
    }

    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (m_NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
        }
    }

   private void OnDestroy()
   {
       _session?.LeaveAsync();
   }

    public async Task JoinSessionAsync(String _sessionName, String _profileName)
    {
       _state = ConnectionState.Connecting;

       try
        {
           AuthenticationService.Instance.SwitchProfile(_profileName);
           await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions() {
                Name = _sessionName,
                MaxPlayers = _maxPlayers
            }.WithDistributedAuthorityNetwork();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);

            if (_session.IsHost)
            {
                Debug.Log("Starting Host...");
                m_NetworkManager.StartHost();
            }
            else
            {
                Debug.Log("Starting Client...");
                m_NetworkManager.StartClient();
            }

           _state = ConnectionState.Connected;
        }
        catch (Exception e)
        {
           _state = ConnectionState.Disconnected;
           Debug.LogException(e);
        }
    }

    public bool ServicesReady()
    {
        return this._servicesReady;
    }
}