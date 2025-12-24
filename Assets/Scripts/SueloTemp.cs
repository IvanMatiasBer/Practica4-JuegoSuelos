using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TemporizadorSueloNet : NetworkBehaviour
{
    public Material materialOriginal;
    public Material materialRojo;
    private Renderer rend;
    private bool activado = false;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (materialOriginal == null && rend != null)
            materialOriginal = rend.material;
    }

    private void OnTriggerEnter(Collider other)
    {
   
        if (!other.CompareTag("Player")) return;
        if (activado) return;

        activado = true;
    
        ActivarSueloServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivarSueloServerRpc()
    {
    
        ActivarSueloClientRpc();
    }

    [ClientRpc]
    private void ActivarSueloClientRpc()
    {
        StartCoroutine(SueloCoroutine());
    }

    private System.Collections.IEnumerator SueloCoroutine()
    {
   
        yield return new WaitForSeconds(0.2f);
        if (rend != null && materialRojo != null)
            rend.material = materialRojo;

   
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
    }
}
