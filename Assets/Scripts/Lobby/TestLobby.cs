using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    private Lobby _hostLobby;
    private float _heartBeatInterval;
    
    private async void Start()
    {
        await UnityServices.Instance.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In. Player ID: " + AuthenticationService.Instance.PlayerId);
        };
        
        // For now log in is anonymous. Later on upgrade to steam.
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    private async void HandleLobbyHeartbeat()
    {
        if(_hostLobby is null) { return; }
        
        _heartBeatInterval -= Time.deltaTime;
        if (_heartBeatInterval < 0f)
        {
            float heartBeatIntervalMax = 15f;
            _heartBeatInterval = heartBeatIntervalMax;
            
            await LobbyService.Instance.SendHeartbeatPingAsync(_hostLobby.Id);
        }
    }

    [ConsoleCommand("create_lobby")]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "TestLobby";
            int maxPlayers = 5;
            
            _hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);
            Debug.Log("Lobby created! " + _hostLobby.Name + " " + _hostLobby.MaxPlayers);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    [ConsoleCommand("list_lobbies", info:"Lists available lobbies")]
    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found: " + queryResponse.Results.Count);

            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
