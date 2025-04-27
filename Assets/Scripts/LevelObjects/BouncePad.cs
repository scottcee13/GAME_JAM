using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Tooltip("Multiplier to the normal jump force")]
    [SerializeField] private float _bounceStrength = 3f;

    private Animator _animator;

    private const string BOUNCE_ANIMATION = "BouncePadActivate";
    private PlayerController _caughtPlayer;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerPart player))
        {
            Debug.Log("Found player");
            //player.PlayerController.Jump(_bounceStrength);
            _caughtPlayer = player.PlayerController;
            _animator.Play(BOUNCE_ANIMATION);
        }
    }

    public void BouncePlayer()
    {
        if (_caughtPlayer != null)
        {
            _caughtPlayer.Jump(_bounceStrength);
            _caughtPlayer = null;
        }
    }
}
