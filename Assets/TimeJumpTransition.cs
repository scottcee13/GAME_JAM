using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeJumpTransition : MonoBehaviour
{
    [Header("Swap Settings")]
    public GameObject pastEnvironment;
    public GameObject presentEnvironment;
    private bool isInPast = false;

    [Header("Flash Settings")]
    public Image flashImage;
    public float flashDuration = 0.5f;

    [Header("Particle Effects")]
    public ParticleSystem swapParticles;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(SwapTime());
        }
    }
    private void Start()
    {
        pastEnvironment.SetActive(false);
    }
    IEnumerator SwapTime()
    {

        StartCoroutine(FlashEffect());

        if (swapParticles != null)
        {
            swapParticles.Play();
        }


        yield break;
    }


    IEnumerator FlashEffect()
    {
        float t = 0f;


        while (t < flashDuration / 2f)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / (flashDuration / 2f));
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        isInPast = !isInPast;
        pastEnvironment.SetActive(isInPast);
        presentEnvironment.SetActive(!isInPast);

        // (optional)
        if (swapParticles != null)
        {
            swapParticles.Play();
        }

        t = 0f;
        while (t < flashDuration / 2f)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / (flashDuration / 2f));
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    }

}
