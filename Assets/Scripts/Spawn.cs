using Unity.Netcode;
using UnityEngine;

public class JugadorSpawn : NetworkBehaviour
{
    [Header("Puntos de spawn para hasta 4 jugadores")]
    [SerializeField] Transform[] spawnPoints;   

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; 

        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

 
        int index = (int)OwnerClientId;

    
        index = index % spawnPoints.Length;

        Transform punto = spawnPoints[index];
        if (punto != null)
        {
            transform.position = punto.position;
            transform.rotation = punto.rotation;
        }
    }
}
