using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // --- LÓGICA PARA EL VIENTO (CORREGIDA) ---

    public void GenerarAlgunasHojas()
    {
        ActualizarEstadoEscena();
        int cantidadATraer = Random.Range(1, 4);
        int contador = 0;

        // Cambiamos foreach por for para poder manejar nulos con seguridad
        for (int i = 0; i < poolHojas.Count; i++)
        {
            if (contador >= cantidadATraer) break;

            // FIX: Si el objeto fue destruido (recogido), lo re-instanciamos en el pool
            if (poolHojas[i] == null)
            {
                poolHojas[i] = InstanciarIndividual(hojasPrefab, PrimitiveType.Cube, "HojasSecas_RE");
            }

            if (poolHojas[i].activeSelf) continue;

            if (IntentarPosicionarObjeto(poolHojas[i], true, false))
            {
                contador++;
            }
        }
    }

    public void GenerarAlgunosArboles()
    {
        ActualizarEstadoEscena();
        int cantidadAAsignar = Random.Range(1, 4);
        int contador = 0;

        for (int i = 0; i < poolArboles.Count; i++)
        {
            if (contador >= cantidadAAsignar) break;

            // FIX: Manejo de nulos si el árbol fue destruido por completo
            if (poolArboles[i] == null)
            {
                poolArboles[i] = InstanciarIndividual(arbolPrefab, PrimitiveType.Cylinder, "Arbol_RE");
            }

            if (poolArboles[i].activeSelf) continue;

            if (IntentarPosicionarObjeto(poolArboles[i], false, true))
            {
                contador++;
            }
        }
    }

    // --- LÓGICA DE POSICIONAMIENTO ---

    private bool IntentarPosicionarObjeto(GameObject obj, bool esHoja, bool esArbol)
    {
        if (obj == null) return false; // Failsafe extra

        int intentos = 0;
        while (intentos < 100)
        {
            intentos++;
            float rx = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
            float rz = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
            Vector3 candidata = transform.position + new Vector3(rx, 0, rz);

            if (EsPosicionValida(candidata))
            {
                ActivarObjetoDelPool(obj, candidata, esArbol);
                return true;
            }
        }
        return false;
    }

    private void ActivarObjetoDelPool(GameObject obj, Vector3 posicion, bool esArbol)
    {
        if (obj == null) return;

        obj.transform.position = posicion;
        Vector3 escalaObjetivo = obj.transform.localScale;

        if (esArbol)
        {
            Tree treeScript = obj.GetComponentInChildren<Tree>();
            if (treeScript != null) treeScript.ResetearArbol();
        }

        obj.SetActive(true);
        posicionesOcupadas.Add(new Vector3(posicion.x, 0, posicion.z));
        StartCoroutine(EfectoCrecimiento(obj.transform, escalaObjetivo));
    }

    private IEnumerator EfectoCrecimiento(Transform t, Vector3 escalaFinal)
    {
        if (t == null) yield break;

        float duracion = 1.5f;
        float tiempo = 0;
        t.localScale = Vector3.zero;

        while (tiempo < duracion)
        {
            if (t == null) yield break; // Si se destruye durante el crecimiento
            tiempo += Time.deltaTime;
            t.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, tiempo / duracion);
            yield return null;
        }
        if (t != null) t.localScale = escalaFinal;
    }

    // --- MÉTODOS DE SOPORTE (Limpieza de Nulos) ---

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
            // Verificamos que el hijo no sea nulo antes de preguntar si está activo
            if (hijo != null && hijo.gameObject.activeSelf)
                posicionesOcupadas.Add(new Vector3(hijo.position.x, 0, hijo.position.z));
        }
    }

    private bool EsPosicionValida(Vector3 pos)
    {
        foreach (Vector3 ocupada in posicionesOcupadas)
        {
            if (Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(ocupada.x, ocupada.z)) < radioExclusion)
                return false;
        }
        return true;
    }

    // Métodos para generar grupos (incluyen limpieza de nulos)
    public void GenerarSoloArboles() { ActualizarEstadoEscena(); ActivarGrupoAleatorio(poolArboles, maxArboles, true); }
    public void GenerarSoloHojas() { ActualizarEstadoEscena(); ActivarGrupoAleatorio(poolHojas, maxHojas, false); }

    private void ActivarGrupoAleatorio(List<GameObject> listaPool, int limite, bool esArbol)
    {
        int activos = 0;
        for (int i = 0; i < listaPool.Count; i++)
        {
            if (listaPool[i] != null && listaPool[i].activeSelf) activos++;
        }

        for (int i = 0; i < listaPool.Count; i++)
        {
            if (activos >= limite) break;

            if (listaPool[i] == null) // Limpieza si el objeto fue destruido
            {
                GameObject prefab = esArbol ? arbolPrefab : hojasPrefab;
                PrimitiveType type = esArbol ? PrimitiveType.Cylinder : PrimitiveType.Cube;
                listaPool[i] = InstanciarIndividual(prefab, type, "Reciclado_" + i);
            }

            if (listaPool[i].activeSelf) continue;

            if (IntentarPosicionarObjeto(listaPool[i], !esArbol, esArbol)) activos++;
        }
    }

    public void GenerarPoligonoHogueras()
    {
        ActualizarEstadoEscena();
        float anguloPaso = 360f / cantidadHogueras;
        for (int i = 0; i < poolHogueras.Count; i++)
        {
            if (poolHogueras[i] == null)
            {
                poolHogueras[i] = InstanciarIndividual(hogueraPrefab, PrimitiveType.Sphere, "Hoguera_RE");
            }
            if (poolHogueras[i].activeSelf) continue;

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