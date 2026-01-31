using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaskMeteorAttackState : MaskState
{
    [Header("Ajustes del Ataque")]
    [SerializeField] private float anticipationDuration = 1.5f;
    [SerializeField] private float attackDuration = 3f;
    private float attackCounter;
    private MeteorPhase currentPhase;

    [Header("Animación de Mandíbula")]
    [SerializeField] private float jawOpenDistance = 1.2f;
    [SerializeField] private float jawChompSpeed = 8f;
    [SerializeField] private int chompCount = 3;
    private Vector3 jawInitialPosition;
    private Vector3 jawTargetPosition;
    private float chompTimer;
    private int currentChomps;
    private bool isOpening;

    [Header("Configuración de Meteoritos")]
    [SerializeField] private GameObject[] meteorPrefabs;
    [SerializeField] private Transform groundReference;
    [SerializeField] private int minMeteors = 10;
    [SerializeField] private int maxMeteors = 20;
    [SerializeField] private float meteorHeight = 10f;
    [SerializeField] private float damageRadius = 1f;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private Color warningColor = new Color(1f, 0.2f, 0f, 0.5f);

    [SerializeField] private AudioClip audioQ;

    private List<GameObject> activeWarnings = new List<GameObject>();
    private bool meteorsSpawned = false;

    protected override void StateEnter()
    {
        Debug.Log("Entering Meteor State");
        attackCounter = anticipationDuration;
        currentPhase = MeteorPhase.Anticipation;
        currentChomps = 0;
        isOpening = true;
        chompTimer = 0;

        // Inicializar posición de mandíbula
        if (machine.JawTransform != null)
        {
            jawInitialPosition = machine.JawTransform.localPosition;
            jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
        }

        if (audioQ != null && machine.GetComponent<AudioSource>() != null)
        {
            machine.GetComponent<AudioSource>().PlayOneShot(audioQ);
        }
    }

    protected override void StateUpdate()
    {
        machine.MirrorPlayerPosition();
        machine.LookAtPlayer();

        switch (currentPhase)
        {
            case MeteorPhase.Anticipation:
                attackCounter -= Time.deltaTime;

                // Hacer chomping (abrir y cerrar 3 veces)
                if (currentChomps < chompCount && machine.JawTransform != null)
                {
                    machine.JawTransform.localPosition = Vector3.Lerp(
                        machine.JawTransform.localPosition,
                        jawTargetPosition,
                        Time.deltaTime * jawChompSpeed
                    );

                    // Detectar cuando alcanzamos el objetivo
                    float distance = Vector3.Distance(machine.JawTransform.localPosition, jawTargetPosition);
                    if (distance < 0.05f)
                    {
                        if (isOpening)
                        {
                            // Cambiar a cerrar
                            isOpening = false;
                            jawTargetPosition = jawInitialPosition;
                        }
                        else
                        {
                            // Cambiar a abrir y contar el chomp
                            isOpening = true;
                            currentChomps++;
                            if (currentChomps < chompCount)
                            {
                                jawTargetPosition = jawInitialPosition + Vector3.back * jawOpenDistance;
                            }
                        }
                    }
                }

                if (attackCounter <= 0)
                {
                    currentPhase = MeteorPhase.Attacking;
                    attackCounter = attackDuration;

                    // Asegurar que la mandíbula esté cerrada
                    if (machine.JawTransform != null)
                    {
                        machine.JawTransform.localPosition = jawInitialPosition;
                    }

                    // Iniciar lluvia de meteoritos
                    StartMeteorRain();
                }
                break;

            case MeteorPhase.Attacking:
                attackCounter -= Time.deltaTime;

                if (attackCounter <= 0)
                    machine.SetState(machine.idleState.Value);
                break;
        }
    }

    protected override void StateExit()
    {
        // Cerrar mandíbula
        if (machine.JawTransform != null)
        {
            machine.JawTransform.localPosition = jawInitialPosition;
        }

        // Limpiar warnings activos
        CleanupWarnings();
        meteorsSpawned = false;
    }

    private void StartMeteorRain()
    {
        if (meteorPrefabs == null || meteorPrefabs.Length == 0)
        {
            Debug.LogError("No meteor prefabs assigned!");
            return;
        }

        if (groundReference == null)
        {
            Debug.LogError("Ground reference not assigned!");
            return;
        }

        int meteorCount = Random.Range(minMeteors, maxMeteors + 1);

        for (int i = 0; i < meteorCount; i++)
        {
            float delay = Random.Range(0f, attackDuration - warningDuration);
            StartCoroutine(SpawnMeteorWithWarning(delay));
        }

        meteorsSpawned = true;
    }

    private IEnumerator SpawnMeteorWithWarning(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        Vector3 targetPosition = GetRandomPositionOnGround();

        // Crear warning visual
        GameObject warning = CreateWarningIndicator(targetPosition);
        activeWarnings.Add(warning);

        // Esperar el tiempo de aviso
        yield return new WaitForSeconds(warningDuration);

        // Destruir el warning
        if (warning != null)
        {
            activeWarnings.Remove(warning);
            Destroy(warning);
        }

        // Spawear el meteorito
        SpawnMeteor(targetPosition);
    }

    private Vector3 GetRandomPositionOnGround()
    {
        if (groundReference == null) return Vector3.zero;

        // Obtener los bounds del suelo
        Renderer groundRenderer = groundReference.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            Bounds bounds = groundRenderer.bounds;

            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);

            return new Vector3(randomX, bounds.max.y, randomZ);
        }

        // Fallback si no hay renderer
        return groundReference.position + new Vector3(
            Random.Range(-10f, 10f),
            0f,
            Random.Range(-10f, 10f)
        );
    }

    private GameObject CreateWarningIndicator(Vector3 position)
    {
        // Crear un cilindro aplanado como indicador
        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        warning.name = "MeteorWarning";

        // Posicionar ligeramente sobre el suelo
        warning.transform.position = position + Vector3.up * 0.05f;
        warning.transform.localScale = new Vector3(damageRadius * 2, 0.01f, damageRadius * 2);

        // Configurar material
        Renderer renderer = warning.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = warningColor;
        renderer.material.SetFloat("_Mode", 3); // Transparent mode
        renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        renderer.material.SetInt("_ZWrite", 0);
        renderer.material.DisableKeyword("_ALPHATEST_ON");
        renderer.material.EnableKeyword("_ALPHABLEND_ON");
        renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        renderer.material.renderQueue = 3000;

        // Remover collider
        Collider collider = warning.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        return warning;
    }

    private void SpawnMeteor(Vector3 targetPosition)
    {
        // Seleccionar prefab aleatorio
        GameObject meteorPrefab = meteorPrefabs[Random.Range(0, meteorPrefabs.Length)];

        // Posición de spawn en el cielo
        Vector3 spawnPosition = targetPosition + Vector3.up * meteorHeight;

        // Instanciar meteorito
        //GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Random.rotation);
        GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);

        // Asegurar que tiene Rigidbody
        Rigidbody rb = meteor.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = meteor.AddComponent<Rigidbody>();
        }
        rb.useGravity = true;

        // Agregar el script Meteor si no lo tiene
        Meteor meteorScript = meteor.GetComponent<Meteor>();
        if (meteorScript == null)
        {
            meteorScript = meteor.AddComponent<Meteor>();
        }
        meteorScript.Initialize(damageRadius);
    }

    private void CleanupWarnings()
    {
        foreach (GameObject warning in activeWarnings)
        {
            if (warning != null)
                Destroy(warning);
        }
        activeWarnings.Clear();
    }
}

public enum MeteorPhase { Anticipation, Attacking }
