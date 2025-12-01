using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource breathingSource;

    [Header("Clips")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip slideClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip heavyBreathingClip;

    private PlayerStats playerStats;
    private AudioClip currentMoveClip;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();

        // Setup breathing source
        if (breathingSource != null)
        {
            breathingSource.clip = heavyBreathingClip;
            breathingSource.loop = true;
            breathingSource.volume = 0;
            if (!breathingSource.isPlaying) breathingSource.Play();
        }
    }

    private void Update()
    {
        HandleBreathingAudio();
    }
    public void UpdateMovementAudio(Vector2 input, bool isSprinting, bool isGrounded, bool isSliding, bool isCrouching)
    {
        if (footstepSource == null) return;

        AudioClip targetClip = null;
        float targetVolume = 1.0f;

        if (isGrounded)
        {
            if (isSliding)
            {
                targetClip = slideClip;
            }
            else if (input.magnitude > 0.1f)
            {
                if (isSprinting)
                {
                    targetClip = runClip;
                }
                else
                {
                    targetClip = walkClip;
                    if (isCrouching) targetVolume = 0.5f;
                }
            }
        }

        footstepSource.volume = Mathf.Lerp(footstepSource.volume, targetVolume, Time.deltaTime * 10f);

        if (targetClip == null)
        {
            if (footstepSource.isPlaying) footstepSource.Stop();
            currentMoveClip = null;
        }
        else if (currentMoveClip != targetClip || !footstepSource.isPlaying)
        {
            footstepSource.clip = targetClip;
            footstepSource.loop = true;
            footstepSource.Play();
            currentMoveClip = targetClip;
        }
    }
    public void PlayJump()
    {
        PlayOneShot(jumpClip);
    }

    public void PlayOneShot(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    private void HandleBreathingAudio()
    {
        if (breathingSource == null || playerStats == null) return;

        float targetVolume = (playerStats.StaminaPercentage < 0.5f) ? 1.0f : 0.0f;

        breathingSource.volume = Mathf.Lerp(breathingSource.volume, targetVolume, Time.deltaTime * 2f);
    }
}