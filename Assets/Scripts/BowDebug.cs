using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BowDebug : MonoBehaviour
{
    [SerializeField] private InputActionProperty toggleButton;
    public bool enableDebug = false;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip lGrab;
    [SerializeField] private AudioClip rGrab;
    [SerializeField] private AudioClip debugOn;
    [SerializeField] private AudioClip debugOff;

    [Header("Arrow")]
    [SerializeField] private GameObject arrow;
    [SerializeField] private Vector2 dis_mul;
    private Pose startPos;
    private Coroutine arrowCountdownCoroutine;

    private void Start()
    {
        startPos = new Pose(arrow.transform.position, arrow.transform.rotation);
        arrow.SetActive(false);
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (!enableDebug) return;

        Debug.Log("Hover entered on bow.");
        if (hoverClip != null)
            AudioSource.PlayClipAtPoint(hoverClip, transform.position);
    }

    public void OnFirstHandGrabbed(SelectEnterEventArgs args)
    {
        if (!enableDebug) return;

        // Record the very first hand that grabs the bow
        IXRSelectInteractor primaryHand = args.interactorObject;
        if (primaryHand.handedness == InteractorHandedness.Left)
        {
            Debug.Log("Left hand grabbed the bow first.");
            if (lGrab != null)
                AudioSource.PlayClipAtPoint(lGrab, transform.position);
        }
        else if (primaryHand.handedness == InteractorHandedness.Right)
        {
            Debug.Log("Right hand grabbed the bow first.");
            if (rGrab != null)
                AudioSource.PlayClipAtPoint(rGrab, transform.position);
        }
    }

    private void ToggleActionPerformed(InputAction.CallbackContext context)
    {
        enableDebug = !enableDebug;
        Debug.Log($"Toggle button pressed. Debug mode is now {(enableDebug ? "enabled" : "disabled")}");
        if (enableDebug && debugOn != null)
        {
            AudioSource.PlayClipAtPoint(debugOn, transform.position);
        }
        else if (!enableDebug && debugOff != null)
        {
            AudioSource.PlayClipAtPoint(debugOff, transform.position);
        }
    }

    private void ShootArrow()
    {
        print("Debug Shoot!");
        arrow.SetActive(true);
        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
        arrowRb.isKinematic = true;
        arrow.transform.SetPositionAndRotation(startPos.position, startPos.rotation);
        arrowRb.isKinematic = false;
        float finalSpeed = dis_mul.x * dis_mul.y;
        Debug.Log($"drawDistance: {dis_mul.x}, finalSpeed: {finalSpeed}");

        // Propel the arrow forward
        arrowRb.linearVelocity = arrow.transform.forward * finalSpeed;
        if (arrowCountdownCoroutine!=null) StopCoroutine(arrowCountdownCoroutine);
        arrowCountdownCoroutine = StartCoroutine(CountdownBeforeDisableArrow());

    }

    IEnumerator CountdownBeforeDisableArrow()
    {
        yield return new WaitForSeconds(5f);
    }

    private void OnEnable()
    {
        if (toggleButton != null)
        {
            toggleButton.action.performed += ToggleActionPerformed;
        }
    }

    private void OnDisable()
    {
        if (toggleButton != null)
        {
            toggleButton.action.performed -= ToggleActionPerformed;
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ShootArrow();
    }

    private void FixedUpdate()
    {
        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
        // Rotate the arrow tip to follow its parabolic flight trajectory while airborne
        if (arrowRb != null && !arrowRb.isKinematic && arrowRb.linearVelocity.sqrMagnitude > 0.1f)
        {
            arrow.transform.rotation = Quaternion.LookRotation(arrowRb.linearVelocity);
        }
    }
}
