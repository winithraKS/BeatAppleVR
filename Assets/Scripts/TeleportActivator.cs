using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class TeleportActivator : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private InputActionProperty teleportActivateAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rayInteractor.enabled = false;
    }

    private void OnEnable()
    {
        teleportActivateAction.action.performed += OnTeleportActivate;
        teleportActivateAction.action.canceled += OnTeleportDeactivate;
    }

    private void OnDisable()
    {
        teleportActivateAction.action.performed -= OnTeleportActivate;
        teleportActivateAction.action.canceled -= OnTeleportDeactivate;
    }

    private void OnTeleportActivate(InputAction.CallbackContext context)
    {
        rayInteractor.enabled = true;
    }

    private void OnTeleportDeactivate(InputAction.CallbackContext context)
    {
        // Wait until the end of the frame so XRRayInteractor can complete the teleport first
        StartCoroutine(DisableRayEndOfFrame());
    }

    private IEnumerator DisableRayEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        rayInteractor.enabled = false;
    }
}
