﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonsterDrop))]
public class MobDropEditor : Editor
{ // this script must be in "Editor" folder
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Container.Table((Container)target);
    }
}
