using UnityEngine;

public class PlayerClimbArea : MonoBehaviour
{
    public bool CanClimb {  get; private set; } = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CanClimb = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        CanClimb = false;
    }
}
