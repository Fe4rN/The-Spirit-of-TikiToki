using UnityEngine;

[CreateAssetMenu(fileName = "NuevoObjeto", menuName = "Inventario/Objeto")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon; // El dibujo que aparecerá en el Canvas
    public GameObject prefab; // El modelo 3D que el personaje soltará
    public int maxStack = 1; 
}