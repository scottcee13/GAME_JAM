using UnityEngine;

/// <summary>
/// To reference the player controller from other player body parts
/// </summary>
public class PlayerPart : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    public PlayerController PlayerController => _playerController;
}
