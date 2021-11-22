﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldObject))]
public class CustomWorldObjectInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Set Position"))
        {
            ((WorldObject)target).SetPosition();
        }
    }
}
