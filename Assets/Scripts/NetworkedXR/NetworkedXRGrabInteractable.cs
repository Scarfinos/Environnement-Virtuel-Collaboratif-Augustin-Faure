using UnityEngine;
using Unity.Netcode;

public class NetworkedXRGrabInteractable : NetworkBehaviour {
    protected NetworkObject networkObject;
    private Color catchableColor = Color.grey;
    private Color caughtColor = Color.white;
    private Color initialColor;
    protected Rigidbody rb;
    protected Renderer colorRenderer;
    protected bool caught = false;

    public virtual void Start() {
        networkObject = (NetworkObject)GameObject.FindFirstObjectByType(typeof(NetworkObject));
        colorRenderer = GetComponentInChildren<Renderer>();
        initialColor = colorRenderer.material.color;
        rb = GetComponent<Rigidbody> ();
    }

    public NetworkVariable<Color> CubeColor = new NetworkVariable<Color>(
        Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        CubeColor.OnValueChanged += (oldColor, newColor) =>
        {
            colorRenderer.material.color = newColor;
        };

        colorRenderer.material.color = CubeColor.Value;
    }

    public virtual void LocalCatch() {
        print ("LocalCatch");
        if (!caught) {
            if (!HasAuthority) {
                networkObject.RequestOwnership();
            }
            Catch();
        }
    }

    public virtual void Catch() {
        print ("Catch");
        rb.isKinematic = true;
        caught = true;
        ShowCaughtRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void ShowCaughtRpc() {
        print ("ShowCaught");
        colorRenderer.material.color = caughtColor;
    }

    public virtual void LocalRelease() {
        print ("LocalRelease");
        Release();
    }

    public virtual void Release() {
        print ("Release");
        rb.isKinematic = false;
        caught = false;
        ShowReleasedRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void ShowReleasedRpc() {
        print ("ShowReleased");
        colorRenderer.material.color = catchableColor;
    }

    public void LocalShowCatchable() {
        print ("LocalShowCatchable");
        ShowCatchableRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void ShowCatchableRpc() {
        colorRenderer.material.color = catchableColor;
    }

    public void LocalHideCatchable() {
        print ("LocalHideCatchable");
        HideCatchableRpc();
    }

    [Rpc(SendTo.Everyone)]
    public void HideCatchableRpc() {
   	    colorRenderer.material.color = initialColor;
    }
}