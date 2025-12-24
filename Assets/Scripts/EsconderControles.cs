using Unity.Netcode;
using UnityEngine;

public class HideOnGameStart : NetworkBehaviour
{
    [SerializeField] private GameObject canvasToHide;

   
    public void HideCanvas()
    {
        HideCanvasClientRpc();
    }

    [ClientRpc]
    private void HideCanvasClientRpc()
    {
        if (canvasToHide != null)
            canvasToHide.SetActive(false);
    }
}

