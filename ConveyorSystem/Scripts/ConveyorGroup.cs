using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary> The parent of a single or multiple <see cref="ConveyorBelt"/>s. </summary>
public class ConveyorGroup : MonoBehaviour, IConveyorSystem {

    #region Inspector Variables 
    #region Activation
    [SerializeField] [HideInInspector] internal bool m_overwriteActivation = true;
    /// <summary> Should this <see cref="ConveyorGroup"/> control its children <see cref="ConveyorBelt"/>s' activation? </summary> 
    public bool overwriteActivation {
        get => m_overwriteActivation;
        set {
            m_overwriteActivation = value;
            if (m_overwriteActivation) Activate(m_activated);
        }
    }
    [SerializeField] [HideInInspector] private bool m_activated = true;
    /// <summary> The activation state of this <see cref="ConveyorBelt"/>. </summary>
    public bool activated { get => m_activated; }
    #endregion

    #region Physics
    [SerializeField] [HideInInspector] private bool m_overwritePhysicValues = true;
    /// <summary> Should this <see cref="ConveyorGroup"/> control its children <see cref="ConveyorBelt"/>s' physics settings? </summary>
    public bool overwritePhysicValues {
        get => m_overwritePhysicValues;
        set {
            m_overwritePhysicValues = value;
            if (m_overwritePhysicValues) HandlePhysicChange();
        }
    }

    /// <summary> How the children <see cref="ConveyorBelt"/>(s) apply force on other objects. </summary>
    [HideInInspector] public ConveyorSegment.PhysicMode physicMode = ConveyorSegment.PhysicMode.Velocity;
    [SerializeField] [HideInInspector] private float m_forceMultiplier = 5, m_maxSpeed = 1.5f;
    public float forceMultiplier { get => m_forceMultiplier; }
    public float maxSpeed { get => m_maxSpeed; }
    #endregion

    [HideInInspector] public bool allowRotation = false;
    [HideInInspector] public Vector2 pivotOffset;
    #endregion

    [HideInInspector] public ConveyorBelt[] conveyorBelts;

    #region Public Functions
    /// <summary> Activates all <see cref="ConveyorBelt"/>(s) if <see cref="overwriteActivation"/> is true. </summary> 
    public void Activate(bool value) {
        m_activated = value;
        HandleActivationChange();
    }
    public void SetPhysicValues(float forceMultiplier, float maxSpeed) {
        m_forceMultiplier = forceMultiplier;
        m_maxSpeed = maxSpeed;
        HandlePhysicChange();
    }

    /// <summary> Rotates the belt by 90° in the clockwise direction. </summary>
    /// <param name="count">Number of 90˚ rotations to make.</param>
    public void RotateClockwise(int count = 1) {
        if (!allowRotation) {
            Debug.LogWarning("There won't be any effect since allowOrientation is set to false!");
            return;
        }

        Vector3 pivot = transform.position + (Vector3)pivotOffset;

        foreach (ConveyorBelt conveyorBelt in conveyorBelts) {
            conveyorBelt.RotateClockwise(count);

            for (int i = 0; i < count; i++) {
                Vector3 direction = conveyorBelt.transform.position - pivot;
                Vector3 rotatedDirection = Quaternion.Euler(0, 0, -90) * direction;
                conveyorBelt.transform.position = rotatedDirection + pivot;
            }
        }
    }

    /// <summary> Does a 180˚ rotation (shorthand for <see cref="RotateClockwise(int)"/> with 2 as the input. </summary>
    public void FlipBelt() => RotateClockwise(2);
    #endregion

    #region Handle Change
    internal void HandleHierarchyChange() {
        conveyorBelts = transform.GetComponentsInChildren<ConveyorBelt>();
    }

    internal void HandleActivationChange() {
        if (!m_overwriteActivation) return;

        foreach (ConveyorBelt conveyorBelt in conveyorBelts) {
            conveyorBelt.m_overwriteActivation = true;
            conveyorBelt.OverwriteActivate(activated);
        }
    }

    internal void HandlePhysicChange() {
        if (!m_overwritePhysicValues) return;

        foreach (ConveyorBelt conveyorBelt in conveyorBelts) {
            conveyorBelt.m_overwritePhysicValues = true;
            conveyorBelt.physicMode = physicMode;
            conveyorBelt.OverwriteSetPhysic(m_forceMultiplier, m_maxSpeed);
        }
    }
    #endregion
}


