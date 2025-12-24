using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerNet : NetworkBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float multiplicadorSprint = 1.8f;
    public float fuerzaSalto = 5f;
    public float gravedadExtra = 2f;

    [Header("Cámara")]
    public Camera playerCamera;
    public Camera menuCamera; 
    public float sensibilidadMouse = 5f;
    private float rotacionX = 0f;
    private float rotacionY = 20f;
    public float distanciaCamara = 5f;
    public float alturaCamara = 2f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool enSuelo = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(false);
        if (menuCamera != null)
            menuCamera.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        if (GameManagerNet.Instance != null)
            GameManagerNet.Instance.gameState.OnValueChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        if (!IsOwner) return;
        if (GameManagerNet.Instance != null)
            GameManagerNet.Instance.gameState.OnValueChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState prev, GameState current)
    {
        if (!IsOwner) return;
        if (current == GameState.Playing && playerCamera != null)
            playerCamera.gameObject.SetActive(true);
    }

    private bool CanMove()
    {
        return GameManagerNet.Instance != null && GameManagerNet.Instance.gameState.Value == GameState.Playing;
    }

    private void Update()
    {
        if (!IsOwner || !CanMove()) return;

        //WASD
        float x = 0f;
        float z = 0f;
        if (Input.GetKey(KeyCode.W)) z += 1f;
        if (Input.GetKey(KeyCode.S)) z -= 1f;
        if (Input.GetKey(KeyCode.A)) x -= 1f;
        if (Input.GetKey(KeyCode.D)) x += 1f;

        moveInput = new Vector3(x, 0f, z).normalized;

        //jump
        if (Input.GetKeyDown(KeyCode.Space) && enSuelo)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); 
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.VelocityChange);
            enSuelo = false;
        }

        //CAMARA
        if (Input.GetMouseButton(1))
        {
            rotacionX += Input.GetAxis("Mouse X") * sensibilidadMouse;
            rotacionY -= Input.GetAxis("Mouse Y") * sensibilidadMouse;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !CanMove()) return;

        //CAMARA
        float velocidadActual = velocidad;
        if (Input.GetKey(KeyCode.LeftShift))
            velocidadActual *= multiplicadorSprint;

        Vector3 camForward = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = playerCamera.transform.right;
        Vector3 direccion = (camForward * moveInput.z + camRight * moveInput.x).normalized;

        
        Vector3 nuevaVel = new Vector3(direccion.x * velocidadActual, rb.linearVelocity.y, direccion.z * velocidadActual);
        nuevaVel += Physics.gravity * (gravedadExtra - 1f) * Time.fixedDeltaTime;

        rb.linearVelocity = nuevaVel;

       
        if (direccion.sqrMagnitude > 0.01f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.fixedDeltaTime * 10f);
        }

      
        if (playerCamera != null)
        {
            Quaternion rotacion = Quaternion.Euler(rotacionY, rotacionX, 0);
            Vector3 offset = rotacion * new Vector3(0, alturaCamara, -distanciaCamara);
            playerCamera.transform.position = transform.position + offset;
            playerCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Muerte"))
        {
            
            DespawnPlayerServerRpc();
        }
    }

    [ServerRpc]
    private void DespawnPlayerServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsSpawned)
        {
            
            if (IsOwner && playerCamera != null)
                playerCamera.gameObject.SetActive(false);

         
            GetComponent<NetworkObject>().Despawn();

          
            if (IsOwner && menuCamera != null)
                menuCamera.gameObject.SetActive(true);

         
            GameEndManager endManager = FindObjectOfType<GameEndManager>();
            if (endManager != null && IsServer)
            {
                endManager.CheckForWinnerServerRpc();
            }
        
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Detecta suelo
        if (collision.contacts[0].normal.y > 0.5f)
        {
            enSuelo = true;
        }
    }
}


