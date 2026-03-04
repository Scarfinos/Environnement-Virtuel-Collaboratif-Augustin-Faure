using Unity.Netcode.Components;
using UnityEngine;
using Unity.Netcode;
#if UNITY_EDITOR
using Unity.Netcode.Editor;
using UnityEditor;

[CustomEditor(typeof(NetworkedPlayerController), true)]
public class NetworkedPlayerControllerEditor : NetworkTransformEditor
{
    private SerializedProperty m_Speed;
    private SerializedProperty m_ApplyVerticalInputToZAxis;
    private SerializedProperty m_ObjectToCreate;

    public override void OnEnable()
    {
        m_Speed = serializedObject.FindProperty(nameof(NetworkedPlayerController.Speed));
        m_ApplyVerticalInputToZAxis = serializedObject.FindProperty(nameof(NetworkedPlayerController.ApplyVerticalInputToZAxis));
        m_ObjectToCreate = serializedObject.FindProperty(nameof(NetworkedPlayerController.ObjectToCreate));
        base.OnEnable();
    }

    private void DisplayPlayerCubeControllerProperties()
    {
        EditorGUILayout.PropertyField(m_Speed);
        EditorGUILayout.PropertyField(m_ApplyVerticalInputToZAxis);
        EditorGUILayout.PropertyField(m_ObjectToCreate);
    }

    public override void OnInspectorGUI()
    {
        var playerController = target as NetworkedPlayerController;
        void SetExpanded(bool expanded) { playerController.PlayerControllerPropertiesVisible = expanded; };
        DrawFoldOutGroup<NetworkedPlayerController>(playerController.GetType(), DisplayPlayerCubeControllerProperties, playerController.PlayerControllerPropertiesVisible, SetExpanded);
        base.OnInspectorGUI();
    }
}
#endif

public class NetworkedPlayerController : NetworkTransform
{
#if UNITY_EDITOR
    public bool PlayerControllerPropertiesVisible;
#endif
    public float Speed = 10;
    public bool ApplyVerticalInputToZAxis;
    private Vector3 m_Motion;
    public Vector3 cameraPositionOffset = new Vector3 (0, 1.6f, 0); // 1.6f
    public Quaternion cameraOrientationOffset = new Quaternion ();
    protected Transform cameraTransform;
    protected Camera theCamera;
    public GameObject ObjectToCreate;

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

    private void Update()
    {
        // If not spawned or we don't have authority, then don't update
        if (!IsSpawned || !HasAuthority)
        {
            return;
        }

        var x = Input.GetAxis ("Horizontal") * Time.deltaTime * 150.0f;
        var z = Input.GetAxis ("Vertical") * Time.deltaTime * 3.0f;

        transform.Rotate (0, x, 0);
        transform.Translate (0, 0, z);

        if (Input.GetKeyDown (KeyCode.P)) {
            Debug.Log ("Spawn requested");
            var myNewCube = Instantiate (ObjectToCreate);
            var myNetworkedNewCube = myNewCube.GetComponent<NetworkObject> ();

            Vector3 newPosition = transform.position + transform.forward * 5.0f + transform.up * 1.0f;
            myNewCube.transform.position = newPosition;

            Color chosenColor = rainbowColors[Random.Range(0, rainbowColors.Length)];
            myNewCube.GetComponent<NetworkedXRGrabInteractable>().CubeColor.Value = chosenColor;

            myNetworkedNewCube.Spawn();
        }
    }

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