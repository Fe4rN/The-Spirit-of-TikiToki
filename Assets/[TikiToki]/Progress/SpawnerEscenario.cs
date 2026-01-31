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

    // --- LÓGICA DE ACTIVACIÓN UNIFICADA ---

    private void ActivarObjetoDelPool(GameObject obj, Vector3 posicion, bool esArbol)
    {
        obj.transform.position = posicion;

        // Capturamos la escala real del prefab ANTES de ponerla a cero para el efecto
        Vector3 escalaObjetivo = obj.transform.localScale;

        if (esArbol)
        {
            Tree treeScript = obj.GetComponent<Tree>();
            if (treeScript == null) treeScript = obj.GetComponentInChildren<Tree>();
            if (treeScript != null) treeScript.ResetearArbol();
        }

        obj.SetActive(true);
        posicionesOcupadas.Add(new Vector3(posicion.x, 0, posicion.z));

        // Iniciamos el crecimiento para ambos tipos de objeto
        StartCoroutine(EfectoCrecimiento(obj.transform, escalaObjetivo));
    }

    private IEnumerator EfectoCrecimiento(Transform t, Vector3 escalaFinal)
    {
        float duracion = 1.5f;
        float tiempo = 0;

        // Empezamos desde escala cero
        t.localScale = Vector3.zero;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            // Escalamos suavemente hasta la escala original (ej. 0.25 para hojas)
            t.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, tiempo / duracion);
            yield return null;
        }
        t.localScale = escalaFinal;
    }

    public void GenerarAlgunosArboles()
    {
        ActualizarEstadoEscena();
        int cantidadAAsignar = Random.Range(1, 4);
        int contador = 0;

        foreach (GameObject obj in poolArboles)
        {
            if (contador >= cantidadAAsignar) break;
            if (obj.activeSelf) continue;

            // CORRECCIÓN: Añadidos los parámetros bool (esHoja = false, esArbol = true)
            if (IntentarPosicionarObjeto(obj, false, true))
            {
                contador++;
            }
        }
    }

    // --- LÓGICA PARA EL VIENTO ---

    public void GenerarAlgunasHojas()
    {
        // 1. Refrescamos qué posiciones están libres en el mapa
        ActualizarEstadoEscena();

        // 2. Definimos una cantidad aleatoria de hojas nuevas [1 a 3]
        int cantidadATraer = Random.Range(1, 4);
        int contador = 0;

        // 3. Buscamos hojas inactivas en el pool para "traerlas" al mapa
        foreach (GameObject hoja in poolHojas)
        {
            if (contador >= cantidadATraer) break;
            if (hoja.activeSelf) continue;

            // Intentamos posicionarla en un lugar válido
            // esHoja = true, esArbol = false
            if (IntentarPosicionarObjeto(hoja, true, false))
            {
                contador++;
            }
        }
    }

    // Cambia la firma para aceptar los booleanos de control
    private bool IntentarPosicionarObjeto(GameObject obj, bool esHoja, bool esArbol)
    {
        int intentos = 0;
        while (intentos < 100)
        {
            intentos++;
            float rx = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
            float rz = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
            Vector3 candidata = transform.position + new Vector3(rx, 0, rz);

            if (EsPosicionValida(candidata))
            {
                // CORRECCIÓN: Ahora pasamos 'esArbol' al siguiente método
                ActivarObjetoDelPool(obj, candidata, esArbol);
                return true;
            }
        }
        return false;
    }

    // --- MÉTODOS DE SOPORTE Y POOLING ---

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
            // Usamos Vector2 para ignorar la altura en el cálculo de proximidad
            if (Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(ocupada.x, ocupada.z)) < radioExclusion)
                return false;
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