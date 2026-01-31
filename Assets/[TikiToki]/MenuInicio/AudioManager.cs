using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Configuración")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    private AudioSource _musicSource;

    void Awake()
    {
        // Lógica de Singleton: Si ya existe uno, se destruye. Si no, persiste.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ConfigurarFuentesIniciales();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ConfigurarFuentesIniciales()
    {
        // Creamos un AudioSource dedicado solo para la música
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.outputAudioMixerGroup = musicGroup;
        _musicSource.playOnAwake = false;
    }

    // --- MÉTODOS PÚBLICOS PARA LLAMAR DESDE CUALQUIER LUGAR ---

    // Reproducir música (cambia la canción actual)
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (_musicSource.clip == clip) return; // Ya está sonando

        _musicSource.clip = clip;
        _musicSource.volume = volume;
        _musicSource.Play();
    }

    // Reproducir un efecto de sonido 2D (interfaz, sonidos globales)
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        // Creamos un AudioSource temporal para efectos 2D o usamos PlayOneShot
        _musicSource.PlayOneShot(clip, volume);
    }

    // Reproducir un sonido 3D en una posición específica
    public void Play3DSound(AudioClip clip, Vector3 position, float spatialBlend = 1.0f)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = sfxGroup;
        source.spatialBlend = spatialBlend; // 1.0 es 3D total
        source.Play();

        Destroy(tempGO, clip.length);
    }
}