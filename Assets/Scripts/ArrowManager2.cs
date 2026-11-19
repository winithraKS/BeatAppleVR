using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowManager2 : MonoBehaviour, IRadialChoice
{
    [SerializeField] private AudioClip releaseWArrow;

    [Header("Ballistics Settings")]
    public float minPullPercent = 0.3f; // Minimum draw ratio required to fire a shot
    [SerializeField] private float minLaunchSpeed = 5f;    // Launch speed right at the minimum draw threshold (m/s)
    [SerializeField] private float maxLaunchSpeed = 35f;   // Launch speed at full draw (m/s)

    [Header("Ray Settings")]
    [SerializeField] private float maxAimDistance = 30f;
    [SerializeField] private Transform arrowTip;
    [SerializeField] private LineRenderer straightAimLine;

    [Header("Ray Settings: Trajectory")]
    [SerializeField] private float launchSpeed = 20f;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int maxPoints = 50;
    [SerializeField] private float timeStep = 0.04f;
    private Vector3[] pointsBuffer;

    private Rigidbody arrowRb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arrowRb = GetComponent<Rigidbody>();
        DrawStraightAim(arrowTip.position, transform.forward);
        pointsBuffer = new Vector3[maxPoints];
        DrawTrajectory(launchSpeed);
    }

    //void Update()
    //{
    //    DrawStraightAim(arrowTip.position, transform.forward);
    //    DrawTrajectory(launchSpeed);
    //}

    public void DrawArrowRay(float pullPercent)
    {
        float spd = GetArrowSpeed(pullPercent);

        DrawStraightAim(arrowTip.position, transform.forward);
        DrawTrajectory(spd);
    }

    public void ShootArrow(float pullPercent, float triggerReleaseDelay)
    {
        Debug.Log($"Shoot! triggerReleaseDelay: {triggerReleaseDelay}");
        if (triggerReleaseDelay > 0) gameObject.SetActive(true);
        if (!gameObject.activeSelf) return;
        if (releaseWArrow != null) AudioSource.PlayClipAtPoint(releaseWArrow, transform.position);
        if (arrowRb != null)
        {
            // Enable physics for flight
            arrowRb.isKinematic = false;

            // Calculate launch speed proportional to the distance between the two hands
            float speed = GetArrowSpeed(pullPercent);
            Debug.Log($"arrow speed {speed}");

            // Propel the arrow forward
            arrowRb.linearVelocity = transform.forward * speed;
            if (straightAimLine != null) StartCoroutine(Fadeline(straightAimLine, pullPercent));
            if (trajectoryLine != null) StartCoroutine(Fadeline(trajectoryLine, pullPercent));
        }
    }

    private void DrawStraightAim(Vector3 origin, Vector3 direction)
    {
        if (straightAimLine == null) return;
        straightAimLine.enabled = true;
        SetLineAlpha(straightAimLine, 1f);

        straightAimLine.positionCount = 2;
        straightAimLine.SetPosition(0, origin);
        straightAimLine.SetPosition(1, origin + direction * maxAimDistance);
    }

    public void DrawTrajectory(float speed)
    {
        if (trajectoryLine == null) return;
        trajectoryLine.enabled = true;
        SetLineAlpha(trajectoryLine, 1f);

        Vector3 startPosition = transform.position;
        Vector3 initialVelocity = transform.forward * speed;
        Vector3 gravity = Physics.gravity;

        int validPoints = 0;

        for (int i = 0; i < maxPoints; i++)
        {
            float t = i * timeStep;
            // s_new = s + ut + (1/2)gt^2
            Vector3 currentPoint = startPosition + (initialVelocity * t) + (0.5f * t * t * gravity);

            if (i==0) currentPoint = arrowTip.position;

            pointsBuffer[validPoints++] = currentPoint;
        }

        trajectoryLine.positionCount = validPoints;
        trajectoryLine.SetPositions(pointsBuffer);
    }

    float GetArrowSpeed(float pullPercent)
    {
        if (pullPercent < minPullPercent) return 0f;
        return Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, pullPercent);
    }

    public void SelectChoice(int choice)
    {
        print($"{gameObject.name} receive choice {choice}");

        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
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
            -1 => Color.white,
            _ => renderer.material.color // Default: keep current color if choice is out of range
        };
    }

    private void FixedUpdate()
    {
        // Rotate the arrow tip to follow its parabolic flight trajectory while airborne
        if (arrowRb != null && !arrowRb.isKinematic && arrowRb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(arrowRb.linearVelocity);
        }
    }

    public void HideRay()
    {
        if (straightAimLine != null) straightAimLine.enabled = false;
        if (trajectoryLine != null) trajectoryLine.enabled = false;
    }

    private IEnumerator Fadeline(LineRenderer lr, float pullPercent)
    {
        float elapsed = 0f;
        float from = lr.startColor.a;

        // the more pull, the more time
        float fadeDuration = Mathf.Lerp(1f, 2f, pullPercent);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(from, 0f, t);

            SetLineAlpha(lr, alpha);
            yield return null;
        }

        lr.enabled = false;
        SetLineAlpha(lr, 1f);
    }

    void SetLineAlpha(LineRenderer lr, float alpha)
    {
        Color c = lr.startColor;
        c.a = alpha;
        lr.startColor = lr.endColor = c;
    }
}
