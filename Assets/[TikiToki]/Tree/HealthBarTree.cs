using UnityEngine;
using UnityEngine.UI;

public class TreeHealthBarTree : MonoBehaviour
{
    public Slider slider;
    [Header("Ajustes de Posición")]
    public float altura = 1f;      // Altura sobre el suelo
    public float distanciaEje = 2f; // Qué tan alejada está del centro del tronco

    private Transform _camTransform;
    private Transform _treeTransform;

    void Start()
    {
        if (Camera.main != null) _camTransform = Camera.main.transform;

        // El padre es el objeto del árbol que rotó el Spawner
        _treeTransform = transform.parent;
    }

    public void SetHealth(float current, float max)
    {
        slider.maxValue = max;
        slider.value = current;
    }

    void LateUpdate()
    {
        if (_camTransform == null || _treeTransform == null) return;

        // 1. ROTACIÓN: Que siempre mire a la cámara (Billboard)
        transform.rotation = _camTransform.rotation;

        // 2. POSICIÓN DINÁMICA: 
        // Calculamos la dirección desde el árbol hacia la cámara (solo en el plano XZ)
        Vector3 direccionHaciaCamara = (_camTransform.position - _treeTransform.position).normalized;
        direccionHaciaCamara.y = 0; // Evitamos que la barra suba o baje por la posición de la cámara

        // Reposicionamos la barra para que siempre esté "delante" del tronco según el jugador
        transform.position = _treeTransform.position + (Vector3.up * altura) + (direccionHaciaCamara * distanciaEje);
    }
}