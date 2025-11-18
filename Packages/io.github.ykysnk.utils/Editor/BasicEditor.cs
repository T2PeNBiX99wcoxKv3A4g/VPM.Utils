using System;
using UnityEditor;
using UnityEngine;

namespace io.github.ykysnk.utils.Editor;

public abstract class BasicEditor : UnityEditor.Editor
{
    protected virtual bool IsBaseOnOldInspectorGUI => false;

    protected virtual void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        if (IsBaseOnOldInspectorGUI)
            base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        try
        {
            OnInspectorGUIDraw();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorGUILayout.HelpBox($"Editor Error: {e.Message}\n{e.StackTrace}", MessageType.Error, true);
        }

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();
    }

    protected virtual void OnInspectorGUIDraw()
    {
    }
}