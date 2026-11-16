using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public interface IRadialChoice
{
    public void SelectChoice(int choice);
}

public class RadialMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] bool debug = false;
    [Range(3, 8)]
    [SerializeField] private int n = 4;
    [SerializeField] private float gapAngle;
    [SerializeField] private float holdDuration = 1f;
    [SerializeField] private ContinuousMoveProvider moveProvider;

    [Header("Item")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemParent;
    private GameObject[] itemArray;

    [Header("ProgressBar")]
    [SerializeField] private GameObject progressPrefab;
    [SerializeField] private Transform progressParent;
    private GameObject[] progressArray;
    private Image[] progressImages;

    [Header("Input")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private InputActionProperty radialActivator;
    [SerializeField] private InputActionProperty leftStick;

    [Header("Choice")]
    [Tooltip("Only GameObject that implements IRadialChoice interface")]
    [SerializeField] private GameObject choiceObject;
    private IRadialChoice choice; // Interface

    private bool isRadialActive = false;
    private Vector2 stickDirection = Vector2.zero;
    private float fillMax;
    private float selectTimer = 0f;
    private int selectedIndex = -1;

    void Start()
    {
        if (!debug)
        {
            itemParent.gameObject.SetActive(isRadialActive);
            progressParent.gameObject.SetActive(isRadialActive);
        }
        isRadialActive = debug;
        if (choiceObject != null) choice = choiceObject.GetComponent<IRadialChoice>();
        //print($"{GetType().Name} in {gameObject.name} has choice = {choice}");

        itemArray = new GameObject[n];

        if (n <= 4) gapAngle += 5f;
        fillMax = 1f / n - gapAngle / 360f;

        itemArray = SpawnRadial(itemPrefab, itemParent);
        progressArray = SpawnRadial(progressPrefab, progressParent);

        for (int i = 0; i < n; i++) itemArray[i].GetComponent<Image>().fillAmount = fillMax;

        progressImages = new Image[n];
        for (int i = 0; i < n; i++)
        {
            progressImages[i] = progressArray[i].GetComponent<Image>();
            progressImages[i].fillAmount = 0f;
        }
    }

    GameObject[] SpawnRadial(GameObject prefab, Transform parent)
    {
        int existingCount = parent.childCount;
        GameObject[] array = new GameObject[n];

        for (int i = 0; i < n; i++)
        {
            GameObject radialItem;

            if (i < existingCount)
            {
                // Reuse existing child placed in Edit mode
                radialItem = parent.GetChild(i).gameObject;
                radialItem.SetActive(true);
            }
            else
            {
                // Instantiate only if existing children are fewer than n
                radialItem = Instantiate(prefab, parent);
            }

            float angle = 360f / n * i;
            radialItem.transform.localEulerAngles = new Vector3(0, 0, angle);

            array[i] = radialItem;
        }

        // Destroy any leftover children if Edit mode had more than n
        for (int i = existingCount - 1; i >= n; i--)
        {
            Transform extra = parent.GetChild(i);
            extra.SetParent(null); // Detach immediately so childCount updates right away
            Destroy(extra.gameObject);
        }

        Vector3 rot = parent.localEulerAngles;
        parent.localEulerAngles = new Vector3(rot.x, rot.y, rot.z - (gapAngle - 360f / n)/2f );

        return array;
    }

    private void OnEnable()
    {
        radialActivator.action.performed += ActivateRadialOn;
        radialActivator.action.canceled += ActivateRadialOff;
    }

    private void OnDisable()
    {
        radialActivator.action.performed -= ActivateRadialOn;
        radialActivator.action.canceled -= ActivateRadialOff;
    }

    void ActivateRadialOn(InputAction.CallbackContext ctx)
    {
        isRadialActive = true;
        SwitchMoveAndRadial();
    }
    void ActivateRadialOff(InputAction.CallbackContext ctx)
    {
        isRadialActive = false;
        SwitchMoveAndRadial();
    }

    void SwitchMoveAndRadial()
    {
        itemParent.gameObject.SetActive(isRadialActive);
        progressParent.gameObject.SetActive(isRadialActive);
        moveProvider.enabled = !isRadialActive;
    }

    private void Update()
    {
        if (!isRadialActive) return;

        transform.position = leftHand.position + 0.8f * leftHand.forward;
        Vector3 rot = Quaternion.LookRotation(leftHand.forward, leftHand.up).eulerAngles;
        transform.rotation = Quaternion.Euler(rot.x, rot.y, 0f);

        if (leftStick != null && leftStick.action != null)
        {
            stickDirection = leftStick.action.ReadValue<Vector2>();
            if (debug) stickDirection = ReadInputFromKeyboard();
            //print($"stickDirection: {stickDirection}, selectTimer: {selectTimer}");

            if (stickDirection.sqrMagnitude > 0.45f) // 0.5 deadzone
            {
                float angle = Mathf.Atan2(stickDirection.y, stickDirection.x) * Mathf.Rad2Deg - 90f;
                if (angle < 0f) angle += 360f;

                int newIdx = Mathf.Clamp((int)(angle / 360f * n), 0, n - 1);
                //print($"angle: {angle}, selectedIdx: {selectedIndex}, newIdx: {newIdx}");

                if (newIdx != selectedIndex)
                {
                    if (selectedIndex >= 0) progressImages[selectedIndex].fillAmount = 0f;
                    selectedIndex = newIdx;
                    selectTimer = 0f;
                }
                else
                {
                    selectTimer += Time.deltaTime;
                    progressImages[selectedIndex].fillAmount = Mathf.Lerp(0f, fillMax, selectTimer/holdDuration);

                    if (selectTimer >= holdDuration)
                    {
                        //print($"Radial {gameObject.name} select choice {selectedIndex}!");
                        choice.SelectChoice(selectedIndex);
                    }
                }
            }
            else
            {
                if (selectedIndex < 0) return;
                progressImages[selectedIndex].fillAmount = 0f;
                selectTimer = 0f;
                selectedIndex = -1;
            }
        }
    }

    Vector2 ReadInputFromKeyboard()
    {
        float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
        float y = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);

        // Normalize so diagonal presses (e.g., W+D) have a magnitude of 1.0 just like a real thumbstick
        return new Vector2(x, y).normalized;
    }
}
