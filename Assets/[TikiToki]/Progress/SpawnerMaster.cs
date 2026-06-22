using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TikiToki.Gameplay.Environment
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "SpawnerEscenario")]
    public class SpawnerMaster : MonoBehaviour
    {
        [Header("Generation Area")]
        public Vector2 areaSize = new Vector2(15f, 15f);
        [UnityEngine.Serialization.FormerlySerializedAs("radioExclusion")]
        [Range(0.5f, 5f)] public float exclusionRadius = 2.5f;

        [Header("Maximum Limits")]
        [UnityEngine.Serialization.FormerlySerializedAs("maxArboles")]
        public int maxTrees = 3;
        [UnityEngine.Serialization.FormerlySerializedAs("maxHojas")]
        public int maxLeaves = 4;
        [UnityEngine.Serialization.FormerlySerializedAs("maxPiedras")]
        public int maxStones = 6;

        [Header("Prefabs")]
        [UnityEngine.Serialization.FormerlySerializedAs("hogueraPrefab")]
        public GameObject bonfirePrefab;
        [UnityEngine.Serialization.FormerlySerializedAs("arbolPrefab")]
        public GameObject treePrefab;
        [UnityEngine.Serialization.FormerlySerializedAs("hojasPrefab")]
        public GameObject leavesPrefab;
        [UnityEngine.Serialization.FormerlySerializedAs("piedra1Prefab")]
        public GameObject rock1Prefab;
        [UnityEngine.Serialization.FormerlySerializedAs("piedra2Prefab")]
        public GameObject rock2Prefab;

        [Header("Bonfire Configuration")]
        [UnityEngine.Serialization.FormerlySerializedAs("cantidadHogueras")]
        public int bonfireCount = 5;
        [UnityEngine.Serialization.FormerlySerializedAs("radioPentagono")]
        public float pentagonRadius = 4f;

        [Header("Height Settings")]
        [UnityEngine.Serialization.FormerlySerializedAs("alturaHojas")]
        public float leavesHeight = 0.36f;

        private List<GameObject> _bonfirePool = new List<GameObject>();
        private List<GameObject> _treePool = new List<GameObject>();
        private List<GameObject> _leavesPool = new List<GameObject>();
        private List<GameObject> _stonePool = new List<GameObject>();
        private List<Vector3> _occupiedPositions = new List<Vector3>();

        void Awake()
        {
            CreatePool(_bonfirePool, bonfirePrefab, bonfireCount, PrimitiveType.Sphere, "Bonfire_");
            CreatePool(_treePool, treePrefab, maxTrees, PrimitiveType.Cylinder, "Tree_");
            CreatePool(_leavesPool, leavesPrefab, maxLeaves, PrimitiveType.Cube, "Leaves_");

            for (int i = 0; i < maxStones; i++)
            {
                GameObject prefab = (i % 2 == 0) ? rock1Prefab : rock2Prefab;
                _stonePool.Add(InstantiateIndividual(prefab, PrimitiveType.Sphere, "Stone_"));
            }
        }

        void Start()
        {
            GenerateAll();
        }

        public void GenerateAll()
        {
            UpdateSceneState();
            GenerateBonfirePolygon();
            GenerateOnlyTrees();
            GenerateOnlyLeaves();
            GenerateOnlyStones();
        }

        public void GenerateOnlyStones()
        {
            UpdateSceneState();
            int activeCount = 0;
            for (int i = 0; i < _stonePool.Count; i++)
            {
                if (_stonePool[i] != null && _stonePool[i].activeSelf) activeCount++;
            }

            for (int i = 0; i < _stonePool.Count; i++)
            {
                if (activeCount >= maxStones) break;

                if (_stonePool[i] == null)
                {
                    GameObject prefab = (Random.value > 0.5f) ? rock1Prefab : rock2Prefab;
                    _stonePool[i] = InstantiateIndividual(prefab, PrimitiveType.Sphere, "Stone_RE");
                }

                if (_stonePool[i].activeSelf) continue;

                if (TryToPositionObject(_stonePool[i], false, false)) activeCount++;
            }
        }

        public void GenerateSomeLeaves()
        {
            UpdateSceneState();
            int countToBring = Random.Range(1, 4);
            int counter = 0;
            for (int i = 0; i < _leavesPool.Count; i++)
            {
                if (counter >= countToBring) break;
                if (_leavesPool[i] == null) _leavesPool[i] = InstantiateIndividual(leavesPrefab, PrimitiveType.Cube, "Leaves_RE");
                if (_leavesPool[i].activeSelf) continue;
                if (TryToPositionObject(_leavesPool[i], true, false)) counter++;
            }
        }

        public void GenerateSomeTrees()
        {
            UpdateSceneState();
            int countToAssign = Random.Range(1, 4);
            int counter = 0;
            for (int i = 0; i < _treePool.Count; i++)
            {
                if (counter >= countToAssign) break;
                if (_treePool[i] == null) _treePool[i] = InstantiateIndividual(treePrefab, PrimitiveType.Cylinder, "Tree_RE");
                if (_treePool[i].activeSelf) continue;
                if (TryToPositionObject(_treePool[i], false, true)) counter++;
            }
        }

        private bool TryToPositionObject(GameObject obj, bool isLeaf, bool isTree)
        {
            if (obj == null) return false;
            int attempts = 0;
            while (attempts < 100)
            {
                attempts++;
                float rx = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
                float rz = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);

                float yPos = isLeaf ? leavesHeight : 0f;
                Vector3 candidate = transform.position + new Vector3(rx, yPos, rz);

                if (IsValidPosition(candidate))
                {
                    ActivateObjectFromPool(obj, candidate, isTree);
                    return true;
                }
            }
            return false;
        }

        private void ActivateObjectFromPool(GameObject obj, Vector3 position, bool isTree)
        {
            if (obj == null) return;
            obj.transform.position = position;

            if (!isTree && !obj.name.Contains("Bonfire"))
            {
                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
            }
            else
            {
                obj.transform.rotation = Quaternion.identity;
            }

            Vector3 targetScale = obj.transform.localScale;
            if (isTree)
            {
                Tree treeScript = obj.GetComponentInChildren<Tree>();
                if (treeScript != null) treeScript.ResetearArbol();
            }

            obj.SetActive(true);
            _occupiedPositions.Add(new Vector3(position.x, 0, position.z));
            StartCoroutine(GrowthEffect(obj.transform, targetScale));
        }

        private bool IsValidPosition(Vector3 pos)
        {
            foreach (Vector3 occupied in _occupiedPositions)
            {
                if (Vector2.Distance(new Vector2(pos.x, pos.z), new Vector2(occupied.x, occupied.z)) < exclusionRadius)
                    return false;
            }
            return true;
        }

        private void UpdateSceneState()
        {
            _occupiedPositions.Clear();
            _occupiedPositions.Add(transform.position);
            foreach (Transform child in transform)
            {
                if (child != null && child.gameObject.activeSelf)
                    _occupiedPositions.Add(new Vector3(child.position.x, 0, child.position.z));
            }
        }

        private IEnumerator GrowthEffect(Transform t, Vector3 finalScale)
        {
            if (t == null) yield break;
            float duration = 1.5f;
            float time = 0;
            t.localScale = Vector3.zero;
            while (time < duration)
            {
                if (t == null) yield break;
                time += Time.deltaTime;
                t.localScale = Vector3.Lerp(Vector3.zero, finalScale, time / duration);
                yield return null;
            }
            if (t != null) t.localScale = finalScale;
        }

        private GameObject InstantiateIndividual(GameObject prefab, PrimitiveType fallback, string name)
        {
            GameObject obj = (prefab != null) ? Instantiate(prefab, transform) : GameObject.CreatePrimitive(fallback);
            obj.transform.parent = transform;
            obj.name = name;
            obj.SetActive(false);
            return obj;
        }

        private void CreatePool(List<GameObject> poolList, GameObject prefab, int amount, PrimitiveType fallback, string baseName)
        {
            for (int i = 0; i < amount; i++)
            {
                poolList.Add(InstantiateIndividual(prefab, fallback, baseName + i));
            }
        }

        public void GenerateOnlyTrees()
        {
            UpdateSceneState();
            ActivateRandomGroup(_treePool, maxTrees, true);
        }

        public void GenerateOnlyLeaves()
        {
            UpdateSceneState();
            ActivateRandomGroup(_leavesPool, maxLeaves, false);
        }

        private void ActivateRandomGroup(List<GameObject> poolList, int limit, bool isTree)
        {
            int activeCount = 0;
            for (int i = 0; i < poolList.Count; i++)
            {
                if (poolList[i] != null && poolList[i].activeSelf) activeCount++;
            }
            for (int i = 0; i < poolList.Count; i++)
            {
                if (activeCount >= limit) break;
                if (poolList[i] == null)
                {
                    GameObject prefab = isTree ? treePrefab : leavesPrefab;
                    poolList[i] = InstantiateIndividual(prefab, isTree ? PrimitiveType.Cylinder : PrimitiveType.Cube, "Recycled_" + i);
                }
                if (poolList[i].activeSelf) continue;
                if (TryToPositionObject(poolList[i], !isTree, isTree)) activeCount++;
            }
        }

        public void GenerateBonfirePolygon()
        {
            UpdateSceneState();
            float stepAngle = 360f / bonfireCount;
            for (int i = 0; i < _bonfirePool.Count; i++)
            {
                if (_bonfirePool[i] == null) _bonfirePool[i] = InstantiateIndividual(bonfirePrefab, PrimitiveType.Sphere, "Bonfire_RE");
                if (_bonfirePool[i].activeSelf) continue;
                float angle = (i * stepAngle + 90f) * Mathf.Deg2Rad;
                Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * pentagonRadius;
                ActivateObjectFromPool(_bonfirePool[i], pos, false);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 0, areaSize.y));
        }
    }
}
