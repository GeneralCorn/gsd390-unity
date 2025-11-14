using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    /** FEATURE (Static): Shared counter so all instances of this script
    can know can track active players**/
    public static int PlayerCount = 0;

    /** FEATURE (Attribute): [SerializeField] Used so that player rotation speed can be easily 
    from the inspector **/
    [SerializeField] private float rotSpeed = 90f;

    /** FEATURE (Attribute): Exposes player move speed for arrow key control **/
    [SerializeField] private float moveSpeed = 3f;

    /** FEATURE (Attribute): Exposes list of possible start body colors for the player to
    randomly select at play **/
    [SerializeField] private Color[] possibleColors;

    /** FEATURE (Property): Wraps the rotation speed so other code
    can read it, but only this class can change the value. **/
    public float RotationSpeed
    {
        get => rotSpeed;
        private set => rotSpeed = value;
    }

    private Renderer _renderer;

    private void Awake()
    {
        PlayerCount++;
        Debug.Log($"[PlayerController] Awake: there are now {PlayerCount} Player(s) using this script.");
    }

    private void Start()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer != null && possibleColors != null && possibleColors.Length > 0)
        {
            Color currColor = GetRandomElement(possibleColors);
            _renderer.material.color = currColor;
            Debug.Log($"[PlayerController]: The color has now been switched to {currColor}");
        }

        Debug.Log($"[PlayerController]: Rotation Speed: {rotSpeed}, moveSpeed: {moveSpeedcd}");
    }

    private void Update()
    {
        transform.Rotate(0f, RotationSpeed * Time.deltaTime, 0f);
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.downArrowKey.isPressed) vertical -= 1f;
        if (keyboard.upArrowKey.isPressed) vertical += 1f;

        Vector3 move = new Vector3(horizontal, 0f, vertical);

        if (move.sqrMagnitude > 0f)
        {
            move = move.normalized * moveSpeed * Time.deltaTime;
            transform.Translate(move, Space.World);
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            RotationSpeed = -RotationSpeed;
            Debug.Log($"[PlayerController] Toggled rotation direction. New RotationSpeed = {RotationSpeed}");
        }
    }

    // FEATURE (Generics): Generic helper for random element selection (but for now just for color picking)
    private T GetRandomElement<T>(T[] array)
    {
        int index = Random.Range(0, array.Length);
        return array[index];
    }

    private void OnDestroy()
    {
        PlayerCount--;
    }
}
