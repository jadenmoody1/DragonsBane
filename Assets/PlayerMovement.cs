using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(RPGStats))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Input Keys")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Animation")]
    public GameObject idleModel;
    public GameObject walk1Model;
    public GameObject walk2Model;
    public GameObject airborneModel;
    public float baseWalkAnimationSpeed = 2f;
    public float speedAnimationMultiplier = 0.5f;

    [HideInInspector] public int walkState = 0;

    private CharacterController controller;
    private RPGStats stats;
    private Vector3 velocity;
    private Vector3 lastForward = Vector3.forward;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<RPGStats>();
    }

    void Update()
    {
        if (cameraTransform == null || stats == null) return;

        bool isGrounded = controller.isGrounded;

        // --- Input ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = Vector3.zero;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        moveDir = (forward * v + right * h).normalized;

        if (moveDir.magnitude > 0.01f)
        {
            lastForward = forward;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lastForward), 10f * Time.deltaTime);
        }

        // --- Movement & Speed ---
        bool isRunning = Input.GetKey(sprintKey) && moveDir.magnitude > 0.01f;
        float speed = stats.GetCurrentMoveSpeed(isRunning);
        controller.Move(moveDir * speed * Time.deltaTime);

        // --- Jump ---
        if (Input.GetKeyDown(jumpKey) && isGrounded && stats.currentStamina >= stats.jumpStaminaCost)
        {
            velocity.y = Mathf.Sqrt(stats.jumpHeight * -2f * Physics.gravity.y);
            stats.UseStamina(stats.jumpStaminaCost);
            stats.GainSkillXP("Acrobatics", 1f);
        }

        // --- Gravity ---
        velocity.y += Physics.gravity.y * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- Animation ---
        UpdateAnimation(moveDir, isGrounded);

        // --- Handle Running / Stamina ---
        stats.HandleRunning(moveDir.magnitude > 0.01f && isRunning, Time.deltaTime);
    }

    private void UpdateAnimation(Vector3 moveDir, bool isGrounded)
    {
        if (!isGrounded)
        {
            walkState = 0;
            SetModelState(false, false, false, true);
        }
        else if (moveDir.magnitude > 0.01f)
        {
            float animSpeed = baseWalkAnimationSpeed + speedAnimationMultiplier * moveDir.magnitude;
            float t = Time.time * animSpeed;
            walkState = (Mathf.FloorToInt(t) % 2) + 1;
            SetModelState(false, walkState == 1, walkState == 2, false);
        }
        else
        {
            walkState = 0;
            SetModelState(true, false, false, false);
        }
    }

    private void SetModelState(bool idle, bool walk1, bool walk2, bool air)
    {
        if (idleModel != null) idleModel.SetActive(idle);
        if (walk1Model != null) walk1Model.SetActive(walk1);
        if (walk2Model != null) walk2Model.SetActive(walk2);
        if (airborneModel != null) airborneModel.SetActive(air);
    }
}
