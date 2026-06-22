using System;
using UnityEngine;
using TikiToki.Gameplay;

namespace TikiToki.Gameplay.Environment
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "Hoguera")]
    public class Bonfire : MonoBehaviour, IHoldInteractable
    {
        [Header("State")]
        [UnityEngine.Serialization.FormerlySerializedAs("estaEncendida")]
        public bool isLit = false;
        [UnityEngine.Serialization.FormerlySerializedAs("tieneMadera")]
        public bool hasWood = false;
        [UnityEngine.Serialization.FormerlySerializedAs("tieneHojas")]
        public bool hasLeaves = false;

        [Header("Lighting Progress")]
        [UnityEngine.Serialization.FormerlySerializedAs("progresoActual")]
        public float currentProgress = 0f;
        [UnityEngine.Serialization.FormerlySerializedAs("tiempoNecesario")]
        public float requiredTime = 2.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("velocidadDescenso")]
        public float decreaseSpeed = 0.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("sistemaChispas")]
        public ParticleSystem sparkSystem;
        [UnityEngine.Serialization.FormerlySerializedAs("barraProgreso")]
        public TreeHealthBarTree progressBar;

        [Header("Unified Models")]
        [UnityEngine.Serialization.FormerlySerializedAs("modeloVacio")]
        public GameObject emptyModel;
        [UnityEngine.Serialization.FormerlySerializedAs("modeloConMadera")]
        public GameObject woodModel;
        [UnityEngine.Serialization.FormerlySerializedAs("modeloConHojas")]
        public GameObject leavesModel;
        [UnityEngine.Serialization.FormerlySerializedAs("modeloCompleto")]
        public GameObject completeModel;

        [Header("Effects")]
        [UnityEngine.Serialization.FormerlySerializedAs("efectosFuego")]
        public GameObject fireEffects;

        [Header("Visual Settings")]
        private MeshRenderer[] _renderers;
        [ColorUsage(true, true)] public Color highlightColor = new Color(0.5f, 0.5f, 0.5f);

        private bool _isBeingLitThisFrame = false;

        public static Action OnBonfireLit;

        // --- IInteractable & IHoldInteractable Implementation ---
        public string InteractionPrompt => "Bonfire";

        public bool CanInteract(TikiToki.Inventory.PlayerInventory inventory)
        {
            if (isLit) return false;

            // Can add materials
            if (!hasWood || !hasLeaves)
            {
                var activeSlot = inventory.slots[inventory.activeSlotIndex];
                if (activeSlot.item != null)
                {
                    string itemName = activeSlot.item.itemName.ToLower();
                    if (itemName == "woodpile" && !hasWood) return true;
                    if (itemName == "leavespile" && !hasLeaves) return true;
                }
            }

            // Can light
            if (hasWood && hasLeaves)
            {
                return true;
            }

            return false;
        }

        public void Interact(TikiToki.Inventory.PlayerInventory inventory)
        {
            if (isLit) return;

            var activeSlot = inventory.slots[inventory.activeSlotIndex];
            if (activeSlot.item != null)
            {
                string itemName = activeSlot.item.itemName.ToLower();
                if (itemName == "woodpile" && !hasWood)
                {
                    hasWood = true;
                    TikiToki.Inventory.PlayerInventory.OnBonfireMaterialAdded?.Invoke("woodPile");
                    inventory.ConsumeSlot(activeSlot, this);
                    return;
                }
                if (itemName == "leavespile" && !hasLeaves)
                {
                    hasLeaves = true;
                    TikiToki.Inventory.PlayerInventory.OnBonfireMaterialAdded?.Invoke("leavesPile");
                    inventory.ConsumeSlot(activeSlot, this);
                    return;
                }
            }
        }

        public void InteractHold(TikiToki.Inventory.PlayerInventory inventory, float deltaTime)
        {
            if (!isLit && hasWood && hasLeaves)
            {
                inventory.BonfireBeingLit = this;
                TryToLight(deltaTime);
            }
        }

        public void StopInteractHold(TikiToki.Inventory.PlayerInventory inventory)
        {
            StopLighting();
            if (inventory.BonfireBeingLit == this)
            {
                inventory.BonfireBeingLit = null;
            }
        }

        public void SetHighlight(bool active)
        {
            var activeRenderers = GetComponentsInChildren<MeshRenderer>();

            foreach (var ren in activeRenderers)
            {
                if (ren == null) continue;
                Material[] mats = ren.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (active)
                    {
                        mats[i].EnableKeyword("_EMISSION");
                        mats[i].SetColor("_EmissionColor", highlightColor);
                    }
                    else
                    {
                        mats[i].SetColor("_EmissionColor", Color.black);
                    }
                }
                ren.materials = mats;
            }
        }

        void Start()
        {
            Transform fireObj = transform.Find("FireMobile");
            if (fireObj != null)
            {
                fireEffects = fireObj.gameObject;
            }

            UpdateVisuals();
            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (sparkSystem != null) sparkSystem.Stop();

            if (TikiToki.UI.ProgressBar.Instance != null) TikiToki.UI.ProgressBar.Instance.RegisterBonfire(this);
        }

        void Update()
        {
            if (isLit) return;

            // Cooldown logic when not holding space
            if (currentProgress > 0 && !_isBeingLitThisFrame)
            {
                currentProgress -= Time.deltaTime * decreaseSpeed;

                if (currentProgress <= 0)
                {
                    currentProgress = 0;
                    if (progressBar != null) progressBar.gameObject.SetActive(false);
                }
                else
                {
                    if (progressBar != null)
                    {
                        progressBar.gameObject.SetActive(true);
                        progressBar.SetHealth(currentProgress, requiredTime);
                    }
                }
            }

            _isBeingLitThisFrame = false;
        }

        public void UpdateVisuals()
        {
            if (emptyModel != null) emptyModel.SetActive(false);
            if (woodModel != null) woodModel.SetActive(false);
            if (leavesModel != null) leavesModel.SetActive(false);
            if (completeModel != null) completeModel.SetActive(false);

            if (hasWood && hasLeaves) completeModel.SetActive(true);
            else if (hasWood) woodModel.SetActive(true);
            else if (hasLeaves) leavesModel.SetActive(true);
            else emptyModel.SetActive(true);

            if (fireEffects != null) fireEffects.SetActive(isLit);
        }

        public void TryToLight(float increment)
        {
            if (isLit || !hasWood || !hasLeaves) return;

            _isBeingLitThisFrame = true;

            if (progressBar != null && !progressBar.gameObject.activeSelf) progressBar.gameObject.SetActive(true);
            if (sparkSystem != null && !sparkSystem.isPlaying) sparkSystem.Play();

            currentProgress += increment;

            if (progressBar != null)
                progressBar.SetHealth(currentProgress, requiredTime);

            if (currentProgress >= requiredTime)
            {
                FinishLighting();
            }
        }

        public void StopLighting()
        {
            if (sparkSystem != null) sparkSystem.Stop();
            _isBeingLitThisFrame = false;
        }

        void FinishLighting()
        {
            isLit = true;
            OnBonfireLit?.Invoke();

            currentProgress = requiredTime;
            if (sparkSystem != null) sparkSystem.Stop();
            if (progressBar != null) progressBar.gameObject.SetActive(false);

            UpdateVisuals();

            if (TikiToki.UI.ProgressBar.Instance != null)
                TikiToki.UI.ProgressBar.Instance.RecalculateRateOfChange();
        }

        public void Extinguish()
        {
            if (!isLit) return;

            isLit = false;
            currentProgress = 0f;

            if (progressBar != null) progressBar.gameObject.SetActive(false);
            if (sparkSystem != null) sparkSystem.Stop();

            UpdateVisuals();

            if (TikiToki.UI.ProgressBar.Instance != null) TikiToki.UI.ProgressBar.Instance.RecalculateRateOfChange();

            Debug.Log("<color=blue>BONFIRE:</color> The wind has extinguished a bonfire.");
        }
    }
}
