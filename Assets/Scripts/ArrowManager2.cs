using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowManager2 : MonoBehaviour, IRadialChoice
{
    [SerializeField] private GameObject arrow;
    [SerializeField] private float arrowSpeedMultiplier = 40f;

    private Rigidbody arrowRb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arrowRb = arrow.GetComponent<Rigidbody>();
    }

    public void ShootArrow(Vector3 firstHandPosition, Vector3 secondHandPosition, float triggerReleaseDelay)
    {
        if (arrow == null) return;
        Debug.Log("Shoot!");
        if (triggerReleaseDelay > 0) arrow.SetActive(true);
        if (arrowRb != null && arrow.activeSelf)
        {
            // Enable physics for flight
            arrowRb.isKinematic = false;

            // Calculate launch speed proportional to the distance between the two hands
            float drawDistance = Vector3.Distance(firstHandPosition, secondHandPosition);
            float finalSpeed = drawDistance * arrowSpeedMultiplier;
            Debug.Log($"drawDistance: {drawDistance}, finalSpeed: {finalSpeed}");

            // Propel the arrow forward
            arrowRb.linearVelocity = arrow.transform.forward * finalSpeed;
        }
    }

    public void SelectChoice(int choice)
    {
        print($"{gameObject.name} receive choice {choice}");
        if (arrow == null) return;

        MeshRenderer renderer = arrow.GetComponentInChildren<MeshRenderer>();
        if (renderer == null || renderer.material == null)
        {
            Debug.Log($"no renderer or material in {gameObject.name}");
            return;
        }

        renderer.material.color = choice switch
        {
            0 => Color.red,
            1 => Color.green,
            2 => Color.blue,
            3 => Color.purple,
            _ => renderer.material.color // Default: keep current color if choice is out of range
        };
    }

    public void SetArrowPose(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }

    public Transform GetArrowTransform() => transform;
    public Vector3 GetArrowPos() => transform.position;
    public Quaternion GetArrowRot() => transform.rotation;
}
