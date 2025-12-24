using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HostUI : NetworkBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text statusText;

    [Header("GameObject a ocultar al iniciar")]
    [SerializeField] private GameObject objectToHide;

    private void Awake()
    {
        
        rootPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        
        if (!IsServer)
        {
            rootPanel.SetActive(false);
            return;
        }

        rootPanel.SetActive(true);

        var gm = GameManagerNet.Instance;

        gm.gameState.OnValueChanged += OnGameStateChanged;
        gm.playerCount.OnValueChanged += OnPlayerCountChanged;

        startButton.onClick.AddListener(OnStartClicked);

        UpdateUI(gm.gameState.Value, gm.playerCount.Value);
    }

    private void OnDestroy()
    {
        if (!IsServer) return;
        if (GameManagerNet.Instance == null) return;

        GameManagerNet.Instance.gameState.OnValueChanged -= OnGameStateChanged;
        GameManagerNet.Instance.playerCount.OnValueChanged -= OnPlayerCountChanged;
    }

    private void OnGameStateChanged(GameState prev, GameState current)
    {
        UpdateUI(current, GameManagerNet.Instance.playerCount.Value);

       
        if (current == GameState.Playing)
        {
            rootPanel.SetActive(false);
        }
    }

    private void OnPlayerCountChanged(int prev, int current)
    {
        UpdateUI(GameManagerNet.Instance.gameState.Value, current);
    }

    private void UpdateUI(GameState state, int players)
    {
        if (state == GameState.Waiting)
        {
            rootPanel.SetActive(true);
            statusText.text = $"Esperando jugadores ({players}/4)";
            startButton.interactable = false;
        }
        else if (state == GameState.Ready)
        {
            rootPanel.SetActive(true);
            statusText.text = "4 jugadores conectados";
            startButton.interactable = true;
        }
        else if (state == GameState.Playing)
        {
            startButton.interactable = false;
        }
    }

    private void OnStartClicked()
    {
        
        if (objectToHide != null)
        {
            HideObjectClientRpc();
        }

        
        GameManagerNet.Instance.StartGameRpc();
    }

    [ClientRpc]
    public void HideObjectClientRpc()
    {
        if (objectToHide != null)
        {
            objectToHide.SetActive(false);
        }
    }
}

