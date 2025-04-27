using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTester : MonoBehaviour
{
    // Properly set up level
    void Start()
    {
        UIManager.Instance.Restart();
    }
}
