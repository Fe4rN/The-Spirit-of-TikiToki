using UnityEngine;

public class Tree : MonoBehaviour
{
    [Header("Ajustes de Tala")]
    public int tapsToCut = 5;
    private int _currentTaps = 0;

    [Header("Referencias")]
    public TreeHealthBarTree healthBar; // Arrastra aquí el script de la barra
    public GameObject woodPrefab;
    public int woodAmount = 3;

    private Vector3 _originalScale;

    void Start()
    {
        _originalScale = transform.localScale;
        // Inicializamos la barra pero estará oculta por su propio Start
    }

    public void TakeHit()
    {
        _currentTaps++;

        // Actualizamos la barra (Vida restante = Total - Toques)
        if (healthBar != null)
        {
            healthBar.SetHealth(tapsToCut - _currentTaps, tapsToCut);
        }

        StopAllCoroutines();
        StartCoroutine(HitEffect());

        if (_currentTaps >= tapsToCut)
        {
            Die();
        }
    }

    System.Collections.IEnumerator HitEffect()
    {
        transform.localScale = _originalScale * 0.9f;
        yield return new WaitForSeconds(0.05f);
        transform.localScale = _originalScale;
    }

    void Die()
    {
        for (int i = 0; i < woodAmount; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-0.6f, 0.6f), 0.2f, Random.Range(-0.6f, 0.6f));
            Instantiate(woodPrefab, transform.position + randomOffset, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}