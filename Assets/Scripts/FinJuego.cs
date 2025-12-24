using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameEndManager : NetworkBehaviour
{
    [Header("UI de fin de juego")]
    [SerializeField] public GameObject winCanvas; 
    [SerializeField] public TMP_Text winText;
    

    private void Start()
    {
        if (winCanvas != null)
            winCanvas.SetActive(false); // Por si acaso

   
    }

  
    [ServerRpc(RequireOwnership = false)]
    public void CheckForWinnerServerRpc()
    {
        int jugadoresVivos = 0;
        JugadorColor ganador = null;

        foreach (var playerObj in FindObjectsOfType<JugadorColor>())
        {
            if (playerObj.IsSpawned)
            {
                jugadoresVivos++;
                ganador = playerObj; 
            }
        }

        if (jugadoresVivos == 1 && ganador != null)
        {
            ShowWinnerClientRpc(ganador.OwnerClientId);
        }
    }

    [ClientRpc]
    private void ShowWinnerClientRpc(ulong winnerClientId)
    {
       
        var jugadores = FindObjectsOfType<JugadorColor>();
        Color colorGanador = Color.white;

        foreach (var j in jugadores)
        {
            if (j.OwnerClientId == winnerClientId)
            {
                if (j.GetComponent<Renderer>() != null)
                    colorGanador = j.GetComponent<Renderer>().material.color;
                break;
            }
        }

        if (winText != null)
            winText.text = $"¡Ha ganado {ColorToName(colorGanador)}!";

        if (winCanvas != null)
            winCanvas.SetActive(true);

 

       
        Time.timeScale = 0f;
    }

    private string ColorToName(Color c)
    {
        if (c == Color.red) return "Rojo";
        if (c == Color.green) return "Verde";
        if (c == Color.blue) return "Azul";
        if (c == Color.white) return "Blanco";
        return "Desconocido";
    }
}

