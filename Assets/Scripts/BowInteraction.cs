using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class BowInteraction : XRBaseGrabTransformer, IXRSelectFilter
{
    //[SerializeField] private InputActionProperty gripAction;
    [SerializeField] private Transform bowString;
    [SerializeField] private Transform notchPoint;
    [SerializeField] private Transform startPos;
    [SerializeField] private float cooldown = 0.5f; // Cooldown time in seconds before the bow can be pulled again
    [SerializeField] private Vector3 maxOffAxisDistance = new(0.25f,0.5f,0.5f); // Radius for the second hand to be considered "holding" the bow
    public BowAnimationAndSound bowAnim;

    [Header("Arrow")]
    [SerializeField] private GameObject arrow;
    [SerializeField] private float arrowSpeedMultiplier = 10f;
    [SerializeField] private AudioClip releaseSound;

    bool finishedCooldown = true;
    float pullPercent = 0f;
    public IXRSelectInteractor primaryHand;
    private XRGrabInteractable grabInteractable;

    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
    {
        this.grabInteractable = grabInteractable;
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed) return;
        Transform f = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable);
        //Debug.Log($"First hand position: {f.position}, rotation: {f.rotation}, hand: {grabInteractable.interactorsSelecting[0].handedness} {grabInteractable.interactorsSelecting.Count >= 2}");
        targetPose.position = f.position;
        targetPose.rotation = f.rotation;
        if (grabInteractable.interactorsSelecting.Count >= 2)
        {
            Transform s = grabInteractable.interactorsSelecting[1].GetAttachTransform(grabInteractable);

            // 1. Actual pull vector of the hand (relative to the string's start position)
            Vector3 pullVector = s.position - startPos.position;

            // 2. The straight axis of the bow (pointing from the bow towards the player)
            Vector3 bowAxis = (startPos.position - notchPoint.position).normalized;

            // 3. Use Dot Product to find the pull distance "along the axis" (in meters)
            // - Positive value (+) when pulled backward
            // - Negative value (-) when pushed forward past the startPos
            float pullDistance = Vector3.Dot(pullVector, bowAxis);

            // 4. Find the off-axis offset (deviation left/right/up/down)
            Vector3 projectedPull = bowAxis * pullDistance;
            Vector3 offsetVector = pullVector - projectedPull;

            // 5. Combine release conditions
            bool isOffAxis = offsetVector.magnitude > maxOffAxisDistance.x; // Pulled off-axis beyond the radius limit
            bool isPulledTooFar = pullDistance > maxOffAxisDistance.y;       // Pulled backward beyond the maximum distance
            bool isPushedForward = pullDistance < maxOffAxisDistance.z;               // Pushed forward past the start point by 5 cm (buffer to prevent accidental drops)

            if (isOffAxis || isPulledTooFar || isPushedForward)
            {
                // Force the second hand to release
                IXRSelectInteractor secondHandInteractor = grabInteractable.interactorsSelecting[1];
                grabInteractable.interactionManager.SelectCancel(secondHandInteractor, grabInteractable);
            }
            else
            {
                bowString.position = s.position;
                pullPercent = Mathf.Clamp01(pullDistance / maxOffAxisDistance.y);
                //print($"Pull distance: {pullDistance}, Pull percent: {pullPercent}");
                bowAnim.FreezeAnimationAtTargetPercent(pullPercent);

                if (arrow != null)
                {
                    arrow.transform.position = s.position;
                    Vector3 aimDirection = transform.position - s.position;
                    if (aimDirection.sqrMagnitude > 0.0001f) arrow.transform.rotation = Quaternion.LookRotation(aimDirection);
                }
            }
        }
        else
        {
            //targetPose.rotation = f.rotation;
            //Debug.Log($"Only one hand is holding the bow ({f.gameObject.name}). First hand position: {f.position}, rotation: {f.rotation}");
            //clip.SampleAnimation(gameObject, 0f);
        }
    }

    protected override void Start()
    {
        base.Start();

        if (grabInteractable == null) grabInteractable = GetComponent<XRGrabInteractable>();
        if (bowAnim == null) bowAnim = GetComponent<BowAnimationAndSound>();
    }

    public void OnFirstHandGrabbed(SelectEnterEventArgs args)
    {
        // Record the very first hand that grabs the bow
        primaryHand = args.interactorObject;
    }

    public void OnHandGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactorObject == primaryHand) return;
        if (arrow != null)
        {
            arrow.SetActive(true);
            arrow.transform.SetPositionAndRotation(transform.position, transform.rotation);
            arrow.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            arrow.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    public void OnAnyHandReleased(SelectExitEventArgs args)
    {
        // If the hand that just let go IS the primary hand, clear the reference
        if (args.interactorObject == primaryHand)
        {
            primaryHand = null;
        } else
        {
            // If the hand that just let go is NOT the primary hand, do nothing
            //Debug.Log($"Secondary hand released. Primary hand is still holding the bow.");
            if (pullPercent > 0.9f)
            {
                bowAnim.AnimateBowOnSecondHandRelease();
                Vector3 f = grabInteractable.interactorsSelecting[0].GetAttachTransform(grabInteractable).position;
                Vector3 s = args.interactorObject.transform.position;
                if (arrow!=null) ShootArrow(f, s);
                StartCoroutine(CooldownCoroutine());
            }
            else
            {
                pullPercent = 0f;
                bowString.position = startPos.position;
                bowAnim.FreezeAnimationAtTargetPercent(0f);

                if (arrow != null)
                {
                    arrow.SetActive(false);
                    arrow.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                }
            }
        }
    }

    IEnumerator CooldownCoroutine()
    {
        finishedCooldown = false;
        yield return new WaitForSeconds(cooldown);
        finishedCooldown = true;
    }

    // XRI calls this before allowing any interactor (hand) to grab the object
    // XRI runs this check every frame for every hand holding (or trying to hold) the object
    public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
    {
        // If the object is already selected, but primaryHand became null (meaning the 1st hand let go),
        // return false to immediately force the 2nd hand to drop the object too!
        if (interactable.isSelected && primaryHand == null) return false;
        if (interactor == primaryHand) return true; // If the interactor is the primary hand, allow the grab
        return finishedCooldown; // If the cooldown is finished, allow the grab; otherwise, deny it

    }

    private void LateUpdate()
    {
        if (grabInteractable == null) return;
        if (grabInteractable.interactorsSelecting.Count >= 2)
        {
            Transform s = grabInteractable.interactorsSelecting[1].GetAttachTransform(grabInteractable);
            bowString.position = s.position;
        }
    }

    private void ShootArrow(Vector3 firstHandPosition, Vector3 secondHandPosition)
    {
        if (arrow == null) return;
        Debug.Log("Shoot!");
        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();
        if (arrowRb != null)
        {
            // Enable physics for flight
            arrowRb.isKinematic = false;
            //Vector3 firstHandPosition = GetHandPosition(isFirstHandLeft);
            //Vector3 secondHandPosition = GetHandPosition(!isFirstHandLeft);

            // Calculate launch speed proportional to the distance between the two hands
            float drawDistance = Vector3.Distance(firstHandPosition, secondHandPosition);
            float finalSpeed = drawDistance * arrowSpeedMultiplier;
            Debug.Log($"drawDistance: {drawDistance}, finalSpeed: {finalSpeed}");

            // Propel the arrow forward
            arrowRb.linearVelocity = arrow.transform.forward * finalSpeed;
        }

        if (releaseSound != null) AudioSource.PlayClipAtPoint(releaseSound, transform.position);
    }
}
