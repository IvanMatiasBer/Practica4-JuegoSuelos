using Unity.Netcode;
using UnityEngine;

public enum GameState : int
{
    Waiting = 0,
    Ready = 1,
    Playing = 2
}

public class GameManagerNet : NetworkBehaviour
{
    public static GameManagerNet Instance;

   
    public NetworkVariable<int> playerCount = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(
        GameState.Waiting, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
         
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        playerCount.Value++;

   
        if (playerCount.Value > 4)
        {
        
            NetworkManager.Singleton.DisconnectClient(clientId);
            playerCount.Value--;
            return;
        }

       
        if (playerCount.Value == 4)
        {
            gameState.Value = GameState.Ready;
        }
        else
        {
            gameState.Value = GameState.Waiting;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerCount.Value = Mathf.Max(0, playerCount.Value - 1);

  
        if (playerCount.Value < 4 && gameState.Value != GameState.Playing)
        {
            gameState.Value = GameState.Waiting;
        }
    }

  
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void StartGameRpc()
    {
        if (playerCount.Value == 4 && gameState.Value == GameState.Ready)
        {
            gameState.Value = GameState.Playing;
        }
    }
}
