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
    //[SerializeField] private Animator bowAnimator;
    public BowAnimationAndSound bowAnim;

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
                print($"Pull distance: {pullDistance}, Pull percent: {pullPercent}");
                bowAnim.FreezeAnimationAtTargetPercent(pullPercent);
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
        //bowAnimator.enabled = false; // Disable the animator at the start
        //bowAnimator.speed = 0f; // Pause the animator at the start

        if (grabInteractable == null) grabInteractable = GetComponent<XRGrabInteractable>();
        if (bowAnim == null) bowAnim = GetComponent<BowAnimationAndSound>();
    }

    public void OnFirstHandGrabbed(SelectEnterEventArgs args)
    {
        // Record the very first hand that grabs the bow
        primaryHand = args.interactorObject;
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
                StartCoroutine(CooldownCoroutine());
            }
            else
            {
                pullPercent = 0f;
                bowString.position = startPos.position;
                bowAnim.FreezeAnimationAtTargetPercent(0f);
            }
        }
    }

    //void FreezeAnimationAtTargetPercent(float targetPercent)
    //{
    //    if (bowAnimator == null) return;
    //    //bowAnimator.enabled = true;
    //    // 1. Get information about the clips currently playing on the default layer (layer index 0)
    //    AnimatorClipInfo[] clipInfo = bowAnimator.GetCurrentAnimatorClipInfo(0);
    //    if (clipInfo.Length > 0)
    //    {
    //        // 2. Extract the actual AnimationClip object
    //        AnimationClip currentClip = clipInfo[0].clip;
    //        // 3. Calculate total frames automatically (Length in seconds * Frames Per Second)
    //        float totalFrames = currentClip.length * currentClip.frameRate;
    //        //float totalFrames = 52f; // Hardcoded to 52 frames for the pull animation
    //        // 4. Ensure totalFrames is greater than 0 to prevent division by zero errors
    //        if (totalFrames > 0f)
    //        {
    //            print($"Freezing animation at percent {targetPercent * 100f}% at {targetPercent * totalFrames} frames");
    //            // 5. Get the hash ID of the currently playing state
    //            int currentStateHash = bowAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    //            // 6. Force the animator to jump to the calculated normalized time
    //            bowAnimator.Play(currentStateHash, 0, targetPercent);
    //            // 7. Pause the animator to freeze it at the target frame
    //            bowAnimator.speed = 0f;
    //            bowAnimator.Update(0f); // Force an immediate update to apply the changes
    //        }
    //    }
    //}

    //void AnimateBowOnSecondHandRelease()
    //{
    //    //FreezeAnimationAtTargetFrame(52f);
    //    if (bowAnimator != null)
    //    {
    //        //bowAnimator.enabled = true;
    //        bowAnimator.SetTrigger("Release");
    //        bowAnimator.speed = 1f; // Resume the animator to play the release animation
    //    }
    //    StartCoroutine(CooldownCoroutine());
    //}

    IEnumerator CooldownCoroutine()
    {
        finishedCooldown = false;
        yield return new WaitForSeconds(cooldown);
        //bowAnimator.enabled = false;
        finishedCooldown = true;
        //bowAnimator.speed = 0f; // Pause the animator after the cooldown
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
}
