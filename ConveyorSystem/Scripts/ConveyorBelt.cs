using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary> The parent of a single or multiple <see cref="ConveyorSegment"/>s. </summary>
public class ConveyorBelt : MonoBehaviour, IConveyorSystem {

    #region Inspector Variables
    /// <summary> The parent <see cref="ConveyorGroup"/>. </summary>
    [HideInInspector] public ConveyorGroup conveyorGroup;

    #region Activation
    [SerializeField] [HideInInspector] internal bool m_overwriteActivation = true;
    /// <summary> Should this <see cref="ConveyorBelt"/> control its children <see cref="ConveyorSegment"/>s' activation? </summary>
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
    [SerializeField] [HideInInspector] internal bool m_overwritePhysicValues = true;
    /// <summary> Should this <see cref="ConveyorBelt"/> control its children <see cref="ConveyorSegment"/>s' physics settings? </summary>
    public bool overwritePhysicValues {
        get => m_overwritePhysicValues;
        set {
            m_overwritePhysicValues = value;
            if (m_overwritePhysicValues) HandlePhysicChange();
        }
    }
    /// <summary> How the children <see cref="ConveyorSegment"/>(s) apply force on other objects. </summary>
    [HideInInspector] public ConveyorSegment.PhysicMode physicMode = ConveyorSegment.PhysicMode.Velocity;
    [SerializeField] [HideInInspector] internal float m_forceMultiplier = 5;
    [SerializeField] [HideInInspector] internal float m_maxSpeed = 1.5f;
    public float forceMultiplier => m_forceMultiplier;
    public float maxSpeed => m_maxSpeed;
    #endregion

    #region Orientation 
    [HideInInspector] public bool allowRotation = false;
    [HideInInspector] public Vector2 pivotOffset;
    #endregion
    #endregion

    [HideInInspector] public ConveyorSegment[] conveyorSegments;

    #region Public Functions
    /// <summary> Activates all <see cref="ConveyorSegment"/>(s) if <see cref="overwriteActivation"/> is true. </summary> 
    public void Activate(bool value) {
        if (conveyorGroup != null && conveyorGroup.overwriteActivation) {
            Debug.LogWarning("Activation overwritten by ConveyorGroup.");
            return;
        }

        OverwriteActivate(value);
    }

    public void SetPhysicValues(float forceMultiplier, float maxSpeed) {
        if (conveyorGroup != null && conveyorGroup.overwritePhysicValues) {
            Debug.LogWarning("Physic values overwritten by ConveyorGroup.");
            return;
        }

        OverwriteSetPhysic(forceMultiplier, maxSpeed);
    }

    /// <summary> Rotates the belt by 90° in the clockwise direction. </summary>
    /// <param name="count">Number of 90˚ rotations to make.</param>
    public void RotateClockwise(int count = 1) {
        if (!allowRotation) {
            Debug.LogWarning("There won't be any effect since allowOrientation is set to false!");
            return;
        }

        Vector3 pivot = transform.position + (Vector3)pivotOffset;

        foreach (ConveyorSegment conveyorSegment in conveyorSegments) {
            conveyorSegment.RotateClockwise(count);

            for (int i = 0; i < count; i++) {
                Vector3 direction = conveyorSegment.transform.position - pivot;
                Vector3 rotatedDirection = Quaternion.Euler(0, 0, -90) * direction;
                conveyorSegment.transform.position = rotatedDirection + pivot;
            }
        }
    }

    /// <summary> Does a 180˚ rotation (shorthand for <see cref="RotateClockwise(int)"/> with 2 as the input. </summary>
    public void FlipBelt() => RotateClockwise(2);
    #endregion

    #region Overwrite Access
    internal void OverwriteActivate(bool value) {
        m_activated = value;
        HandleActivationChange();
    }
    internal void OverwriteSetPhysic(float forceMultiplier, float maxSpeed) {
        m_forceMultiplier = forceMultiplier;
        m_maxSpeed = maxSpeed;
        HandlePhysicChange();
    }
    #endregion

    #region Handle Change
    internal void HandleHierarchyChange() {
        if (conveyorGroup != null && conveyorGroup.transform != transform.parent) {
            conveyorGroup.HandleHierarchyChange();
            conveyorGroup = null;
        } else if (conveyorGroup?.transform != transform.parent && transform.parent.TryGetComponent(out conveyorGroup)) conveyorGroup.HandleHierarchyChange();

        conveyorSegments = transform.GetComponentsInChildren<ConveyorSegment>();
    }

    internal void HandleActivationChange() {
        if (!m_overwriteActivation) return;

        foreach (ConveyorSegment conveyorSegment in conveyorSegments)
            conveyorSegment.OverwriteActivate(activated);
    }

