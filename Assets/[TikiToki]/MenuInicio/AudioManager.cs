using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Configuración de Mezclador")]
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private AudioMixerGroup musicGroup;

    [Header("Canciones (Arrastra aquí tus clips)")]
    [SerializeField] private AudioClip musicaMenu;
    [SerializeField] private AudioClip musicaNivel1;

    private AudioSource _musicSource;

    void Awake()
    {
        // Singleton: Solo uno puede sobrevivir
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ConfigurarFuentesIniciales();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        // Suscribirse al evento de carga de escena
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    void OnDisable()
    {
        // Desuscribirse para evitar errores de memoria
        SceneManager.sceneLoaded -= AlCargarEscena;
    }

    private void ConfigurarFuentesIniciales()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.outputAudioMixerGroup = musicGroup;
        _musicSource.playOnAwake = false;
    }

    // Este método se ejecuta AUTOMÁTICAMENTE cada vez que cambias de escena
    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name == "MainMenu")
        {
            PlayMusic(musicaMenu, 1f);
        }
        else if (escena.name == "Level1")
        {
            PlayMusic(musicaNivel1, 0.7f);
        }
    }

    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (_musicSource.clip == clip) return; // Si ya está sonando, no la reinicies

        _musicSource.clip = clip;
        _musicSource.volume = volume;
        _musicSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _musicSource.PlayOneShot(clip, volume);
    }

    // Sonido 3D corregido para usar el Mixer de efectos
    public void Play3DSound(AudioClip clip, Vector3 position, float spatialBlend = 1.0f)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = sfxGroup; // Usamos el grupo de efectos
        source.spatialBlend = spatialBlend;
        source.Play();

        Destroy(tempGO, clip.length);
    }
}