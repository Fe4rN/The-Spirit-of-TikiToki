using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnerMaster : MonoBehaviour
{
    [Header("Área de Generación")]
    public Vector2 areaSize = new Vector2(15f, 15f);
    [Range(0.5f, 5f)] public float radioExclusion = 2.5f;

    [Header("Límites Máximos")]
    public int maxArboles = 3;
    public int maxHojas = 4;

    [Header("Prefabs")]
    public GameObject hogueraPrefab;
    public GameObject arbolPrefab;
    public GameObject hojasPrefab;

    [Header("Configuración Hogueras")]
    public int cantidadHogueras = 5;
    public float radioPentagono = 4f;

    private List<GameObject> poolHogueras = new List<GameObject>();
    private List<GameObject> poolArboles = new List<GameObject>();
    private List<GameObject> poolHojas = new List<GameObject>();
    private List<Vector3> posicionesOcupadas = new List<Vector3>();

    void Awake()
    {
        CrearPool(poolHogueras, hogueraPrefab, cantidadHogueras, PrimitiveType.Sphere, "Hoguera_");
        CrearPool(poolArboles, arbolPrefab, maxArboles, PrimitiveType.Cylinder, "Arbol_");
        CrearPool(poolHojas, hojasPrefab, maxHojas, PrimitiveType.Cube, "HojasSecas_");
    }

    void Start() { GenerarTodo(); }

    public void GenerarTodo()
    {
        ActualizarEstadoEscena();
        GenerarPoligonoHogueras();
        GenerarSoloArboles();
        GenerarSoloHojas();
    }

    // --- GENERACIÓN CON EFECTOS ---

    public void GenerarAlgunasHojas()
    {
        ActualizarEstadoEscena();
        int cantidadAñadir = Random.Range(1, 4);
        GestionarActivacionDinamica(poolHojas, cantidadAñadir, maxHojas, hojasPrefab, PrimitiveType.Cube, "HojasSecas_", true, false);
    }

    public void GenerarAlgunosArboles()
    {
        ActualizarEstadoEscena();
        int cantidadAñadir = Random.Range(1, 4);
        GestionarActivacionDinamica(poolArboles, cantidadAñadir, maxArboles, arbolPrefab, PrimitiveType.Cylinder, "Arbol_", false, true);
    }

    private void GestionarActivacionDinamica(List<GameObject> pool, int cantidadAñadir, int limiteMaximo, GameObject prefab, PrimitiveType fallback, string nombreBase, bool esHoja, bool esArbol)
    {
        int activosActuales = 0;
        foreach (GameObject g in pool) { if (g != null && g.activeSelf) activosActuales++; }

        int espacioLibre = limiteMaximo - activosActuales;
        if (espacioLibre <= 0) return;

        int cantidadRealAGenerar = Mathf.Min(cantidadAñadir, espacioLibre);
        int activadosEnEstaRonda = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            if (activadosEnEstaRonda >= cantidadRealAGenerar) return;
            if (pool[i] == null) pool[i] = InstanciarIndividual(prefab, fallback, nombreBase + i);

            if (!pool[i].activeSelf)
            {
                if (IntentarPosicionarObjeto(pool[i], esHoja, esArbol)) activadosEnEstaRonda++;
            }
        }

        while (activadosEnEstaRonda < cantidadRealAGenerar && pool.Count < limiteMaximo)
        {
            GameObject nuevo = InstanciarIndividual(prefab, fallback, nombreBase + pool.Count);
            pool.Add(nuevo);
            if (IntentarPosicionarObjeto(nuevo, esHoja, esArbol)) activadosEnEstaRonda++;
            else break;
        }
    }

    private bool IntentarPosicionarObjeto(GameObject obj, bool esHoja, bool esArbol)
    {
        int intentos = 0;
        while (intentos < 50)
        {
            intentos++;
            float rx = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
            float rz = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
            Vector3 targetPos = transform.position + new Vector3(rx, 0, rz);

            if (EsPosicionValida(targetPos))
            {
                // Si es hoja, la spawnamos arriba para que caiga
                Vector3 spawnPos = targetPos;
                if (esHoja) spawnPos.y += 10f;

                ActivarObjetoDelPool(obj, spawnPos, esArbol);
                return true;
            }
        }
        return false;
    }

    private void ActivarObjetoDelPool(GameObject obj, Vector3 posicion, bool esArbol)
    {
        obj.transform.position = posicion;

        // --- RESET DE VIDA ---
        if (esArbol)
        {
            Tree treeScript = obj.GetComponent<Tree>();
            // Si no está en el padre, búscalo en los hijos (por si acaso)
            if (treeScript == null) treeScript = obj.GetComponentInChildren<Tree>();

            if (treeScript != null)
            {
                treeScript.ResetearArbol(); // Llamamos al método que acabamos de crear
            }
        }

        obj.SetActive(true);
        posicionesOcupadas.Add(new Vector3(posicion.x, 0, posicion.z));

        if (esArbol)
        {
            StartCoroutine(EfectoCrecimiento(obj.transform));
        }
    }

    private IEnumerator EfectoCrecimiento(Transform t)
    {
        float duracion = 1.5f;
        float tiempo = 0;
        Vector3 escalaFinal = Vector3.one;
        t.localScale = Vector3.zero;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            t.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, tiempo / duracion);
            yield return null;
        }
        t.localScale = escalaFinal;
    }

    // --- MÉTODOS DE SOPORTE ---

    private GameObject InstanciarIndividual(GameObject prefab, PrimitiveType fallback, string nombre)
    {
        GameObject obj = (prefab != null) ? Instantiate(prefab, transform) : GameObject.CreatePrimitive(fallback);
        obj.transform.parent = transform;
        obj.name = nombre;
        obj.SetActive(false);
        return obj;
    }

    private void CrearPool(List<GameObject> listaPool, GameObject prefab, int cantidad, PrimitiveType fallback, string nombreBase)
    {
        for (int i = 0; i < cantidad; i++)
        {
            listaPool.Add(InstanciarIndividual(prefab, fallback, nombreBase + i));
        }
    }

    private void ActualizarEstadoEscena()
    {
        posicionesOcupadas.Clear();
        posicionesOcupadas.Add(transform.position);
        foreach (Transform hijo in transform)
        {
            if (hijo.gameObject.activeSelf)
                posicionesOcupadas.Add(new Vector3(hijo.position.x, 0, hijo.position.z));
        }
    }

    private bool EsPosicionValida(Vector3 pos)
    {
        foreach (Vector3 ocupada in posicionesOcupadas)
        {
            if (Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(ocupada.x, ocupada.z)) < radioExclusion) return false;
        }
        return true;
    }

    public void GenerarSoloArboles() { ActualizarEstadoEscena(); ActivarGrupoAleatorio(poolArboles, maxArboles, true); }
    public void GenerarSoloHojas() { ActualizarEstadoEscena(); ActivarGrupoAleatorio(poolHojas, maxHojas, false); }

    private void ActivarGrupoAleatorio(List<GameObject> listaPool, int limite, bool esArbol)
    {
        int activos = 0;
        foreach (GameObject obj in listaPool) { if (obj != null && obj.activeSelf) activos++; }

        foreach (GameObject obj in listaPool)
        {
            if (activos >= limite) break;
            if (obj == null || obj.activeSelf) continue;
            if (IntentarPosicionarObjeto(obj, !esArbol, esArbol)) activos++;
        }
    }

    public void GenerarPoligonoHogueras()
    {
        ActualizarEstadoEscena();
        float anguloPaso = 360f / cantidadHogueras;
        for (int i = 0; i < cantidadHogueras; i++)
        {
            if (poolHogueras[i] == null || poolHogueras[i].activeSelf) continue;
            float angulo = (i * anguloPaso + 90f) * Mathf.Deg2Rad;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(angulo), 0, Mathf.Sin(angulo)) * radioPentagono;
            ActivarObjetoDelPool(poolHogueras[i], pos, false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0, areaSize.y));
    }
}