    internal void HandlePhysicChange() {
        if (!m_overwritePhysicValues) return;

        foreach (ConveyorSegment conveyorSegment in conveyorSegments) {
            conveyorSegment.physicMode = physicMode;
            conveyorSegment.OverwriteSetPhysic(m_forceMultiplier, m_maxSpeed);
        }
    }
    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(ConveyorBelt))]
[CanEditMultipleObjects]
public class ConveyorBeltEditor : Editor {

    private ConveyorBelt conveyorBelt;

    SerializedProperty conveyorGroup;
    SerializedProperty overwriteActivation, activated;
    SerializedProperty overwritePhysicValues, physicMode, forceMultiplier, maxSpeed;
    SerializedProperty allowRotation, pivotOffset;

    private void OnEnable() {
        conveyorBelt = (ConveyorBelt)target;

        conveyorGroup = serializedObject.FindProperty("conveyorGroup");

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

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(conveyorGroup);
        EditorGUI.EndDisabledGroup();

        if (!CompareMultiple(belt => belt.conveyorGroup == conveyorBelt.conveyorGroup)) {
            EditorGUILayout.HelpBox("Multi-object editing not supported across ConveyorBelts with different ConveyorGroups", MessageType.Info);
            goto SceneEditingTools;
        }

        #region Activation
        EditorGUILayout.Space();
        if (conveyorBelt.conveyorGroup != null && conveyorBelt.conveyorGroup.overwriteActivation)
            EditorGUILayout.HelpBox("Activation overwritten by ConveyorGroup.", MessageType.Info);
        else {
            EditorGUILayout.PropertyField(overwriteActivation);

            if (CompareMultiple(belt => belt.overwriteActivation == conveyorBelt.overwriteActivation)) {
                EditorGUI.BeginDisabledGroup(!conveyorBelt.overwriteActivation);
                EditorGUILayout.PropertyField(activated);
                EditorGUI.EndDisabledGroup();
            } else
                EditorGUILayout.HelpBox("Multi-object editing of activation not supported across ConveyorBelts with different overwriteActivation permission", MessageType.Info);
        }
        #endregion

        #region Physics
        EditorGUILayout.Space();
        if (conveyorBelt.conveyorGroup != null && conveyorBelt.conveyorGroup.overwritePhysicValues)
            EditorGUILayout.HelpBox("Physic values overwritten by ConveyorGroup.", MessageType.Info);
        else {
            EditorGUILayout.PropertyField(overwritePhysicValues);

            if (CompareMultiple(belt => belt.overwritePhysicValues == conveyorBelt.overwritePhysicValues)) {
                EditorGUI.BeginDisabledGroup(!conveyorBelt.overwritePhysicValues);
                EditorGUILayout.PropertyField(physicMode);
                EditorGUILayout.PropertyField(forceMultiplier);
                EditorGUILayout.PropertyField(maxSpeed);
                EditorGUI.EndDisabledGroup();
            } else
                EditorGUILayout.HelpBox("Multi-object editing of physic values not supported across ConveyorBelts with different overwritePhysicValues permission", MessageType.Info);
        }
        #endregion

        #region Orientation
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(allowRotation);

        if (CompareMultiple(belt => belt.allowRotation == conveyorBelt.allowRotation)) {
            EditorGUI.BeginDisabledGroup(!conveyorBelt.allowRotation);
            EditorGUILayout.PropertyField(pivotOffset);
            EditorGUI.EndDisabledGroup();
        } else
            EditorGUILayout.HelpBox("Multi-object editing of activation not supported across ConveyorBelts with different allowRotation permission", MessageType.Info);
        #endregion

        #region Scene Editing Tools
        SceneEditingTools:
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene Editing Tools", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate +90°")) RunOnMultiple(belt => belt.RotateClockwise(3));
        if (GUILayout.Button("Rotate -90°")) RunOnMultiple(belt => belt.RotateClockwise());
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Flip")) RunOnMultiple(belt => belt.FlipBelt());
        #endregion

        if (GUI.changed || conveyorBelt.transform.hasChanged) {
            conveyorBelt.transform.hasChanged = false;
            serializedObject.ApplyModifiedProperties();
            RunOnMultiple(belt => {
                belt.HandleHierarchyChange();
                belt.HandleActivationChange();
                belt.HandlePhysicChange();
                foreach (ConveyorSegment segment in belt.conveyorSegments)
                    EditorUtility.SetDirty(segment);
            });
        }
    }

    private bool CompareMultiple(System.Func<ConveyorBelt, bool> func) {
        foreach (ConveyorBelt conveyorBelt in targets) if (!func(conveyorBelt)) return false;
        return true;
    }
    private void RunOnMultiple(System.Action<ConveyorBelt> action) {
        foreach (ConveyorBelt conveyorBelt in targets) action(conveyorBelt);
    }
}
#endif