using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
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

    [Header("References")]
    [SerializeField] private Collider2D _feetCollider;
    [SerializeField] private Collider2D _bodyCollider;

    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    private PlayerInputManager _playerInputManager;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _isFacingRight = true;

        _playerInputManager = GetComponent<PlayerInputManager>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        CollisionChecks();

        if (_isGrounded)
        {
            Move(GroundAcceleration, GroundDeceleration, _playerInputManager.MovementDirection);
        }
        else
        {
            Move(AirAcceleration, AirDeceleration, _playerInputManager.MovementDirection);
        }
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
}
