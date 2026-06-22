using UnityEngine;

namespace TikiToki.Gameplay.Boss
{
    public class MaskState : MonoBehaviour
    {
        [SerializeField] private NombreEstado stateName;
        [SerializeField] private bool initial = false;
        protected MaskMachine machine;

        public string StateName
        {
            get
            {
                if (stateName == null) return "";
                return stateName.Value;
            }
        }

        public bool Initial { get => initial; set => initial = value; }

        void Awake()
        {
            machine = GetComponent<MaskMachine>();
            OnAwake();
        }

        protected virtual void OnAwake()
        {
            if (machine == null)
            {
                Debug.LogError("Machine not assigned in state " + this.name);
                return;
            }
        }

        public void OnEnter()
        {
            StateEnter();
        }

        public void OnUpdate()
        {
            StateUpdate();
        }

        public void OnExit()
        {
            StateExit();
        }

        protected virtual void StateEnter() { }
        protected virtual void StateUpdate() { }
        protected virtual void StateExit() { }
    }
}
