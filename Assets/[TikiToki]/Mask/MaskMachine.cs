using UnityEngine;
using UnityEngine.Events;

namespace TikiToki.Gameplay.Boss
{
    public class MaskMachine : MonoBehaviour
    {
        public Transform playerTransform;
        [SerializeField] private Transform maskTransform;

        [Header("Animation")]
        public Transform jawTransform; // Reference to Beige.001

        [Header("Default Movement")]
        [SerializeField] private float smoothTime = 0.2f;
        private float _velocityX;

        [Header("States")]
        public NombreEstado idleState;
        public NombreEstado laserState;
        public NombreEstado windState;
        public NombreEstado meteorState;
        public NombreEstado screamState;

        #region State Machine Logic

        public UnityEvent<string> OnStateChanged;

        private MaskState[] _states;
        private MaskState _currentState;

        public MaskState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState == value) return;

                if (_currentState != null)
                    _currentState.OnExit();

                _currentState = value;

                foreach (MaskState state in _states)
                    state.enabled = (state == _currentState);

                if (_currentState != null)
                    _currentState.OnEnter();

                OnStateChanged?.Invoke(_currentState.StateName);
            }
        }

        void Awake()
        {
            _states = GetComponents<MaskState>();
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            foreach (MaskState state in _states)
            {
                if (state.Initial)
                {
                    CurrentState = state;
                }
            }

            if (CurrentState == null && _states.Length > 0)
            {
                CurrentState = _states[0];
            }
        }

        void Start()
        {
            var cc = GameObject.FindFirstObjectByType<CharacterController>();
            if (cc != null)
            {
                playerTransform = cc.transform;
            }
        }

        void Update()
        {
            if (_currentState != null)
            {
                _currentState.OnUpdate();
            }
        }

        public void SetState(string name)
        {
            foreach (MaskState state in _states)
            {
                if (state.StateName == name)
                {
                    CurrentState = state;
                    return;
                }
            }

            Debug.LogError($"No state with name {name}");
        }

        #endregion

        public void MirrorPlayerPosition()
        {
            if (!playerTransform) return;

            Vector3 pos = transform.position;
            pos.x = Mathf.SmoothDamp(pos.x, playerTransform.position.x, ref _velocityX, smoothTime);
            transform.position = pos;
        }

        public void LookAtPlayer()
        {
            // Optional face player logic
        }
    }
}