#if UNITY_EDITOR
[CustomEditor(typeof(ConveyorGroup))]
[CanEditMultipleObjects]
public class ConveyorGroupEditor : Editor {

    private ConveyorGroup conveyorGroup;

    SerializedProperty conveyorBelts;
    SerializedProperty overwriteActivation, activated;
    SerializedProperty overwritePhysicValues, physicMode, forceMultiplier, maxSpeed;
    SerializedProperty allowRotation, pivotOffset;

    private void OnEnable() {
        conveyorGroup = (ConveyorGroup)target;

        conveyorBelts = serializedObject.FindProperty("conveyorBelts");

        overwriteActivation = serializedObject.FindProperty("m_overwriteActivation");
        activated = serializedObject.FindProperty("m_activated");

        overwritePhysicValues = serializedObject.FindProperty("m_overwritePhysicValues");
        physicMode = serializedObject.FindProperty("physicMode");
        forceMultiplier = serializedObject.FindProperty("m_forceMultiplier");
        maxSpeed = serializedObject.FindProperty("m_maxSpeed");

        allowRotation = serializedObject.FindProperty("allowRotation");
        pivotOffset = serializedObject.FindProperty("pivotOffset");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(conveyorBelts);
        EditorGUI.EndDisabledGroup();

        #region Activation
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(overwriteActivation);

        if (CompareMultiple(group => group.overwriteActivation == conveyorGroup.overwriteActivation)) {
            EditorGUI.BeginDisabledGroup(!conveyorGroup.overwriteActivation);
            EditorGUILayout.PropertyField(activated);
            EditorGUI.EndDisabledGroup();
        } else
            EditorGUILayout.HelpBox("Multi-object editing of activation not supported across ConveyorGroups with different overwriteActivation permission", MessageType.Info);

        #endregion

        #region Physics
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(overwritePhysicValues);

        if (CompareMultiple(group => group.overwritePhysicValues == conveyorGroup.overwritePhysicValues)) {
            EditorGUI.BeginDisabledGroup(!conveyorGroup.overwritePhysicValues);
            EditorGUILayout.PropertyField(physicMode);
            EditorGUILayout.PropertyField(forceMultiplier);
            EditorGUILayout.PropertyField(maxSpeed);
            EditorGUI.EndDisabledGroup();
        } else
            EditorGUILayout.HelpBox("Multi-object editing of physic values not supported across ConveyorGroups with different overwritePhysicValues permission", MessageType.Info);
        #endregion

        #region Orientation
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(allowRotation);

        if (CompareMultiple(group => group.allowRotation == conveyorGroup.allowRotation)) {
            EditorGUI.BeginDisabledGroup(!conveyorGroup.allowRotation);
            EditorGUILayout.PropertyField(pivotOffset);
            EditorGUI.EndDisabledGroup();
        } else
            EditorGUILayout.HelpBox("Multi-object editing of activation not supported across ConveyorGroups with different allowRotation permission", MessageType.Info);
        #endregion

        #region Scene Editing Tools
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene Editing Tools", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate +90°")) RunOnMultiple(group => group.RotateClockwise(3));
        if (GUILayout.Button("Rotate -90°")) RunOnMultiple(group => group.RotateClockwise());
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Flip")) RunOnMultiple(group => group.FlipBelt());
        #endregion

        if (GUI.changed || conveyorGroup.transform.hasChanged) {
            conveyorGroup.transform.hasChanged = false;
            serializedObject.ApplyModifiedProperties();
            RunOnMultiple(group => {
                group.HandleHierarchyChange();
                group.HandleActivationChange();
                group.HandlePhysicChange();
                foreach (ConveyorBelt belt in group.conveyorBelts) {
                    EditorUtility.SetDirty(belt);
                    foreach (ConveyorSegment segment in belt.conveyorSegments)
                        EditorUtility.SetDirty(segment);
                }
            });
        }
    }

    private bool CompareMultiple(System.Func<ConveyorGroup, bool> func) {
        foreach (ConveyorGroup conveyorGroup in targets) if (!func(conveyorGroup)) return false;
        return true;
    }
    private void RunOnMultiple(System.Action<ConveyorGroup> action) {
        foreach (ConveyorGroup conveyorGroup in targets) action(conveyorGroup);
    }
}
#endif
