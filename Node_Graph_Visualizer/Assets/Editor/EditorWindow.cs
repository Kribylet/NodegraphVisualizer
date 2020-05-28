using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextAreaExample : EditorWindow
{
    string text = "Nothing Opened...";
    TextAsset txtAsset;
    Vector2 scroll;

    [MenuItem("Window/DebugWindow")]
    static void Init()
    {
        TextAreaExample window = (TextAreaExample)GetWindow(typeof(TextAreaExample));
        window.Show();
    }

    Object source;

    void OnGUI()
    {
        source = EditorGUILayout.ObjectField(source, typeof(Object), true);
        TextAsset newTxtAsset = (TextAsset)source;

        if (newTxtAsset != txtAsset)
            ReadTextAsset(newTxtAsset);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        text = EditorGUILayout.TextArea(text, GUILayout.Height(position.height - 30));
        EditorGUILayout.EndScrollView();
    }

    void ReadTextAsset(TextAsset txt)
    {
        text = txt.text;
        txtAsset = txt;
    }
}
