using UnityEngine;

public class BowAnimationAndSound : MonoBehaviour
{
    public Animator anim;
    [SerializeField] private AudioClip releaseWoArrow;

    //Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (anim == null) return;
        anim.speed = 0f;
    }

    public void FreezeAnimationAtTargetPercent(float targetPercent)
    {
        if (anim == null) return;
        //bowAnimator.enabled = true;
        // 1. Get information about the clips currently playing on the default layer (layer index 0)
        AnimatorClipInfo[] clipInfo = anim.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            // 2. Extract the actual AnimationClip object
            AnimationClip currentClip = clipInfo[0].clip;
            // 3. Calculate total frames automatically (Length in seconds * Frames Per Second)
            float totalFrames = currentClip.length * currentClip.frameRate;
            //float totalFrames = 52f; // Hardcoded to 52 frames for the pull animation
            // 4. Ensure totalFrames is greater than 0 to prevent division by zero errors
            if (totalFrames > 0f)
            {
                //print($"Freezing animation at percent {targetPercent * 100f}% at {targetPercent * totalFrames} frames");
                // 5. Get the hash ID of the currently playing state
                int currentStateHash = anim.GetCurrentAnimatorStateInfo(0).fullPathHash;
                // 6. Force the animator to jump to the calculated normalized time
                anim.Play(currentStateHash, 0, targetPercent);
                // 7. Pause the animator to freeze it at the target frame
                anim.speed = 0f;
                anim.Update(0f); // Force an immediate update to apply the changes
            }
        }
    }

    public void AnimateBowOnSecondHandRelease()
    {
        //FreezeAnimationAtTargetFrame(52f);
        if (anim != null)
        {
            //bowAnimator.enabled = true;
            anim.SetTrigger("Release");
            anim.speed = 1f; // Resume the animator to play the release animation
        }

        //if (releaseWoArrow != null) AudioSource.PlayClipAtPoint(releaseWoArrow,transform.position);
    }

    private void OnFinishAnimateBowRelease()
    {
        anim.speed = 0f; // Pause the animator after the release animation finishes
    }
}
