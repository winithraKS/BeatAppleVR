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
        teleportActivateAction.action.performed += OnTeleportActivate;
        //teleportActivateAction.action.canceled += OnTeleportDeactivate;
    }

    private void OnTeleportActivate(InputAction.CallbackContext context)
    {
        rayInteractor.enabled = !rayInteractor.enabled;
    }

    //private void OnTeleportDeactivate(InputAction.CallbackContext context)
    //{
    //    rayInteractor.enabled = false;
    //}

    private void Update()
    {
        if (teleportActivateAction.action.WasReleasedThisFrame())
        {
            rayInteractor.enabled = false;
        }
    }
}
