using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ConveyorSegment : MonoBehaviour, IConveyorSystem {
    /// <summary> How force is applied on other objects. </summary>
    public enum PhysicMode { Force, Impulse, Velocity, Transform }
    /// <summary> The role of this <see cref="ConveyorSegment"/> in its parent <see cref="ConveyorBelt"/> (if any). <br/>
    /// Determines the sprite and animation of the conveyor segment. </summary>
    public enum Alignment { Single, First, Mid, Last }

    #region Inspector Variables
    /// <summary> The parent <see cref="ConveyorBelt"/>. </summary>
    [HideInInspector] public ConveyorBelt conveyorBelt;

    #region Activation
    [SerializeField] [HideInInspector] private bool m_activated = true;
    /// <summary> The activation state of this <see cref="ConveyorSegment"/>. </summary>
    public bool activated { get => m_activated; }
	#endregion

	#region Physics
	/// <summary> How this <see cref="ConveyorSegment"/> applies force on other objects. </summary>
	[HideInInspector] public PhysicMode physicMode = PhysicMode.Velocity;
    [SerializeField] [HideInInspector] private float m_forceMultiplier = 5;
    [SerializeField] [HideInInspector] private float m_maxSpeed = 1.5f;
    /// <summary> Amount of force to apply to other objects. </summary>
    public float forceMultiplier => m_forceMultiplier;
    /// <summary> Maximum speed of objects affected by this <see cref="ConveyorSegment"/>. </summary>
    public float maxSpeed => m_maxSpeed;
    /// <summary> Scale of the animation speed to synchronise with the affected objects' movement. <br/>
    /// It is recommended to set the value to how many units the belt travels in 1 second under normal animation speed. </summary>
    [HideInInspector] public float animationSpeedScaling = 1.5f;
    #endregion

    #region Orientation
    /// <summary> Determines the variant of conveyor belt (edge position) based on its position from the source of direction (imaginary) </summary>
    [HideInInspector] public Alignment alignment = Alignment.Single;
    [SerializeField] [HideInInspector] private bool m_isHorizontal = false, m_flipDirection = false;
    public bool isHorizontal { get => m_isHorizontal; }
    public bool flipDirection { get => m_flipDirection; }
    #endregion
    #endregion

    private Animator anim;
    private int directionIndex => (m_isHorizontal ? 2 : 0) + (m_flipDirection ? 1 : 0);
    private float animSpeed => maxSpeed / animationSpeedScaling;

#if UNITY_EDITOR
    [SerializeField] [HideInInspector] private ConveyorTheme conveyorTheme;
#endif

    private void Awake() {
        anim = GetComponent<Animator>();
        HandleHierarchyChange();
        HandleActivationChange();
        HandlePhysicChange();
        HandleOrientationChange();
    }

    #region Public Functions
    /// <summary> Resumes or halts the animation and effect on other objects. </summary> 
    public void Activate(bool value) {
        if (conveyorBelt != null && conveyorBelt.overwriteActivation) {
            Debug.LogWarning("Activation overwritten by ConveyorBelt.");
            return;
        }

        OverwriteActivate(value);
    }
    public void SetPhysicValues(float forceMultiplier, float maxSpeed) {
        if (conveyorBelt != null && conveyorBelt.overwritePhysicValues) {
            Debug.LogWarning("Physic values overwritten by ConveyorBelt.");
            return;
        }

        OverwriteSetPhysic(forceMultiplier, maxSpeed);
    }
    /// <summary> Rotates the segment by 90° in the clockwise direction (Changes to another <see cref="Sprite"/> and plays a different <see cref="Animation"/>). </summary>
    /// <param name="count">Number of 90˚ rotations to make.</param>
    public void RotateClockwise(int count = 1) {
        count = Mathf.Clamp(count, 0, 4);
        for (int i = 0; i < count; i++) {
            if (m_isHorizontal) m_flipDirection = !m_flipDirection;
            m_isHorizontal = !m_isHorizontal;
        }
        HandleOrientationChange();
    }
    /// <summary> Does a 180˚ rotation (shorthand for <see cref="RotateClockwise(int)"/> with 2 as the input. </summary>
    public void FlipSegment() => RotateClockwise(2);
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
        if (conveyorBelt != null && conveyorBelt.transform != transform.parent) {
            conveyorBelt.HandleHierarchyChange();
            conveyorBelt = null;
        } else if (conveyorBelt?.transform != transform.parent && transform.parent.TryGetComponent(out conveyorBelt)) conveyorBelt.HandleHierarchyChange();
    }
    internal void HandleActivationChange() {
        if (anim != null)
            anim.speed = activated ? animSpeed : 0;
    }
    /// <summary> Changes animation speed </summary>
    internal void HandlePhysicChange() {
        HandleActivationChange();
    }
    /// <summary> Changes sprite or animation </summary>
    internal void HandleOrientationChange() {
        if (anim == null)
            GetComponent<SpriteRenderer>().sprite = conveyorTheme[directionIndex][(int)alignment];
        else {
            anim.SetFloat("Direction", directionIndex);
            anim.SetFloat("Alignment", (int)alignment);
        }
    }
    #endregion

    private Vector2 CalculateDirection(Vector3 directionVector) {
        float xForce = m_isHorizontal ? m_forceMultiplier * (m_flipDirection ? -1 : 1) : directionVector.x * m_forceMultiplier;
        float yForce = !m_isHorizontal ? m_forceMultiplier * (m_flipDirection ? -1 : 1) : directionVector.y * m_forceMultiplier;
        return new Vector2(xForce, yForce);
    }

    private void OnTriggerStay2D(Collider2D collision) {
        if (!activated) return;

        Vector2 direction = CalculateDirection(transform.position - collision.transform.position);
        switch (physicMode) {
            case PhysicMode.Force:
                collision.attachedRigidbody.AddForce(direction, ForceMode2D.Force);
                break;
            case PhysicMode.Impulse:
                collision.attachedRigidbody.AddForce(direction, ForceMode2D.Impulse);
                break;
            case PhysicMode.Velocity:
                collision.attachedRigidbody.velocity = direction;
                break;
            case PhysicMode.Transform:
                collision.transform.position += (Vector3)direction * Time.deltaTime;
                break;
        }
        collision.attachedRigidbody.velocity = Vector2.ClampMagnitude(collision.attachedRigidbody.velocity, m_maxSpeed);
    }
    private void OnTriggerExit2D(Collider2D collision) =>
        collision.attachedRigidbody.velocity = activated && (alignment == Alignment.Last || alignment == Alignment.Single) ? Vector2.zero : collision.attachedRigidbody.velocity;
}
internal interface IConveyorSystem {
    void Activate(bool value);
    void SetPhysicValues(float forceMultiplier, float maxSpeed);
    void RotateClockwise(int count = 1);
}

