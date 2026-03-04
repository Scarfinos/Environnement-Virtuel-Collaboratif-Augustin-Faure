using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InteractableConnectingToSharedWorld : MonoBehaviour
{
    public XRSimpleInteractable xRSimpleInteractable;
    public VRConnectionManager vRConnectionManager;

    private string userName = "VRHeadset";
    private bool hasConnected = false;

    [Header("Shared World Settings")]
    public string sessionName;

    void OnEnable()
    {
        xRSimpleInteractable.firstSelectEntered.AddListener(OnFirstSelectEntered);
    }

    void OnDisable()
    {
        xRSimpleInteractable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
    }

    private async void OnFirstSelectEntered(SelectEnterEventArgs args)
    {
        if (hasConnected)
        {
            return;
        }
        hasConnected = true;
        await vRConnectionManager.JoinSharedWorldAsync(userName, sessionName);
    }
}