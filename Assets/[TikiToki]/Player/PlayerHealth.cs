using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int MaxHits;
    public int remainingHits;
    private bool isImmune = false;
    [SerializeField] private GameObject Character;
    [SerializeField] private HealthBarUI healthBar;

    private void Start()
    {
        remainingHits = MaxHits;
    }

    public void TakeDamage()
    {
        // 1. Si es inmune, salimos inmediatamente
        if (isImmune || remainingHits <= 0) return;

        // 2. Restamos UNA sola vez
        remainingHits--;
        Debug.Log("Restando una hostia. Vidas restantes: " + remainingHits);

        // 3. Actualizamos UI con seguridad (null check)
        if (healthBar != null)
        {
            healthBar.updateSlider(remainingHits);
        }

        // 4. Comprobamos si ha muerto
        if (remainingHits <= 0)
        {
            Die();
        }
        else
        {
            // 5. Activamos efectos e inmunidad SOLO si sigue vivo
            StartCoroutine(FlashCharacter());
            StartCoroutine(ActivateImmunity());
        }
    }

    private void Die()
    {
        StopAllCoroutines();
        GameObject.FindFirstObjectByType<WinLose>().FinalizarPartida(false, "Vidas agotadas.");
    }

    private IEnumerator FlashCharacter()
    {
        Character.SetActive(false);
        yield return new WaitForSeconds(.1f);
        Character.SetActive(true);
        yield return new WaitForSeconds(.1f);
        Character.SetActive(false);
        yield return new WaitForSeconds(.1f);
        Character.SetActive(true);
    }

    private IEnumerator ActivateImmunity()
    {
        isImmune = true;
        yield return new WaitForSeconds(1.5f);
        isImmune = false;
    }
}
