using UnityEngine;

namespace TikiToki.Gameplay.Boss
{
    [CreateAssetMenu(fileName = "StateName", menuName = "States/StateName")]
    public class NombreEstado : ScriptableObject
    {
        [SerializeField]
        private string m_Value;

        public string Value { get { return m_Value; } }
    }
}
