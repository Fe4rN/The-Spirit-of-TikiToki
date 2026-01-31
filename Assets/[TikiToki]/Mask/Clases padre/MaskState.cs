using UnityEngine;

public class MaskState : MonoBehaviour
{
    [SerializeField] NombreEstado stateName;
    [SerializeField] bool initial = false;
    protected MaskMachine machine;

    public string StateName
    {
        get
        {
            if (!stateName) return "";
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
            Debug.LogError("Maquina no asignada en el estado " + this.name);
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
