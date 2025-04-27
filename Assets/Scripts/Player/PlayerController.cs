using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Walk")]
    public float MaxWalkSpeed = 12.5f;
    public float GroundAcceleration = 5f;
    public float GroundDeceleration = 20f;
    public float AirAcceleration = 5f;
    public float AirDeceleration = 20f;

    [Header("Jumping")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float JumpForce = 10f;
    public float JumpCooldown = 0.2f;
    public float GravityScale = 2f;
    [SerializeField] private bool _showDebugGroundedBox = false;

    [Header("Climbing")]
    public float ClimbSpeed = 3f;
    public float ClimbShuffleSpeed = 0.2f; // speed when moving sideways while climbing

    [Header("Time Shift")]
    public float TimeShiftCooldown = 0.2f;

    [Header("Animation")]
    public float VelocityAnimationThreshold = 0.2f; // threshold for animation to play

    [Header("References")]
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private PlayerClimbArea _climbArea;

    private const string IDLE_ANIMATION = "Idle";
    private const string RUNNING_ANIMATION = "Running";
    private const string JUMP_ANIMATION = "Jump";
    private const string CLIMB_LADDER_ANIMATION = "ClimbLadder";
    private const string CLIMB_LADDER_IDLE_ANIMATION = "ClimbLadderIdle";
    private const string CLIMB_ROPE_ANIMATION = "ClimbRope";
    private const string CLIMB_ROPE_IDLE_ANIMATION = "ClimbRopeIdle";

    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    private RaycastHit2D _groundHit;
    private bool _isGrounded;

    //private bool _isJumping = false;
    private float _jumpCooldownTimer = 0f;
    private float _timeShiftCooldownTimer = 0f;

    private bool _isClimbing = false;

    private PlayerInputManager _playerInputManager;
    private Rigidbody2D _rb;
    private Animator _animator;

    private void Awake()
    {
        _isFacingRight = true;

        _playerInputManager = GetComponent<PlayerInputManager>();
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = GravityScale;
    }

    private void Start()
    {
        _playerInputManager.TimeShiftEvent.AddListener(TimeShift);
        _animator.Play(IDLE_ANIMATION);
    }

    private void Update()
    {
        //Debug.Log($"{_playerInputManager.JumpPressed} {_isGrounded} {_jumpCooldownTimer}");

        if (_playerInputManager.JumpPressed && (_isGrounded || _isClimbing)&& _jumpCooldownTimer <= 0)
        {
            //AudioManager.Instance.PlaySFX(2); // play bounce sound
            Jump(1);
            StopClimb(); //can't jump and climb at same time
        }

        if (_climbArea.CanClimb && !_isClimbing && !Mathf.Approximately(_playerInputManager.MovementDirection.y, 0f))
        {
            StartClimb();
        }

        if (_isClimbing)
        {
            ClimbCheck();
        }

        _jumpCooldownTimer -= Time.deltaTime;
        _timeShiftCooldownTimer -= Time.deltaTime;

        //Vector2 velocityAnim = new(
        //    Mathf.Abs(_rb.linearVelocityX) + VelocityAnimationThreshold, 
        //    Mathf.Abs(_rb.linearVelocityY) + VelocityAnimationThreshold);

        if (_isClimbing)
        {
            if (
                Mathf.Abs(_rb.linearVelocityX) > VelocityAnimationThreshold
                || Mathf.Abs(_rb.linearVelocityY) > VelocityAnimationThreshold)
            {
                if (_climbArea.LadderType == "Rope" && GetCurrentAnimationName() != CLIMB_ROPE_ANIMATION)
                {
                    _animator.Play(CLIMB_ROPE_ANIMATION);
                }
                else if (_climbArea.LadderType == "Ladder" && GetCurrentAnimationName() != CLIMB_LADDER_ANIMATION)
                {
                    _animator.Play(CLIMB_LADDER_ANIMATION);
                }
            }
            else
            {
                if (_climbArea.LadderType == "Rope" && GetCurrentAnimationName() != CLIMB_ROPE_IDLE_ANIMATION)
                {
                    _animator.Play(CLIMB_ROPE_IDLE_ANIMATION);
                }
                else if (_climbArea.LadderType == "Ladder" && GetCurrentAnimationName() != CLIMB_LADDER_IDLE_ANIMATION)
                {
                    _animator.Play(CLIMB_LADDER_IDLE_ANIMATION);
                }
            }
        }
        else if (!_isGrounded && GetCurrentAnimationName() != JUMP_ANIMATION)
        {
            _animator.Play(JUMP_ANIMATION);
            //_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name = JUMP_ANIMATION;
        }
        else if ((Mathf.Abs(_rb.linearVelocityX) > VelocityAnimationThreshold) && GetCurrentAnimationName() != RUNNING_ANIMATION)
        {
            _animator.Play(RUNNING_ANIMATION);
        }
        else if ((Mathf.Abs(_rb.linearVelocityX) <= VelocityAnimationThreshold) && _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name != IDLE_ANIMATION)
        {
            _animator.Play(IDLE_ANIMATION);
        }
    }

    private void FixedUpdate()
    {
        CollisionChecks();

        if (!_isClimbing)
        {
            if (_isGrounded)
            {
                Move(GroundAcceleration, GroundDeceleration, _playerInputManager.MovementDirection);
            }
            else
            {
                Move(AirAcceleration, AirDeceleration, _playerInputManager.MovementDirection);
            }
        }
        else
        {
            Climb();
        }
    }

    #region Movement
    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        FlipCheck(moveInput);

        if (!Mathf.Approximately(moveInput.x, 0f))
        {
            Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * MaxWalkSpeed;

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            //_rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
            _rb.linearVelocityX = _moveVelocity.x;
        }

        else if (Mathf.Approximately(moveInput.x, 0f))
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            //_rb.linearVelocity = new Vector2(_moveVelocity.x, _rb.linearVelocity.y);
            _rb.linearVelocityX = _moveVelocity.x;
        }
    }

    private void Climb()
    {
        _rb.linearVelocityY = _playerInputManager.MovementDirection.y * ClimbSpeed;
        _rb.linearVelocityX = _playerInputManager.MovementDirection.x * ClimbShuffleSpeed;
    }

    private void ClimbCheck()
    {
        // just can't climb
        if (!_climbArea.CanClimb)
        {
            StopClimb();
            Debug.Log("can't climb");
            // little hop if climbing upward and exceed ladder
            if (_playerInputManager.MovementDirection.y > 0f)
                Jump(0.5f);
            return;
        }

        // Climb towards ground, stop climbing
        if (_isGrounded && _playerInputManager.MovementDirection.y < 0f)
        {
            StopClimb();
            Debug.Log("going toward the ground");
            return;
        }

        // Cancel if we decide to move sideways
        //if (!Mathf.Approximately(_playerInputManager.MovementDirection.x, 0f))
        //{
        //    StopClimb();
        //    Debug.Log("movin sideways");
        //    return;
        //}
    }

    private void StartClimb()
    {
        _isClimbing = true;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = Vector2.zero; // reset all movement
        _feetCollider.enabled = false; 
        _bodyCollider.enabled = false; // disable colliders so we can phase thru platforms from below
    }

    private void StopClimb()
    {
        _isClimbing = false;
        _rb.gravityScale = GravityScale;
        _feetCollider.enabled = true;
        _bodyCollider.enabled = true;
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

    public void Jump(float multiplier)
    {
        _jumpCooldownTimer = JumpCooldown;

        _rb.AddForceY(JumpForce * multiplier, ForceMode2D.Impulse);
    }

    #endregion

    public void TimeShift()
    {
        if (_timeShiftCooldownTimer > TimeShiftCooldown)
            return;

        AudioManager.Instance.PlaySFX(0); // play time shift sound
        _timeShiftCooldownTimer = TimeShiftCooldown;
        LevelBase.Instance.OnTimeShift();
    }

    #region Collision Checks

    private void CollisionChecks()
    {
        IsGrounded();
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

    #endregion

    private string GetCurrentAnimationName()
    {
        return _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
    }

}
