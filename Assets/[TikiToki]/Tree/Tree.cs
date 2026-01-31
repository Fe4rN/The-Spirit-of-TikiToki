using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour
{
    [Header("Ajustes de Tala")]
    public int tapsToCut = 5;
    private float _currentDamage = 0f; // Ahora es float para suavidad

    [Header("Ajustes de Regeneración")]
    public float timeToStartRegen = 3.0f;
    public float regenSpeed = 0.5f; // Cantidad de "toques" que recupera por segundo
    private float _lastHitTime;

    [Header("Referencias")]
    public TreeHealthBarTree healthBar;
    public GameObject woodPrefab;
    public int woodAmount = 3;

    private Vector3 _originalScale;

    public AudioClip sounTreeHit;
    public AudioClip soundTreeFall;

    void Start()
    {
        _originalScale = transform.localScale;
    }

    void Update()
    {
        // Si hay daño, intentamos regenerar
        if (_currentDamage > 0)
        {
            if (Time.time - _lastHitTime > timeToStartRegen)
            {
                // Curación gradual y constante
                _currentDamage -= Time.deltaTime * regenSpeed;

                // Evitamos que baje de 0
                if (_currentDamage <= 0)
                {
                    _currentDamage = 0;
                    if (healthBar != null) healthBar.gameObject.SetActive(false);
                }

                UpdateVisuals();
            }
        }
    }

    public void TakeHit()
    {
        // Sumamos 1 de daño completo por cada pulsación
        _currentDamage += 1f;
        _lastHitTime = Time.time;

        AudioSource source = GetComponent<AudioSource>();
        if (source != null && sounTreeHit != null)
        {
            source.PlayOneShot(sounTreeHit);
        }

        UpdateVisuals();

        StopAllCoroutines();
        StartCoroutine(HitEffect());

        // Comprobamos si el daño acumulado supera el límite
        if (_currentDamage >= tapsToCut)
        {
            Die();
        }
    }

    // Nueva función para centralizar la actualización de la barra
    void UpdateVisuals()
    {
        if (healthBar != null)
        {
            if (_currentDamage > 0)
            {
                // Si hay daño, nos aseguramos de que esté encendida y actualizamos
                if (!healthBar.gameObject.activeSelf) healthBar.gameObject.SetActive(true);
                healthBar.SetHealth(tapsToCut - _currentDamage, tapsToCut);
            }
            else
            {
                // Si el daño es 0, apagamos la barra
                if (healthBar.gameObject.activeSelf) healthBar.gameObject.SetActive(false);
            }
        }
    }

    IEnumerator HitEffect()
    {
        transform.localScale = _originalScale * 0.9f;
        yield return new WaitForSeconds(0.05f);
        transform.localScale = _originalScale;
    }

    void Die()
    {
        AudioSource source = GetComponent<AudioSource>();
        if (source != null && soundTreeFall != null)
        {
            // 1. Reproducimos el sonido de caída
            source.PlayOneShot(soundTreeFall);
        }

        for (int i = 0; i < woodAmount; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-0.6f, 0.6f), 0.2f, Random.Range(-0.6f, 0.6f));
            Instantiate(woodPrefab, transform.position + randomOffset, Quaternion.identity);
        }
        gameObject.SetActive(false);
    }

    public void ResetearArbol()
    {
        // 1. Reseteamos el daño a 0 (equivale a vida completa)
        _currentDamage = 0f;

        // 2. Apagamos la barra de vida para que no se vea al "nacer"
        if (healthBar != null)
        {
            // En tu script healthBar es el componente, así que usamos .gameObject
            healthBar.gameObject.SetActive(false);

            // Dejamos el valor de la barra preparado al máximo por si acaso
            healthBar.SetHealth(tapsToCut, tapsToCut);
        }

        // 3. Importante: si el árbol murió, su escala podría haber quedado rara.
        // Aunque el Spawner lo escala de 0 a 1, nos aseguramos de que el valor inicial sea 0.
        transform.localScale = Vector3.zero;
    }
}