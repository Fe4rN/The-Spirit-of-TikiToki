using UnityEngine;
using System.Collections.Generic;

public class SpawnerMaster : MonoBehaviour
{
    [Header("Área de Generación")]
    public Vector2 areaSize = new Vector2(15f, 15f);
    [Range(0.5f, 5f)] public float radioExclusion = 2.5f;

    [Header("Configuración Hogueras")]
    public GameObject hogueraPrefab;
    public int cantidadHogueras = 5; // Aquí defines cuántas forman el polígono
    public float radioPentagono = 4f;

    [Header("Configuración Árboles")]
    public GameObject arbolPrefab;
    public int maxArboles = 3;

    [Header("Configuración Hojas")]
    public GameObject hojasPrefab;
    public int maxHojas = 4;

    // Listas para el Object Pooling
    private List<GameObject> poolHogueras = new List<GameObject>();
    private List<GameObject> poolArboles = new List<GameObject>();
    private List<GameObject> poolHojas = new List<GameObject>();

    private List<Vector3> posicionesOcupadas = new List<Vector3>();

    void Awake()
    {
        // Inicializamos los pools según las cantidades definidas en el inspector
        CrearPool(poolHogueras, hogueraPrefab, cantidadHogueras, PrimitiveType.Sphere, "Hoguera_");
        CrearPool(poolArboles, arbolPrefab, maxArboles, PrimitiveType.Cylinder, "Arbol_");
        CrearPool(poolHojas, hojasPrefab, maxHojas, PrimitiveType.Cube, "HojasSecas_");
    }

    void Start()
    {
        GenerarTodo();
    }

    // --- MÉTODOS PÚBLICOS DE GENERACIÓN ---

    public void GenerarTodo()
    {
        ActualizarEstadoEscena();
        GenerarPoligonoHogueras();
        GenerarSoloArboles();
        GenerarSoloHojas();
    }

    public void GenerarPoligonoHogueras()
    {
        ActualizarEstadoEscena();
        float anguloPaso = 360f / cantidadHogueras;

        for (int i = 0; i < cantidadHogueras; i++)
        {
            if (poolHogueras[i].activeSelf) continue;

            // 90f es para que la primera hoguera siempre esté en el punto más alto
            float angulo = (i * anguloPaso + 90f) * Mathf.Deg2Rad;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(angulo), 0, Mathf.Sin(angulo)) * radioPentagono;

            ActivarObjetoDelPool(poolHogueras[i], pos);
        }
    }

    public void GenerarAlgunosArboles()
    {
        ActualizarEstadoEscena();

        int cantidadAAsignar = Random.Range(1, 3); // Intentará crear 1 o 2 árboles nuevos
        int contador = 0;

        foreach (GameObject obj in poolArboles)
        {
            if (contador >= cantidadAAsignar) break; // Ya hemos creado los que queríamos
            if (obj.activeSelf) continue;            // Si este ya está en el mapa, buscamos el siguiente del pool

            // Intentamos buscarle un sitio
            if (IntentarPosicionarObjeto(obj))
            {
                contador++;
            }
        }
    }

    private bool IntentarPosicionarObjeto(GameObject obj)
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
                ActivarObjetoDelPool(obj, candidata);
                return true;
            }
        }
        return false;
    }

    public void GenerarSoloArboles()
    {
        ActualizarEstadoEscena();
        ActivarGrupoAleatorio(poolArboles);
    }

    public void GenerarSoloHojas()
    {
        ActualizarEstadoEscena();
        ActivarGrupoAleatorio(poolHojas);
    }

    // --- LÓGICA DE POOLING Y POSICIONAMIENTO ---

    private void CrearPool(List<GameObject> listaPool, GameObject prefab, int cantidad, PrimitiveType fallback, string nombreBase)
    {
        for (int i = 0; i < cantidad; i++)
        {
            GameObject obj;
            if (prefab != null)
            {
                obj = Instantiate(prefab, transform);
            }
            else
            {
                obj = GameObject.CreatePrimitive(fallback);
                obj.transform.parent = transform;
                if (fallback == PrimitiveType.Cube) obj.transform.localScale = new Vector3(1, 0.1f, 1);
            }

            obj.name = nombreBase + i;
            obj.SetActive(false);
            listaPool.Add(obj);
        }
    }

    private void ActivarGrupoAleatorio(List<GameObject> listaPool)
    {
        foreach (GameObject obj in listaPool)
        {
            if (obj.activeSelf) continue;

            int intentos = 0;
            while (intentos < 150)
            {
                intentos++;
                float rx = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
                float rz = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
                Vector3 candidata = transform.position + new Vector3(rx, 0, rz);

                if (EsPosicionValida(candidata))
                {
                    ActivarObjetoDelPool(obj, candidata);
                    break;
                }
            }
        }
    }

    private void ActivarObjetoDelPool(GameObject obj, Vector3 posicion)
    {
        obj.transform.position = posicion;
        obj.SetActive(true);
        posicionesOcupadas.Add(posicion);
    }

    private void ActualizarEstadoEscena()
    {
        posicionesOcupadas.Clear();
        posicionesOcupadas.Add(transform.position); // Bloquear el centro siempre

        // Registrar posiciones de todos los objetos que estén encendidos actualmente
        foreach (Transform hijo in transform)
        {
            if (hijo.gameObject.activeSelf)
            {
                posicionesOcupadas.Add(hijo.position);
            }
        }
    }

    private bool EsPosicionValida(Vector3 pos)
    {
        foreach (Vector3 ocupada in posicionesOcupadas)
        {
            if (Vector3.Distance(pos, ocupada) < radioExclusion) return false;
        }
        return true;
    }

    // --- VISUALIZACIÓN ---

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0, areaSize.y));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioPentagono);
    }
}