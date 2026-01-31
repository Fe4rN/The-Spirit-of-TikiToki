using UnityEngine;

public class ScriptVida : MonoBehaviour
{
    // Ya no necesita maxVida ni vida actual, eso lo lleva WinLose
    public bool damage = false;

    void Update()
    {
        // Esto es solo para pruebas, si la casilla damage se marca, resta vida
        if (damage)
        {
            reducirSalud();
            damage = false; // Lo reseteamos para que no reste vida cada frame
        }
    }

    public void reducirSalud()
    {
        // LLAMADA A TU CÓDIGO:
        if (WinLose.Instance != null)
        {
            WinLose.Instance.ModificarVidas(-1);
        }
    }
}