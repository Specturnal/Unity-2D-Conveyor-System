using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ConveyorTheme", menuName = "Conveyor Theme")]
public class ConveyorTheme : ScriptableObject {

    [System.Serializable]
    internal struct Variant_Direction {
        [SerializeField] internal Sprite single;
        [SerializeField] internal Sprite first;
        [SerializeField] internal Sprite mid;
        [SerializeField] internal Sprite last;

        internal Sprite this[int index] {
            get {
                switch (index) {
                    case 0: return single;
                    case 1: return first;
                    case 2: return mid;
                    case 3: return last;
                    default: return null;
                }
            }
        }
    }
    [SerializeField] internal Variant_Direction up;
    [SerializeField] internal Variant_Direction down;
    [SerializeField] internal Variant_Direction right;
    [SerializeField] internal Variant_Direction left;

    internal Variant_Direction this[int index] {
        get {
            switch (index) {
                case 0: return up;
                case 1: return down;
                case 2: return right;
                case 3: return left;
                default: return default;
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ConveyorTheme))]
public class ConveyorThemeEditor : Editor {

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        string info = "Creating a ConveyorTheme asset is optional as it is meant to preview sprite changes while editing the scene. " +
            "When running in play mode, the actual sprite/animation of the conveyor segments rely entirely on the animation clips.";
        EditorGUILayout.HelpBox(info, MessageType.Info);
    }
}
#endif
