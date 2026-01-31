using UnityEngine;

public class SunsetLogic : MonoBehaviour
{
    [Header("Configuración del Tiempo")]
    public float durationMinutes = 4f;

    [Header("Ángulos de Rotación (Eje X)")]
    public float startAngle = 30f;
    public float endAngle = -170f;

    private float _elapsedTime = 0f;
    private float _durationSeconds;

    // Guardamos las rotaciones fijas para que no bailen
    private float _fixedY;
    private float _fixedZ;

    private WinLose winLoseManager;

    void Start()
    {
        winLoseManager = FindFirstObjectByType<WinLose>();

        if (winLoseManager != null)
        {
            _durationSeconds = winLoseManager.tiempoMaximo;
        }else
        {
            _durationSeconds = durationMinutes * 60f;
        }

        // Guardamos la orientación Y y Z original del sol al empezar
        // para que no cambien nunca durante el proceso
        _fixedY = transform.eulerAngles.y;
        _fixedZ = transform.eulerAngles.z;

        // Forzamos la posición inicial
        transform.rotation = Quaternion.Euler(startAngle, _fixedY, _fixedZ);
    }

    void Update()
    {
        if (_elapsedTime < _durationSeconds)
        {
            _elapsedTime += Time.deltaTime;

            // Calculamos el progreso de 0 a 1
            float t = _elapsedTime / _durationSeconds;

            // Usamos Mathf.LerpUnclamped por si los valores son muy distantes, 
            // pero con t de 0 a 1 el Lerp normal sobra.
            float currentX = Mathf.Lerp(startAngle, endAngle, t);

            // CLAVE: Aplicamos la rotación usando los valores fijos de Y y Z.
            // No leemos transform.eulerAngles aquí dentro para evitar el jitter.
            transform.rotation = Quaternion.Euler(currentX, _fixedY, _fixedZ);
        }
        else
        {
            // Aseguramos que termine exactamente en el ángulo final
            transform.rotation = Quaternion.Euler(endAngle, _fixedY, _fixedZ);
        }
    }
}