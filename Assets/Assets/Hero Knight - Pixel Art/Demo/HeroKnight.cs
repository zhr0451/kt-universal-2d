using UnityEngine;
using UnityEngine.InputSystem;

public class HeroKnight : MonoBehaviour
{
    [SerializeField]
    float m_speed = 4.0f;

    [SerializeField]
    float m_jumpForce = 7.5f;

    [SerializeField]
    float m_rollForce = 6.0f;

    [SerializeField]
    bool m_noBlood = false;

    [SerializeField]
    GameObject m_slideDust;

    Animator m_animator;
    Rigidbody2D m_body2d;
    Sensor_HeroKnight m_groundSensor;
    Sensor_HeroKnight m_wallSensorR1;
    Sensor_HeroKnight m_wallSensorR2;
    Sensor_HeroKnight m_wallSensorL1;
    Sensor_HeroKnight m_wallSensorL2;
    bool m_isWallSliding = false;
    bool m_grounded = false;
    bool m_rolling = false;
    int m_facingDirection = 1;
    int m_currentAttack = 0;
    float m_timeSinceAttack = 0.0f;
    float m_delayToIdle = 0.0f;
    float m_rollDuration = 8.0f / 14.0f;
    float m_rollCurrentTime;

    InputAction m_moveAction;
    InputAction m_attackAction;
    InputAction m_blockAction;
    InputAction m_jumpAction;
    InputAction m_rollAction;
    InputAction m_deathAction;
    InputAction m_hurtAction;

    void Awake()
    {
        m_moveAction = new InputAction("Move", InputActionType.Value);
        m_moveAction
            .AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/s")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/a")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/d")
            .With("Right", "<Keyboard>/rightArrow");
        m_moveAction.AddBinding("<Gamepad>/leftStick");

        m_attackAction = new InputAction("Attack", InputActionType.Button);
        m_attackAction.AddBinding("<Mouse>/leftButton");
        m_attackAction.AddBinding("<Gamepad>/buttonWest");

        m_blockAction = new InputAction("Block", InputActionType.Button);
        m_blockAction.AddBinding("<Mouse>/rightButton");
        m_blockAction.AddBinding("<Gamepad>/rightShoulder");

        m_jumpAction = new InputAction("Jump", InputActionType.Button);
        m_jumpAction.AddBinding("<Keyboard>/space");
        m_jumpAction.AddBinding("<Gamepad>/buttonSouth");

        m_rollAction = new InputAction("Roll", InputActionType.Button);
        m_rollAction.AddBinding("<Keyboard>/leftShift");
        m_rollAction.AddBinding("<Gamepad>/leftStickPress");

        m_deathAction = new InputAction("Death", InputActionType.Button);
        m_deathAction.AddBinding("<Keyboard>/e");

        m_hurtAction = new InputAction("Hurt", InputActionType.Button);
        m_hurtAction.AddBinding("<Keyboard>/q");
    }

    void OnEnable()
    {
        m_moveAction.Enable();
        m_attackAction.Enable();
        m_blockAction.Enable();
        m_jumpAction.Enable();
        m_rollAction.Enable();
        m_deathAction.Enable();
        m_hurtAction.Enable();
    }

    void OnDisable()
    {
        m_moveAction.Disable();
        m_attackAction.Disable();
        m_blockAction.Disable();
        m_jumpAction.Disable();
        m_rollAction.Disable();
        m_deathAction.Disable();
        m_hurtAction.Disable();
    }

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
    }

    // Update is called once per frame
    void Update()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if (m_rolling)
        {
            m_rollCurrentTime += Time.deltaTime;
        }

        // Disable rolling if timer extends duration
        if (m_rollCurrentTime > m_rollDuration)
        {
            m_rolling = false;
        }

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = m_moveAction.ReadValue<Vector2>().x;

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
        }
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling)
        {
            m_body2d.linearVelocity = new Vector2(
                inputX * m_speed,
                m_body2d.linearVelocity.y
            );
        }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.linearVelocity.y);

        // -- Handle Animations --
        //Wall Slide
        m_isWallSliding =
            (m_wallSensorR1.State() && m_wallSensorR2.State())
            || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        //Death
        if (m_deathAction.WasPressedThisFrame() && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }

        //Hurt
        else if (m_hurtAction.WasPressedThisFrame() && !m_rolling)
        {
            m_animator.SetTrigger("Hurt");
        }

        //Attack
        else if (m_attackAction.WasPressedThisFrame() && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
            {
                m_currentAttack = 1;
            }

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
            {
                m_currentAttack = 1;
            }

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if (m_blockAction.WasPressedThisFrame() && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }
        else if (m_blockAction.WasReleasedThisFrame())
        {
            m_animator.SetBool("IdleBlock", false);
        }

        // Roll
        else if (m_rollAction.WasPressedThisFrame() && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.linearVelocity = new Vector2(
                m_facingDirection * m_rollForce,
                m_body2d.linearVelocity.y
            );
        }

        //Jump
        else if (m_jumpAction.WasPressedThisFrame() && m_grounded && !m_rolling)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.linearVelocity = new Vector2(m_body2d.linearVelocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
            {
                m_animator.SetInteger("AnimState", 0);
            }
        }
    }

    // Animation Events
    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
        {
            spawnPosition = m_wallSensorR2.transform.position;
        }
        else
        {
            spawnPosition = m_wallSensorL2.transform.position;
        }

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(
                m_slideDust,
                spawnPosition,
                gameObject.transform.localRotation
            ) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
}
