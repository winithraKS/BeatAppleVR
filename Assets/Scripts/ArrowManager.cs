using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ArrowManager : MonoBehaviour
{
    [Header("Left Hand")]
    [SerializeField] private InputActionProperty leftTrigger;
    [SerializeField] private InputActionProperty leftPos;
    [SerializeField] private InputActionProperty leftRot;

    [Header("Right Hand")]
    [SerializeField] private InputActionProperty rightTrigger;
    [SerializeField] private InputActionProperty rightPos;
    [SerializeField] private InputActionProperty rightRot;

    [Header("Arrow & Bow References")]
    [SerializeField] private GameObject arrow_mesh;
    [SerializeField] private Transform bowNotch;
    [SerializeField] private Transform xrOrigin; // Optional: Assign XR Origin if tracking is in local space

    [Header("Shooting Settings")]
    [SerializeField] private Rigidbody arrowRb;
    [SerializeField] private float speedMultiplier = 50f;

    // Store trigger states from the previous frame to detect press/release edges
    private bool prevLeftTrigger;
    private bool prevRightTrigger;

    // Track hand assignment states
    private bool hasFirstHand;
    private bool hasSecondHand;
    private bool isFirstHandLeft;

    void Start()
    {
        arrow_mesh.SetActive(false);
        if (arrowRb == null && arrow_mesh != null)
        {
            arrowRb = GetComponent<Rigidbody>();
            arrowRb.isKinematic = true;
        }
    }

    void Update()
    {
        // 1. Read current trigger states (using IsPressed to safely handle both float and button bindings)
        bool currentLeft = leftTrigger.action != null && leftTrigger.action.IsPressed();
        bool currentRight = rightTrigger.action != null && rightTrigger.action.IsPressed();

        // 2. Assign the first hand when a trigger is newly pressed
        if (!hasFirstHand)
        {
            if (currentLeft && !prevLeftTrigger)
                SetFirstHand(true);
            else if (currentRight && !prevRightTrigger)
                SetFirstHand(false);
        }
        // 3. Assign the second hand when the opposite trigger is newly pressed
        else if (!hasSecondHand)
        {
            if (isFirstHandLeft && currentRight && !prevRightTrigger)
                hasSecondHand = true;
            else if (!isFirstHandLeft && currentLeft && !prevLeftTrigger)
                hasSecondHand = true;
        }

        // 4. Evaluate release conditions
        if (hasFirstHand)
        {
            bool isFirstHandPressing = isFirstHandLeft ? currentLeft : currentRight;
            bool isSecondHandPressing = hasSecondHand && (isFirstHandLeft ? currentRight : currentLeft);

            string firstHand = isFirstHandLeft ? "Left" : "Right";
            Debug.Log($"first hand: {firstHand}, isFirstHandPressing: {isFirstHandPressing},isSecondHandPressing: {isSecondHandPressing}, currentLeft: {currentLeft}, currentRight: {currentRight}");

            // Condition A: First hand released while second hand is still holding trigger -> Shoot arrow
            if (!isFirstHandPressing && hasSecondHand && isSecondHandPressing)
            {
                ShootArrow();
                ClearHands();
            }
            // Condition B: First hand released while second hand is NOT holding trigger -> Hide arrow
            else if (!isFirstHandPressing && !isSecondHandPressing)
            {
                arrow_mesh.SetActive(false);
                ClearHands();
            }
            // Condition C: Second hand released while first hand is still holding trigger -> Revert to first hand
            else if (isFirstHandPressing && hasSecondHand && !isSecondHandPressing)
            {
                hasSecondHand = false;
            }
        }

        // Cache states for the next frame
        prevLeftTrigger = currentLeft;
        prevRightTrigger = currentRight;
    }

    private void LateUpdate()
    {
        // Update arrow transform only while it is active and being held (kinematic)
        if (hasFirstHand && arrow_mesh.activeSelf && arrowRb != null && arrowRb.isKinematic)
        {
            Vector3 firstHandPosition = GetHandPosition(isFirstHandLeft);
            Quaternion firstHandRotation = GetHandRotation(isFirstHandLeft);

            if (hasSecondHand)
            {
                // Rotate arrow to point from the first hand toward the bow notch
                Vector3 aimDirection = bowNotch.position - firstHandPosition;
                if (aimDirection.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(aimDirection);
                }
                //transform.position = bowNotch.position - tipOffset;
                transform.position = firstHandPosition;
            }
            else
            {
                // Single-hand hold: match position and rotation to the first hand
                transform.SetPositionAndRotation(firstHandPosition, firstHandRotation);
            }
        }
    }

    private void SetFirstHand(bool isLeft)
    {
        hasFirstHand = true;
        isFirstHandLeft = isLeft;
        hasSecondHand = false;

        arrow_mesh.SetActive(true);
        if (arrowRb != null)
        {
            // Disable physics while holding the arrow
            arrowRb.isKinematic = true;
            arrowRb.linearVelocity = Vector3.zero;
            arrowRb.angularVelocity = Vector3.zero;
        }
    }

    private void ShootArrow()
    {
        Debug.Log("Shoot!");
        if (arrowRb != null)
        {
            // Enable physics for flight
            arrowRb.isKinematic = false;
            Vector3 firstHandPosition = GetHandPosition(isFirstHandLeft);
            Vector3 secondHandPosition = GetHandPosition(!isFirstHandLeft);

            // Calculate launch speed proportional to the distance between the two hands
            float drawDistance = Vector3.Distance(firstHandPosition, secondHandPosition);
            float finalSpeed = drawDistance * speedMultiplier;

            // Propel the arrow forward
            arrowRb.linearVelocity = transform.forward * finalSpeed;
        }
    }

    private void ClearHands()
    {
        hasFirstHand = false;
        hasSecondHand = false;
    }

    private Vector3 GetHandPosition(bool isLeft)
    {
        InputActionProperty posProp = isLeft ? leftPos : rightPos;
        Vector3 localPos = (posProp.action != null) ? posProp.action.ReadValue<Vector3>() : Vector3.zero;

        // Convert tracking local space to world space if XR Origin is assigned
        return xrOrigin != null ? xrOrigin.TransformPoint(localPos) : localPos;
    }

    private Quaternion GetHandRotation(bool isLeft)
    {
        InputActionProperty rotProp = isLeft ? leftRot : rightRot;
        Quaternion localRot = (rotProp.action != null) ? rotProp.action.ReadValue<Quaternion>() : Quaternion.identity;

        // Convert tracking local rotation to world rotation if XR Origin is assigned
        return xrOrigin != null ? xrOrigin.rotation * localRot : localRot;
    }
}