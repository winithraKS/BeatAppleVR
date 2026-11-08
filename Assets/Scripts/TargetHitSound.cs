using UnityEngine;

public class TargetHitSound : MonoBehaviour
{
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private string arrowTag = "Arrow";

    private void OnCollisionEnter(Collision collision)
    {
        if (IsArrow(collision.gameObject) && hitSound != null)
        {
            // Play the clip in 3D world space at the target's position
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
            collision.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private bool IsArrow(GameObject obj)
    {
        // If arrowTag is empty, allow any collision to trigger the sound
        if (string.IsNullOrEmpty(arrowTag)) return true;

        // Check tag on both the colliding object and its root parent
        return obj.CompareTag(arrowTag) || obj.transform.root.CompareTag(arrowTag);
    }
}
