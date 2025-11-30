using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource breathingSource; // Assign a separate source for breathing

    [Header("Clips")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip slideClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip heavyBreathingClip;

    private PlayerController playerController;
    private PlayerStats playerStats;
    private AudioClip currentMoveClip;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();

        // Setup breathing source
        if (breathingSource != null)
        {
            breathingSource.clip = heavyBreathingClip;
            breathingSource.loop = true;
            breathingSource.volume = 0; // Start silent
            if (!breathingSource.isPlaying) breathingSource.Play();
        }
    }

    private void Update()
    {
        HandleMovementAudio();
        HandleBreathingAudio();
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

    private void HandleMovementAudio()
    {
        if (footstepSource == null) return;

        AudioClip targetClip = null;

        if (playerController.IsGrounded)
        {
            if (playerController.IsSliding) targetClip = slideClip;
            else if (playerController.IsMoving) targetClip = playerController.IsSprinting ? runClip : walkClip;
        }

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

    private void HandleBreathingAudio()
    {
        if (breathingSource == null || playerStats == null) return;

        // Logic: If stamina < 50%, fade in sound. Otherwise, fade out.
        float targetVolume = (playerStats.StaminaPercentage < 0.5f) ? 1.0f : 0.0f;

        // Smoothly transition volume
        breathingSource.volume = Mathf.Lerp(breathingSource.volume, targetVolume, Time.deltaTime * 2f);
    }
}