#if UNITY_EDITOR
[CustomEditor(typeof(ConveyorSegment))]
[CanEditMultipleObjects]
public class ConveyorSegmentEditor : Editor {

    private ConveyorSegment conveyorSegment;

    SerializedProperty conveyorBelt;
    SerializedProperty activated;
    SerializedProperty physicMode, forceMultiplier, maxSpeed, animationSpeedScaling;
    SerializedProperty alignment, isHorizontal, flipDirection;
    SerializedProperty conveyorTheme;

    private void OnEnable() {
        conveyorSegment = (ConveyorSegment)target;

        conveyorBelt = serializedObject.FindProperty("conveyorBelt");
        activated = serializedObject.FindProperty("m_activated");

        physicMode = serializedObject.FindProperty("physicMode");
        forceMultiplier = serializedObject.FindProperty("m_forceMultiplier");
        maxSpeed = serializedObject.FindProperty("m_maxSpeed");
        animationSpeedScaling = serializedObject.FindProperty("animationSpeedScaling");

        alignment = serializedObject.FindProperty("alignment");
        isHorizontal = serializedObject.FindProperty("m_isHorizontal");
        flipDirection = serializedObject.FindProperty("m_flipDirection");

        conveyorTheme = serializedObject.FindProperty("conveyorTheme");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(conveyorBelt);
        EditorGUI.EndDisabledGroup();

        if (!CompareMultiple(segment => segment.conveyorBelt == conveyorSegment.conveyorBelt)) {
            EditorGUILayout.HelpBox("Multi-object editing not supported across ConveyorSegments with different ConveyorBelts", MessageType.Info);
            goto SceneEditingTools;
        }

        #region Activation
        EditorGUILayout.Space();
        if (conveyorSegment.conveyorBelt != null && conveyorSegment.conveyorBelt.overwriteActivation)
            EditorGUILayout.HelpBox("Activation overwritten by ConveyorBelt.", MessageType.Info);
        else
            EditorGUILayout.PropertyField(activated);
        #endregion

        #region Physics
        EditorGUILayout.Space();
        if (conveyorSegment.conveyorBelt != null && conveyorSegment.conveyorBelt.overwritePhysicValues)
            EditorGUILayout.HelpBox("Physics overwritten by ConveyorBelt.", MessageType.Info);
        else {
            EditorGUILayout.PropertyField(physicMode);
            EditorGUILayout.PropertyField(forceMultiplier);
            EditorGUILayout.PropertyField(maxSpeed);
        }
        EditorGUILayout.PropertyField(animationSpeedScaling);
        #endregion

        #region Orientation
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(alignment);
        EditorGUILayout.PropertyField(isHorizontal);
        EditorGUILayout.PropertyField(flipDirection);
    #endregion

    #region Scene Editing Tools
    SceneEditingTools:
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene Editing Tools", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(conveyorTheme);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate +90°")) RunOnMultiple(segment => segment.RotateClockwise(3));
        if (GUILayout.Button("Rotate -90°")) RunOnMultiple(segment => segment.RotateClockwise());
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Flip")) RunOnMultiple(segment => segment.FlipSegment());
        #endregion

        if (GUI.changed || conveyorSegment.transform.hasChanged) {
            conveyorSegment.transform.hasChanged = false;
            serializedObject.ApplyModifiedProperties();
            RunOnMultiple(segment => {
                segment.HandleHierarchyChange();
                segment.HandleActivationChange();
                segment.HandlePhysicChange();
                segment.HandleOrientationChange();
            });
        }
    }

    private bool CompareMultiple(System.Func<ConveyorSegment, bool> func) {
        foreach (ConveyorSegment conveyorSegment in targets) if (!func(conveyorSegment)) return false;
        return true;
    }
    private void RunOnMultiple(System.Action<ConveyorSegment> action) {
        foreach (ConveyorSegment conveyorSegment in targets) action(conveyorSegment);
    }
}
#endif