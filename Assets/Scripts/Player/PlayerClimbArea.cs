using UnityEngine;

public class PlayerClimbArea : MonoBehaviour
{
    public bool CanClimb {  get; private set; } = false;
    public string LadderType { get; private set; } = string.Empty;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CanClimb = true;
        LadderType = collision.gameObject.tag;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        CanClimb = false;
        LadderType = string.Empty;
    }
}
