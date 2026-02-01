using UnityEngine;

[System.Serializable]
public class PasoHistoria
{
    public Sprite imagenComic;
    [TextArea(3, 10)] public string textoRelato;
}

[CreateAssetMenu(fileName = "NuevaHistoria", menuName = "Historia/Cuento")]
public class HistoriaData : ScriptableObject
{
    public PasoHistoria[] pasos;
    public string nombreEscenaSiguiente = "level1";
}