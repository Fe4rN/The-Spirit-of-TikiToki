using UnityEngine;
using UnityEngine.Events;

public class MaskMachine : MonoBehaviour
{
    public Transform PlayerTransform;
    [SerializeField] private Transform MaskTransform;

    [Header("Movimiento por defecto")]
    [SerializeField] private float smoothTime = 0.2f;
    private float velocityX;

    [Header("Cooldown de Ataque")]
    public float minAttackCooldown;
    public float maxAttackCooldown;


    #region Cosas de estado

    public UnityEvent<string> OnStateChanged;

    private MaskState[] states;
    private MaskState currentState;

    public MaskState MaskState
    {
        get => currentState;
        set
        {
            if (currentState == value) return;

            if (currentState != null)
                currentState.OnExit();

            currentState = value;

            foreach (MaskState state in states)
                state.enabled = (state == currentState);

            if (currentState != null)
                currentState.OnEnter();

            OnStateChanged?.Invoke(currentState.StateName);
        }
    }

    void Awake()
    {
        states = GetComponents<MaskState>();
        foreach (MaskState state in states)
        {
            if (state.Initial)
            {
                MaskState = state;

            }
        }

        if (MaskState == null && states.Length > 0)
        {
            MaskState = states[0];
        }

        PlayerTransform = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (currentState != null)
        {
            currentState.OnUpdate();
        }
    }

    public void SetState(string name)
    {
        foreach (MaskState state in states)
        {
            if (state.StateName == name)
            {
                MaskState = state;
                return;
            }
        }

        Debug.LogError($"No hay un estado {name}");
    }

    #endregion

    public void MirrorPlayerPosition()
    {
        if (!PlayerTransform) return;

        Vector3 pos = transform.position;

        pos.x = Mathf.SmoothDamp(pos.x, PlayerTransform.position.x, ref velocityX, smoothTime);

        transform.position = pos;
    }

    public void LookAtPlayer()
    {
        if(!PlayerTransform) return;

        transform.LookAt(PlayerTransform);
    }
}
