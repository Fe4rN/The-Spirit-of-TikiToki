using UnityEngine;
using UnityEngine.SceneManagement;

public class GuardarNivel : MonoBehaviour
{
    void Start()
    {
        ActualizarProgresoNivel();
    }

    private void ActualizarProgresoNivel()
    {
        // 1. Obtenemos el nombre de la escena donde estamos
        string nombreEscenaActual = SceneManager.GetActiveScene().name;

        // 2. Comprobamos si es un nivel (por ejemplo, si empieza por "level")
        if (nombreEscenaActual.StartsWith("level"))
        {
            // Extraemos el número del nombre (ej: de "level3" sacamos el 3)
            string numeroStr = nombreEscenaActual.Replace("level", "");

            if (int.TryParse(numeroStr, out int nivelActual))
            {
                int maxGuardado = PlayerPrefs.GetInt("NivelMaximo", 0);

                // 3. Solo guardamos si el nivel actual es mayor al que ya teníamos
                if (nivelActual > maxGuardado)
                {
                    PlayerPrefs.SetInt("NivelMaximo", nivelActual);
                    PlayerPrefs.Save();
                    Debug.Log($"<color=green>[Progreso]</color> Nuevo nivel alcanzado: {nivelActual}");
                }
            }
        }
        // Si la escena es "Historia", podrías decidir si cuenta como progreso o no
    }
}
