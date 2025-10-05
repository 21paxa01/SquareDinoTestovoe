using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject cubePrefab;

    [SyncVar(hook = nameof(OnRunningChanged))]
    private bool isRunning;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string playerName;

    [SyncVar(hook = nameof(OnRotationChanged))]
    private float modelYRotation;

    private Text nameTag;

    void Awake()
    {
        nameTag = GetComponentInChildren<Text>();
        EnsureComponents();
    }

    void Start()
    {
        if (isLocalPlayer)
            CmdSetPlayerName(PlayerPrefs.GetString("PlayerName"));

        SetupCamera();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        HandleMovement();
        HandleInput();
    }

    #region Movement & Rotation

    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v);

        transform.Translate(move * speed * Time.deltaTime, Space.World);
        
        bool running = move.magnitude > 0.01f;
        if (running != isRunning)
            CmdSetRunning(running);

        if (animator != null)
            animator.SetBool("running", running);

        if (move.magnitude > 0.01f)
            RotateModel(move);
    }

    void RotateModel(Vector3 move)
    {
        float targetY = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;
        float smoothY = Mathf.LerpAngle(modelTransform.eulerAngles.y, targetY, rotationSpeed * Time.deltaTime);
        modelTransform.eulerAngles = new Vector3(0, smoothY, 0);
        CmdUpdateRotation(smoothY);
    }

    #endregion

    #region Input Handling

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
            CmdSpawnCube();

        if (Input.GetKeyDown(KeyCode.Space))
            CmdSendHello();
    }

    #endregion

    #region Mirror Commands & RPCs

    [Command]
    void CmdSetRunning(bool running)
    {
        isRunning = running;
    }

    void OnRunningChanged(bool _, bool running)
    {
        if (animator != null)
            animator.SetBool("running", running);
    }

    [Command]
    void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    void OnNameChanged(string _, string newName)
    {
        if (nameTag != null)
            nameTag.text = newName;
    }

    [Command]
    void CmdSendHello()
    {
        RpcReceiveHello($"Привет от {playerName}");
    }

    [ClientRpc]
    void RpcReceiveHello(string message)
    {
        Debug.Log(message);
    }

    [Command]
    void CmdSpawnCube()
    {
        if (cubePrefab == null) return;

        SpawnCubeAt(transform.position + transform.forward * 2f + Vector3.up * 1f);
    }

    void SpawnCubeAt(Vector3 position)
    {
        GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);
        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        NetworkServer.Spawn(cube);
    }

    [Command]
    void CmdUpdateRotation(float yRotation)
    {
        modelYRotation = yRotation;
    }

    void OnRotationChanged(float _, float newY)
    {
        if (isLocalPlayer) return;
        if (modelTransform != null)
            modelTransform.eulerAngles = new Vector3(0, newY, 0);
    }

    #endregion

    #region Utility Methods

    void EnsureComponents()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (modelTransform == null)
            modelTransform = transform;
    }

    void SetupCamera()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
            cam.enabled = isLocalPlayer;
    }

    #endregion
}
