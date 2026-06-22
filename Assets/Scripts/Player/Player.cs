using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private MoveAbility moveAbility;
    private JumpAbility jumpAbility;
    private TimeSlowAbility timeSlowAbility;
    private DashAbility dashAbility;
    private ActiveWeapon activeWeapon;
    private AttackAbility attackAbility;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleTimer circleTimer;
    [SerializeField] private GameObject look;
    [SerializeField] private float timeOfAiming = 2.0f;

    [SerializeField] private PlayerInput playerInput;
    private float moveDirection = 0;
    private Vector2 lookDirection = Vector2.zero;
    private bool isAttacking = false;
    private bool isDashing = false;
    private float time = 0.0f;
    private bool hasSlowing = false;

    private void Awake()
    {
        enabled = false;
    }
    public void Init()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Dash"].started += OnDashStarted;
        playerInput.actions["Dash"].canceled += OnDashCanceled;
        playerInput.actions["Attack"].canceled += OnAttackCanceled;
        playerInput.actions["Attack"].started += OnAttackStarted;

        moveAbility = GetComponent<MoveAbility>();
        jumpAbility = GetComponent<JumpAbility>();
        timeSlowAbility = GetComponent<TimeSlowAbility>();
        dashAbility = GetComponent<DashAbility>();
        activeWeapon = GetComponent<ActiveWeapon>();
        attackAbility = GetComponent<AttackAbility>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        //
        look.SetActive(false);

        circleTimer.timerEnded += TimerEnd;

        enabled = true;
    }

    private void Update()
    {
        if (isDashing)
        {
            SetLookDirection();
            if (!hasSlowing)
            {
                SlowTimeStart();
            }
        }
        else if (isAttacking)
        {
            time += Time.deltaTime;
            if (time > 0.1f)
            {
                SetLookDirection();
                if (!hasSlowing)
                {
                    SlowTimeStart();
                }
            }
            else
            {
                lookDirection = new Vector2(lookDirection.x, 0).normalized;
            }
        }
        else
        {
            SetLookDirection();
        }

        if (!activeWeapon.Weapon.IsAttacking)
        {
            spriteRenderer.flipX = lookDirection.x < 0 ? true : false;
        }

        if (activeWeapon.Weapon.IsAttacking)
        {
            moveAbility.Direction = 0;
            animator.SetBool("Attacking", true);
            animator.SetBool("Running", false);
            animator.SetBool("Fall", false);
            animator.SetBool("Jump", false);
        }
        else if (Mathf.Abs(rb.linearVelocityY) > 0.1f)
        {
            moveAbility.Direction = moveDirection;
            animator.SetBool("Attacking", false);
            animator.SetBool("Running", false);
            if (rb.linearVelocityY > 0)
            {
                animator.SetBool("Fall", false);
                animator.SetBool("Jump", true);
            }
            else
            {
                animator.SetBool("Jump", false);
                animator.SetBool("Fall", true);
            }
        }
        else
        {
            moveAbility.Direction = moveDirection;
            animator.SetBool("Fall", false);
            animator.SetBool("Jump", false);
            animator.SetBool("Attacking", false);

            if (moveAbility.Direction != 0)
            {
                animator.SetBool("Running", true);
            }
            else
            {
                animator.SetBool("Running", false);
            }
        }    
    }

    private void SetLookDirection()
    {
        Vector2 deltaMouse = Mouse.current.delta.ReadValue();
        if (!(deltaMouse == Vector2.zero || deltaMouse.magnitude <= 5.0f))
        {
            lookDirection = deltaMouse.normalized;
            float newZ = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90.0f;
            look.transform.eulerAngles = new Vector3(0, 0, newZ);
        }
    }

    private void OnMove(InputValue value)
    {
        moveDirection = value.Get<float>();
    }

    private void OnJump()
    {
        jumpAbility.Jump();
    }

    private void OnDashStarted(InputAction.CallbackContext context)
    {
        if (isAttacking) return;

        isDashing = true;
    }

    private void OnDashCanceled(InputAction.CallbackContext context)
    {
        if (!isDashing) return;

        dashAbility.Dash(lookDirection);
        circleTimer.StopTimer();
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        if (isDashing) return;

        lookDirection = new Vector2(lookDirection.x, 0).normalized;
        isAttacking = true;    
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        if (!isAttacking) return;

        moveAbility.Direction = 0;
        attackAbility.Attack(activeWeapon.Weapon, lookDirection);
        circleTimer.StopTimer();
    }

    private void SlowTimeStart()
    {
        timeSlowAbility.IsActive = true;
        hasSlowing = true;

        look.SetActive(true);
        circleTimer.StartTimer(timeOfAiming);
    }

    private void TimerEnd(object o, EventArgs e)
    {
        look.SetActive(false);

        timeSlowAbility.IsActive = false;
        hasSlowing = false;
        time = 0.0f;

        if (isAttacking) isAttacking = false;
        else if (isDashing) isDashing = false;
    }

}
