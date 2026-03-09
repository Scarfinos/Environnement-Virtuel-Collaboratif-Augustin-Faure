using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InteractableConnectingToSharedWorld : MonoBehaviour
{
    public XRSimpleInteractable xRSimpleInteractable;
    public VRConnectionManager vRConnectionManager;

    private string userName;
    private bool hasConnected = false;
    public string sessionName = "efqhclisi";

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        System.Text.StringBuilder result = new System.Text.StringBuilder();

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[UnityEngine.Random.Range(0, chars.Length)]);
        }

        return result.ToString();
    }

    void OnEnable()
    {
        userName = RandomString(8);
        xRSimpleInteractable.selectEntered.AddListener(OnFirstSelectEntered);
    }

    void OnDisable()
    {
        xRSimpleInteractable.selectEntered.RemoveListener(OnFirstSelectEntered);
    }

    private async void OnFirstSelectEntered(SelectEnterEventArgs args)
    {
        if (hasConnected)
        {
            Debug.Log("[InteractableConnectingToSharedWorld] Already connected, ignoring.");
            return;
        }

        if (!vRConnectionManager.ServicesReady())
        {
            Debug.LogWarning("[InteractableConnectingToSharedWorld] Unity Services not ready yet. Try again in a moment.");
            return;
        }
        hasConnected = true;
        Debug.Log($"[InteractableConnectingToSharedWorld] Connecting with user '{userName}' to session '{sessionName}'...");
        await vRConnectionManager.JoinSessionAsync(sessionName, userName);
    }
}