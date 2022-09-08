using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CustomEditor(typeof(PortalPairBuilder))]
public class PortalPairEditor : Editor
{
    private readonly char checkChar = '\u2713';
    private readonly char warningChar = '\u26A0';
    private readonly char errorChar = '\u2716';
    private readonly Color checkColor = Color.green;
    private readonly Color warningColor = Color.yellow;
    private readonly Color errorColor = Color.red;

    private bool connectedLocked = false;

    SerializedProperty portalMesh;
    SerializedProperty stretch;
    SerializedProperty leftHolder;
    SerializedProperty rightHolder;
    SerializedProperty leftFrame;
    SerializedProperty rightFrame;


    private void OnEnable()
    {
        portalMesh = serializedObject.FindProperty("portalMesh");
        stretch = serializedObject.FindProperty("stretch");
        leftHolder = serializedObject.FindProperty("leftHolder");
        rightHolder = serializedObject.FindProperty("rightHolder");
        leftFrame = serializedObject.FindProperty("leftFrame");
        rightFrame = serializedObject.FindProperty("rightFrame");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        PortalFrame left = (PortalFrame)leftFrame.objectReferenceValue;
        PortalFrame right = (PortalFrame)rightFrame.objectReferenceValue;
        if(left != null)
            if(left.Initialized)
                connectedLocked = true;
        if(right != null)
            if(right.Initialized)
                connectedLocked = true;

        #region Connected Properties
        EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(connectedLocked);
                GUILayout.Label("Connected Properties " + (connectedLocked ? "(locked)" : ""), EditorStyles.boldLabel);
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!connectedLocked);
                if(GUILayout.Button(new GUIContent("Unlock", "Portals will be de-initialized")))
                {
                    if (left != null) left.DeInitialize();
                    if (right != null) right.DeInitialize();
                    connectedLocked = false;
                }
            EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUI.BeginDisabledGroup(connectedLocked);
            EditorGUILayout.PropertyField(portalMesh);
            EditorGUILayout.PropertyField(stretch);
        EditorGUI.EndDisabledGroup();

        GUILayout.Space(15);
        #endregion
        #region Portals
        GUILayout.Label("Portals", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Left Portal:");
            GUILayout.Label("Right Portal:");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
            // also check if a new one has been added and it has already been initialized
            EditorGUI.BeginDisabledGroup(left != null && left.Initialized);
                EditorGUILayout.PropertyField(leftFrame, GUIContent.none);
                PortalFrame newLeft = (PortalFrame)leftFrame.objectReferenceValue;
                if (left != newLeft)
                    if (newLeft != null && newLeft.Initialized)
                        newLeft.DeInitialize();
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(right != null && right.Initialized);
                EditorGUILayout.PropertyField(rightFrame, GUIContent.none);
                PortalFrame newRight = (PortalFrame)rightFrame.objectReferenceValue;
                if (right != newRight)
                    if (newRight != null && newRight.Initialized)
                        newRight.DeInitialize();
            EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
            DisplayInitializeButton((PortalFrame)leftFrame.objectReferenceValue, "Left");
            DisplayInitializeButton((PortalFrame)rightFrame.objectReferenceValue, "Right");
        EditorGUILayout.EndHorizontal();
        bool allInitialized =
                (leftFrame.objectReferenceValue != null ? ((PortalFrame)leftFrame.objectReferenceValue).Initialized : false) &&
                (rightFrame.objectReferenceValue != null ? ((PortalFrame)rightFrame.objectReferenceValue).Initialized : false);
        EditorGUI.BeginDisabledGroup(!allInitialized);
            if(GUILayout.Button(new GUIContent(
                "Complete Portal Pair", 
                allInitialized ? 
                "This will merge portals into their holders and destroy this script!" :
                "Can't run this operation without both portals being Initialized!"
                )
                )
            )
            {
                PortalHolder lHolder = new GameObject("Left Holder").AddComponent<PortalHolder>();
                lHolder.transform.parent = ((PortalPairBuilder)target).transform;
                lHolder.Initialize(((PortalFrame)leftFrame.objectReferenceValue), ((PortalPairBuilder)target).transform);
                leftHolder.objectReferenceValue = lHolder;
                PortalHolder rHolder = new GameObject("Right Holder").AddComponent<PortalHolder>();
                rHolder.transform.parent = ((PortalPairBuilder)target).transform;
                rHolder.Initialize(((PortalFrame)rightFrame.objectReferenceValue), ((PortalPairBuilder)target).transform);    
                rightHolder.objectReferenceValue = rHolder;

                PortalPair portalPair = ((PortalPairBuilder)target).gameObject.AddComponent<PortalPair>();
                portalPair.Initialize(lHolder, rHolder);
                serializedObject.ApplyModifiedProperties();
                DestroyImmediate((PortalPairBuilder)target);
                return;
        }
        EditorGUI.EndDisabledGroup();

        #endregion
        serializedObject.ApplyModifiedProperties();
    }

    public void DisplayInitializeButton(PortalFrame portalFrame, string direction)
    {
        bool disabled = (portalMesh.objectReferenceValue == null) || (portalFrame == null);
        EditorGUI.BeginDisabledGroup(disabled);


        string tooltip = "";
        string badgeText = "";
        Color badgeColor = Color.white;
        if (disabled)
        {
            badgeText = errorChar.ToString();
            badgeColor = errorColor;
            tooltip = "Can't initialize because Mesh and/or Portal is missing!";
        }
        else if (!portalFrame.Initialized)
        {
            badgeText = warningChar.ToString();
            badgeColor = warningColor;
            tooltip = "Portal has not been initialized yet!";
        }
        else
        {
            badgeText = checkChar.ToString();
            badgeColor = checkColor;
            tooltip = "Portal has been initialized. You can re-initialize if needed.";
        }

        // create a button to automatically create a portal frame in the scene for us?
        GUI.contentColor = badgeColor;
        GUILayout.Label(new GUIContent(badgeText, tooltip));
        GUI.contentColor = Color.white;


        if (GUILayout.Button("Initialize " + direction + " Portal"))
        {
            portalFrame.Initialize(
                (Mesh)portalMesh.objectReferenceValue,
                stretch.vector3Value
                );
        }
        GUILayout.FlexibleSpace();

        EditorGUI.EndDisabledGroup();
    }
}
