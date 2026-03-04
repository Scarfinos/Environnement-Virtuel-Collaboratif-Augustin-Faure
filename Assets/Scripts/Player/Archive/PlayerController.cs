using Unity.Netcode.Components;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Netcode.Editor;
using UnityEditor;

[CustomEditor(typeof(PlayerController), true)]
public class PlayerControllerEditor : NetworkTransformEditor
{
    private SerializedProperty m_Speed;
    private SerializedProperty m_ApplyVerticalInputToZAxis;

    public override void OnEnable()
    {
        m_Speed = serializedObject.FindProperty(nameof(PlayerCubeController.Speed));
        m_ApplyVerticalInputToZAxis = serializedObject.FindProperty(nameof(PlayerCubeController.ApplyVerticalInputToZAxis));
        base.OnEnable();
    }

    private void DisplayPlayerCubeControllerProperties()
    {
        EditorGUILayout.PropertyField(m_Speed);
        EditorGUILayout.PropertyField(m_ApplyVerticalInputToZAxis);
    }

    public override void OnInspectorGUI()
    {
        var playerController = target as PlayerController;
        void SetExpanded(bool expanded) { playerController.PlayerControllerPropertiesVisible = expanded; };
        DrawFoldOutGroup<PlayerCubeController>(playerController.GetType(), DisplayPlayerCubeControllerProperties, playerController.PlayerControllerPropertiesVisible, SetExpanded);
        base.OnInspectorGUI();
    }
}
#endif

public class PlayerController : NetworkTransform
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