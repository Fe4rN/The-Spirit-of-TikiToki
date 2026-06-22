using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TikiToki.Gameplay.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("MaxHits")]
        [SerializeField] private int maxHits = 5;
        public int remainingHits;
        private bool _isImmune = false;
        [UnityEngine.Serialization.FormerlySerializedAs("Character")]
        [SerializeField] private GameObject characterModel;
        [SerializeField] private HealthBarUI healthBar;

        private void Start()
        {
            remainingHits = maxHits;
        }

        public void TakeDamage()
        {
            if (_isImmune || remainingHits <= 0) return;

            remainingHits--;
            Debug.Log("Player hit. Remaining hits: " + remainingHits);

            if (healthBar != null)
            {
                healthBar.updateSlider(remainingHits);
            }

            if (remainingHits <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(FlashCharacter());
                StartCoroutine(ActivateImmunity());
            }
        }

        private void Die()
        {
            StopAllCoroutines();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.EndGame(false, "Lives exhausted.");
            }
        }

        private IEnumerator FlashCharacter()
        {
            if (characterModel != null)
            {
                characterModel.SetActive(false);
                yield return new WaitForSeconds(.1f);
                characterModel.SetActive(true);
                yield return new WaitForSeconds(.1f);
                characterModel.SetActive(false);
                yield return new WaitForSeconds(.1f);
                characterModel.SetActive(true);
            }
        }

        private IEnumerator ActivateImmunity()
        {
            _isImmune = true;
            yield return new WaitForSeconds(1.5f);
            _isImmune = false;
        }
    }
}
