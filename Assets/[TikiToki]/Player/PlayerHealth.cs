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
        if (isImmune) return;
        if (remainingHits-- > 0)
        {
            remainingHits--;
            Debug.Log("Restando una hostia");
            healthBar.updateSlider(remainingHits);
            StartCoroutine(FlashCharacter());
            StartCoroutine(ActivateImmunity());
        }
        else
        {
            Die();
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
