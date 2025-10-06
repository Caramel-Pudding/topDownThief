
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource), typeof(DoorController))]
public class DoorAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip lockedClip;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;
    [SerializeField] private AudioMixerGroup outputGroup;
    [SerializeField] private bool twoDSound = true;

    private AudioSource audioSource;
    private DoorController doorController;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        doorController = GetComponent<DoorController>();
        ConfigureAudioSource();
    }

    private void OnEnable()
    {
        doorController.OnOpened.AddListener(PlayOpenSound);
        doorController.OnClosed.AddListener(PlayCloseSound);
    }

    private void OnDisable()
    {
        doorController.OnOpened.RemoveListener(PlayOpenSound);
        doorController.OnClosed.RemoveListener(PlayCloseSound);
    }

    private void ConfigureAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = twoDSound ? 0f : 1f;
        if (outputGroup) audioSource.outputAudioMixerGroup = outputGroup;
    }

    public void PlayOpenSound()
    {
        if (openClip) audioSource.PlayOneShot(openClip, sfxVolume);
    }

    public void PlayCloseSound()
    {
        if (closeClip) audioSource.PlayOneShot(closeClip, sfxVolume);
    }

    public void PlayLockedSound()
    {
        if (lockedClip) audioSource.PlayOneShot(lockedClip, sfxVolume);
    }
}
