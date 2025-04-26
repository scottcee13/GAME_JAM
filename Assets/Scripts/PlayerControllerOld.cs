using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerOld : MonoBehaviour
{
    [Header("Walk")]
    public float MaxWalkSpeed = 12.5f;
    public float GroundAcceleration = 5f;
    public float GroundDeceleration = 20f;
    public float AirAcceleration = 5f;
    public float AirDeceleration = 20f;

    [Header("Grounded/Collision Checks")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.02f;
    public float HeadWidth = 0.75f;
    [SerializeField] private bool _showDebugGroundedBox = false;
    [SerializeField] private bool _showDebugHeadBumpBox = false;

    [Header("Jump")]
    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.054f;
    public float TimeToJumpApex = 0.35f;
    public float GravityOnReleaseMultiplier = 2f; // may be optional? but may feel better?
    public float MaxFallSpeed = 26f;
    public int NumberOfJumpsAllowed = 1;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f; // how long is he staying up there

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

    [Header("Jump Visualization")]
    public bool ShowJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    public bool DrawLeft = true;
    [Range(5, 100)] public int ArcResolution = 20;
    [Range(0, 500)] public int VisualizationSteps = 90;

    [Header("References")]
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;

    //values calculated based on stats
    private float _gravity;
    private float _initialJumpVelocity;
    private float _adjustedJumpHeight;

    //movement vars
    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    //collistion check vars
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    //jump vars
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    //apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;

    //jump buffer vars
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;

    //coyote time vars
    private float _coyoteTimer;

    private PlayerInputManagerOld _playerInputManager;
    private Rigidbody2D _rb;

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void Awake()
    {
        _isFacingRight = true;

        _playerInputManager = GetComponent<PlayerInputManagerOld>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CountTimers();
        JumpChecks();
    }

    private void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if (_isGrounded)
        {
            Move(GroundAcceleration, GroundDeceleration, _playerInputManager.MovementDirection);
        }
        else
        {
            Move(AirAcceleration, AirDeceleration, _playerInputManager.MovementDirection);
        }
    }

    private void CalculateValues()
    {
        _adjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        _gravity = -(2f * _adjustedJumpHeight) / Mathf.Pow(TimeToJumpApex, 2f);
        _initialJumpVelocity = Mathf.Abs(_gravity) * TimeToJumpApex;
    }

    #region Movement
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        FlipCheck(moveInput);

        if (moveInput != Vector2.zero)
        {
            Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * MaxWalkSpeed;

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }

        else if (moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
        }
    }

    private void FlipCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            SetFacingRight(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            SetFacingRight(true);
        }

    }

    private void SetFacingRight(bool faceRight)
    {
        if (faceRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jump

    private void JumpChecks()
    {
        // WHEN WE PRESS THE JUMP BUTTON
        if (_playerInputManager.JumpWasPressed)
        {
            _playerInputManager.JumpWasPressed = false;
            _jumpBufferTimer = JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        // WHEN WE RELEASE THE JUMP BUTTON

        if (_playerInputManager.JumpWasReleased)
        {
            _playerInputManager.JumpWasReleased = false;

            if (_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = TimeForUpwardsCancel; // Time from direction change to fast fall
                    VerticalVelocity = 0f; // Reset or else too long to switch
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        // INITIATE JUMP WITH JUMP BUFFERING AND COYOTE TIME
        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if (_jumpReleasedDuringBuffer) // does a bunny hop, idk
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }

        // MULTI JUMP
        else if (_jumpBufferTimer > 0f && !_isJumping && _numberOfJumpsUsed < NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        // AIR JUMP AFTER COYOTE TIME LAPSED
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < NumberOfJumpsAllowed - 1)
        {
            InitiateJump(2); // make sure we don't get bonus jump
            _isFastFalling = false;
        }

        // LANDED
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    private void InitiateJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = _initialJumpVelocity;
    }

    private void Jump()
    {
        // APPLY GRAVITY WHILE JUMPING
        if (_isJumping)
        {
            // CHECK FOR HEAD BUMP
            if (_bumpedHead)
            {
                _isFastFalling = true;
            }

            // GRAVITY ON ASCENDING
            if (VerticalVelocity >= 0f)
            {
                // APEX CONTROLS
                _apexPoint = Mathf.InverseLerp(_initialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                        _timePastApexThreshold = 0f;
                    }

                    if (_isPastApexThreshold)
                    {
                        _timePastApexThreshold += Time.fixedDeltaTime;
                        if (_timePastApexThreshold < ApexHangTime)
                        {
                            VerticalVelocity = 0f;
                        }
                        else
                        {
                            VerticalVelocity = -0.01f;
                        }
                    }
                }

                // GRAVITY ON ASCENING BUT NOT PAST APEX THRESHOLD
                else
                {
                    VerticalVelocity += _gravity * Time.fixedDeltaTime;
                    if (_isPastApexThreshold)
                    {
                        _isPastApexThreshold = false;
                    }
                }
            }

            // GRAVITY ON DESCENDING
            else if (!_isFastFalling)
            {
                VerticalVelocity += _gravity * GravityOnReleaseMultiplier * Time.fixedDeltaTime;
                if (VerticalVelocity < MaxFallSpeed)
                {
                    VerticalVelocity = MaxFallSpeed;
                }
            }

            else if (VerticalVelocity < 0f) //just ensure falling flag if we're going down
            {
                if (!_isFalling)
                    _isFalling = true;
            }
        }

        // JUMP CUT
        if (_isFastFalling)
        {
            if (_fastFallTime >= TimeForUpwardsCancel)
            {
                VerticalVelocity += _gravity * GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < TimeForUpwardsCancel)
            {
                VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, _fastFallTime / TimeForUpwardsCancel);
            }

            _fastFallTime += Time.fixedDeltaTime;
        }

        // NORMAL GRAVITY WHILE FALLING
        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }

            VerticalVelocity += _gravity * Time.fixedDeltaTime;
        }

        // CLAMP FALL SPEED
        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MaxFallSpeed, 50f); // 50 is arbitrary, but should be high enough to not clamp the jump velocity

        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
    }

    #endregion

    #region Collision Checks

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    /// <summary>
    /// Box cast based on player's boxy feet
    /// </summary>
    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
        Vector2 boxCastSize = new(_feetCollider.bounds.size.x, GroundDetectionRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, GroundDetectionRayLength, GroundLayer);

        _isGrounded = (_groundHit.collider != null); //ground if we hit something

        #region Debug Visualization
        if (_showDebugGroundedBox)
        {
            Color rayColor;
            if (_isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion


    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new(_feetCollider.bounds.center.x, _bodyCollider.bounds.min.y);
        Vector2 boxCastSize = new(_feetCollider.bounds.size.x * HeadWidth, HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, HeadDetectionRayLength, GroundLayer);

        _bumpedHead = (_headHit.collider != null); //ground if we hit something

        #region Debug Visualization
        if (_showDebugHeadBumpBox)
        {
            Color rayColor;
            if (_bumpedHead)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * HeadWidth, boxCastOrigin.y), Vector2.down * GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2 * HeadWidth, boxCastOrigin.y), Vector2.down * GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * HeadWidth, boxCastOrigin.y + HeadDetectionRayLength), Vector2.right * boxCastSize.x * HeadWidth, rayColor);
        }
        #endregion


    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        _jumpBufferTimer -= Time.deltaTime;

        // Allow for jumping briefly after losing contact with ground (coyote time)
        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }
        else
        {
            _coyoteTimer = JumpCoyoteTime;
        }
    }

    #endregion
}
