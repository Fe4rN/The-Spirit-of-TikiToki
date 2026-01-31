using UnityEngine;

[CreateAssetMenu(fileName = "NombreEstado", menuName = "Estados/Nombre")]
public class NombreEstado : ScriptableObject
{
    [SerializeField]
    private string m_Value;

    public string Value { get { return m_Value; } }
}