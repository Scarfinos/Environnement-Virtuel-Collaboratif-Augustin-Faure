using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class CursorDriver : NetworkBehaviour {
    public InputAction altModifier;
    public InputAction mousePosition;
    public InputAction mouseScroll;
    public InputAction spawnAction;

    private bool active = false;
    private Camera theCamera ;
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start () {
        if (HasAuthority && IsSpawned) {
            theCamera = (Camera)GameObject.FindFirstObjectByType (typeof(Camera));    
        }
    }

    private void OnEnable()
    {
        altModifier.started += ctx => active = true;
        altModifier.canceled += ctx => active = false;

        altModifier.Enable();
        mousePosition.Enable();
        mouseScroll.Enable();
        spawnAction.Enable();
    }

    private void OnDisable()
    {
        altModifier.Disable();
        mousePosition.Disable();
        mouseScroll.Disable();
        spawnAction.Disable();
    }

   // Update is called once per frame
    private void Update()
    {
        if (!HasAuthority || !IsSpawned)
            return;

        if (spawnAction.triggered) { SpawnCube(); }

        if (!active)
            return;

        Vector2 mousePos = mousePosition.ReadValue<Vector2>();
        float  scroll = mouseScroll.ReadValue<Vector2 >().y / 10f;

        transform.Translate(0, 0, scroll);

        Vector3 worldPoint = theCamera.ScreenToWorldPoint(
            new Vector3(mousePos.x, mousePos.y, transform.localPosition.z)
        );

        transform.position = worldPoint;
    }

    private void SpawnCube()
    {
        Debug.Log("Spawn requested");

        var myNewCube = Instantiate(ObjectToCreate);
        var myNetworkedNewCube = myNewCube.GetComponent<NetworkObject>();

        Vector3 newPosition = transform.position + transform.forward * 2f + transform.up * 1f;
        myNewCube.transform.position = newPosition;

        Color chosenColor = rainbowColors[Random.Range(0, rainbowColors.Length)];
        var renderer = myNewCube.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = chosenColor;

        myNetworkedNewCube.Spawn();
    }
}