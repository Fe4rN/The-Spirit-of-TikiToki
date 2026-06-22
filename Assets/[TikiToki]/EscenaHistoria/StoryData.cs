using UnityEngine;

namespace TikiToki.UI
{
    [System.Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PasoHistoria")]
    public class StoryStep
    {
        [UnityEngine.Serialization.FormerlySerializedAs("imagenComic")]
        public Sprite comicImage;
        [UnityEngine.Serialization.FormerlySerializedAs("textoRelato")]
        [TextArea(3, 10)] public string narrativeText;
    }

    [CreateAssetMenu(fileName = "NewStory", menuName = "Story/Book")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "HistoriaData")]
    public class StoryData : ScriptableObject
    {
        [UnityEngine.Serialization.FormerlySerializedAs("pasos")]
        public StoryStep[] steps;
        [UnityEngine.Serialization.FormerlySerializedAs("nombreEscenaSiguiente")]
        public string nextSceneName = "level1";
    }
}
