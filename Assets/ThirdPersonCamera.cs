using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player;
    public Vector3 offset = new Vector3(1.5f, 2f, -4f); // Third person offset
    public Vector3 firstPersonOffset = new Vector3(0f, 1.7f, 0f); // First person offset
    public float rotationSpeed = 0.2f;
    public float scrollSensitivity = 2f;
    public float minDistance = 1f;
    public float maxDistance = 5f;

    [Header("Pitch Limits")]
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Header("Collision Settings")]
    public float collisionRadius = 0.3f;
    public LayerMask collisionLayers;
    public float smoothSpeed = 10f;

    private float yaw = 0f;
    private float pitch = 20f;
    private float desiredDistance;
    private float currentDistance;

    void Start()
    {
        desiredDistance = maxDistance;
        currentDistance = maxDistance;
    }

    void LateUpdate()
    {
        if (player == null || Mouse.current == null) return;

        // --- Mouse rotation ---
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        yaw += mouseDelta.x * rotationSpeed;
        pitch -= mouseDelta.y * rotationSpeed;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // --- Scroll zoom ---
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            desiredDistance -= scroll * scrollSensitivity;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }

        // Smoothly interpolate current distance
        currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * smoothSpeed);

        bool isFirstPerson = currentDistance <= minDistance + 0.01f;

        if (isFirstPerson)
        {
            // FIRST PERSON: attach to head
            transform.position = player.position + firstPersonOffset;
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            // --- Sync player body with camera yaw ---
            Vector3 bodyEuler = player.rotation.eulerAngles;
            bodyEuler.y = yaw;
            player.rotation = Quaternion.Euler(bodyEuler);
        }
        else
        {
            // THIRD PERSON
            Vector3 rotatedOffset = player.rotation * offset;
            Vector3 pivot = player.position + rotatedOffset;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPos = pivot - rotation * Vector3.forward * currentDistance;

            // Collision check
            int layerMask = collisionLayers & ~(1 << player.gameObject.layer);
            float maxDist = currentDistance;
            if (Physics.SphereCast(pivot, collisionRadius, (desiredPos - pivot).normalized, out RaycastHit hit, currentDistance, layerMask))
            {
                maxDist = hit.distance - collisionRadius;
            }

            // Smooth collision adjustment
            currentDistance = Mathf.Lerp(currentDistance, maxDist, Time.deltaTime * smoothSpeed);
            Vector3 finalPos = pivot - rotation * Vector3.forward * currentDistance;

            transform.position = finalPos;
            transform.LookAt(pivot);
        }
    }
}
