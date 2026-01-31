using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int MaxHits;
    private int remainingHits;
    private bool isImmune = false;
    [SerializeField] private GameObject Character;

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
    }

    private IEnumerator FlashCharacter()
    {
        Character.SetActive(false);
        yield return new WaitForSeconds(.5f);
        Character.SetActive(true);
        yield return new WaitForSeconds(.5f);
        Character.SetActive(false);
        yield return new WaitForSeconds(.5f);
        Character.SetActive(false);
    }

    private IEnumerator ActivateImmunity()
    {
        isImmune = true;
        yield return new WaitForSeconds(1.5f);
        isImmune = false;
    }
}
