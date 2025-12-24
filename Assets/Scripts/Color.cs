using Unity.Netcode;
using UnityEngine;

public class JugadorColor : NetworkBehaviour
{
    [Header("Renderer del jugador")]
    [SerializeField] Renderer targetRenderer;

    Color[] colores =
    {
        Color.green,   // jugador 0
        Color.red,     // jugador 1
        Color.blue,    // jugador 2
        Color.white    // jugador 3
    };

    public override void OnNetworkSpawn()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        int index = (int)OwnerClientId % colores.Length;
        Color c = colores[index];

        if (IsServer)
        {
            SetColorClientRpc(c);  
        }

        
        targetRenderer.material.color = c;
    }


    [ClientRpc]
    void SetColorClientRpc(Color c)
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
            targetRenderer.material.color = c;
    }
}
