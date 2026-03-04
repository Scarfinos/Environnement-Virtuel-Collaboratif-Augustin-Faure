using Unity.Netcode.Components;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using Unity.Netcode.Editor;

[CustomEditor(typeof(NetworkedPlayerController), true)]
public class NetworkedPlayerControllerEditor : NetworkTransformEditor
{
    private SerializedProperty m_Speed;
    private SerializedProperty m_ApplyVerticalInputToZAxis;
    private SerializedProperty m_ObjectToCreate;

    private SerializedProperty m_MoveForward;
    private SerializedProperty m_MoveBackward;
    private SerializedProperty m_TurnLeft;
    private SerializedProperty m_TurnRight;
    private SerializedProperty m_SpawnAction;

    public override void OnEnable()
    {
        m_Speed = serializedObject.FindProperty(nameof(NetworkedPlayerController.Speed));
        m_ApplyVerticalInputToZAxis = serializedObject.FindProperty(nameof(NetworkedPlayerController.ApplyVerticalInputToZAxis));
        m_ObjectToCreate = serializedObject.FindProperty(nameof(NetworkedPlayerController.ObjectToCreate));

        m_MoveForward = serializedObject.FindProperty(nameof(NetworkedPlayerController.moveForward));
        m_MoveBackward = serializedObject.FindProperty(nameof(NetworkedPlayerController.moveBackward));
        m_TurnLeft = serializedObject.FindProperty(nameof(NetworkedPlayerController.turnLeft));
        m_TurnRight = serializedObject.FindProperty(nameof(NetworkedPlayerController.turnRight));
        m_SpawnAction = serializedObject.FindProperty(nameof(NetworkedPlayerController.spawnAction));

        base.OnEnable();
    }

    private void DisplayPlayerControllerProperties()
    {
        EditorGUILayout.PropertyField(m_Speed);
        EditorGUILayout.PropertyField(m_ApplyVerticalInputToZAxis);
        EditorGUILayout.PropertyField(m_ObjectToCreate);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Input Actions", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(m_MoveForward);
        EditorGUILayout.PropertyField(m_MoveBackward);
        EditorGUILayout.PropertyField(m_TurnLeft);
        EditorGUILayout.PropertyField(m_TurnRight);
        EditorGUILayout.PropertyField(m_SpawnAction);
    }

    public override void OnInspectorGUI()
    {
        var playerController = target as NetworkedPlayerController;
        void SetExpanded(bool expanded) {playerController.PlayerControllerPropertiesVisible = expanded;};
        DrawFoldOutGroup<NetworkedPlayerController>(playerController.GetType(), DisplayPlayerControllerProperties, playerController.PlayerControllerPropertiesVisible, SetExpanded);
        base.OnInspectorGUI();
    }
}
#endif

public class NetworkedPlayerController : NetworkTransform
{
#if UNITY_EDITOR
    public bool PlayerControllerPropertiesVisible = true;
#endif
    public float Speed = 10;
    public bool ApplyVerticalInputToZAxis;
    public GameObject ObjectToCreate;

    public InputAction moveForward;
    public InputAction moveBackward;
    public InputAction turnLeft;
    public InputAction turnRight;
    public InputAction spawnAction;

    private static readonly Color[] rainbowColors = new Color[]
        {
            Color.red,
            new Color(1f, 0.5f, 0f), // orange
            Color.yellow,
            Color.green,
            Color.blue,
            new Color(0f, 0.8f, 0.8f), // indigo
            new Color(0.56f, 0f, 1f)     // violet
        };

    private void Start()
    {
        CatchCamera();
    }

    private bool isForwardPressed = false;
    private bool isBackwardPressed = false;
    private bool isTurnLeftPressed = false;
    private bool isTurnRightPressed = false;

    private void OnEnable()
    {
        moveForward.started += ctx => isForwardPressed = true;
        moveForward.canceled += ctx => isForwardPressed = false;

        moveBackward.started += ctx => isBackwardPressed = true;
        moveBackward.canceled += ctx => isBackwardPressed = false;

        turnLeft.started += ctx => isTurnLeftPressed = true;
        turnLeft.canceled += ctx => isTurnLeftPressed = false;

        turnRight.started += ctx => isTurnRightPressed = true;
        turnRight.canceled += ctx => isTurnRightPressed = false;

        moveForward.Enable();
        moveBackward.Enable();
        turnLeft.Enable();
        turnRight.Enable();
        spawnAction.Enable();
    }

    private void OnDisable()
    {
        moveForward.Disable();
        moveBackward.Disable();
        turnLeft.Disable();
        turnRight.Disable();
        spawnAction.Disable();
    }

    private void Update()
    {
        // If not spawned or we don't have authority, then don't update
        if (!IsSpawned || !HasAuthority)
        {
            return;
        }
        float forwardValue = (isForwardPressed ? 1f : 0f) + (isBackwardPressed ? -1f : 0f);
        float turnValue = (isTurnLeftPressed ? -1f : 0f) + (isTurnRightPressed ? 1f : 0f);

        transform.Rotate(0, turnValue * Speed * Time.deltaTime * 10f, 0);
        transform.Translate(0, 0, forwardValue * Speed * Time.deltaTime);

        if (spawnAction.triggered) { SpawnCube(); }
    }

    private void SpawnCube()
    {
        Debug.Log("Spawn requested");

        var myNewCube = Instantiate(ObjectToCreate);
        var myNetworkedNewCube = myNewCube.GetComponent<NetworkObject>();

        Vector3 newPosition = transform.position + transform.forward * 5f + transform.up * 1f;
        myNewCube.transform.position = newPosition;

        Color chosenColor = rainbowColors[Random.Range(0, rainbowColors.Length)];
        var renderer = myNewCube.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = chosenColor;

        myNetworkedNewCube.Spawn();
    }

    public Vector3 cameraPositionOffset = new Vector3 (0, 1.6f, 0); // 1.6f
    public Quaternion cameraOrientationOffset = new Quaternion ();
    protected Transform cameraTransform;
    protected Camera theCamera;

    public void CatchCamera () {
       if (IsSpawned && HasAuthority) {
           // attach the camera to the navigation rig
           theCamera = (Camera)GameObject.FindFirstObjectByType (typeof(Camera));
           theCamera.enabled = true;
           cameraTransform = theCamera.transform;
           cameraTransform.SetParent (transform);
           cameraTransform.localPosition = cameraPositionOffset;
           cameraTransform.localRotation = cameraOrientationOffset;
       }
  }
}