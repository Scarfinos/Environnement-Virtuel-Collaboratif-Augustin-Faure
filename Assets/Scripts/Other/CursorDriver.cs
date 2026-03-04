using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class CursorDriver : NetworkBehaviour {
    private bool active ;
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
            theCamera = (Camera)GameObject.FindFirstObjectByType (typeof(Camera)) ;    
            active = false ;
        }
    }

   // Update is called once per frame
    void Update () {
        if (HasAuthority && IsSpawned) {
            if (Input.GetKeyDown (KeyCode.LeftAlt)) {
                active = true ;
            }

            if (Input.GetKeyUp (KeyCode.LeftAlt)) {
                active = false ;
            }

            if ((Input.mousePosition != null) && (active)) {
                Vector3 point = new Vector3 () ;
                Vector3 mousePos = Input.mousePosition ;
                float deltaZ = Input.mouseScrollDelta.y / 10.0f ;
                transform.Translate (0, 0, deltaZ) ;
                point = theCamera.ScreenToWorldPoint (new Vector3 (mousePos.x, mousePos.y, transform.localPosition.z)) ;
                transform.position = point ;
            }              
        }

        if (Input.GetKeyDown (KeyCode.O)) {
            Debug.Log ("Spawn requested") ;
            var myNewCube = Instantiate (ObjectToCreate) ;
            var myNetworkedNewCube = myNewCube.GetComponent<NetworkObject> () ;
                
            Vector3 newPosition = transform.position + transform.forward * 2.0f + transform.up * 1.0f;
            myNewCube.transform.position = newPosition ;

            Color chosenColor = rainbowColors[Random.Range(0, rainbowColors.Length)];
            var renderer = myNewCube.GetComponent<Renderer>();
            if (renderer != null) {
                renderer.material.color = chosenColor;
            }

            myNetworkedNewCube.Spawn () ;
        }
    }
}