
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class AnimationHierarchyEditor : EditorWindow
{
    private static int columnWidth = 300;

    private int clipSize;
    private List<AnimationClip> clips;
    private ArrayList pathsKeys;
    private Hashtable paths;
    private GameObject selectedGo;

    private string replaceTextFrom;
    private string replaceTextTo;

    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Window/Animation/Hierarchy Editor")]
    static void ShowWindow()
    {
        EditorWindow.GetWindow<AnimationHierarchyEditor>();
    }

    public AnimationHierarchyEditor()
    {
        clips = new List<AnimationClip>();
    }

    void OnGUI()
    {
        if (Event.current.type == EventType.ValidateCommand)
        {
            switch (Event.current.commandName)
            {
                case "UndoRedoPerformed":
                    FillModel();
                    break;
            }
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Target Object:", GUILayout.Width(120));
        var newSelectedGo = (GameObject)EditorGUILayout.ObjectField(selectedGo, typeof(GameObject), true, GUILayout.Width(columnWidth));
        if (newSelectedGo != selectedGo)
        {
            selectedGo = newSelectedGo;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Clip Size:", GUILayout.Width(120));
        GUILayout.Label($"{clipSize}", GUILayout.Width(20));

        if (GUILayout.Button("+", GUILayout.Width(20)))
            clipSize++;

        if (GUILayout.Button("-", GUILayout.Width(20)))
            clipSize--;

        if (clipSize != clips.Count)
        {
            var newClips = new List<AnimationClip>();
            for (var i = 0; i < clipSize; i++)
                newClips.Add((i < clips.Count) ? clips[i] : null);

            clips = newClips;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Animation Clips:", GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();

        var addClipIndex = -1;
        var removeClipIndex = -1;
        for (var i = 0; i < clipSize; i++)
        {
            EditorGUILayout.BeginHorizontal();

            clips[i] = ((AnimationClip)EditorGUILayout.ObjectField(
                 clips[i],
                 typeof(AnimationClip),
                 true,
                 GUILayout.Width(columnWidth)));

            if (GUILayout.Button("+", GUILayout.Width(20)))
                addClipIndex = i;

            if (GUILayout.Button("-", GUILayout.Width(20)))
                removeClipIndex = i;

            EditorGUILayout.EndHorizontal();
        }

        if (addClipIndex >= 0)
        {
            clips.Insert(addClipIndex + 1, null);
            clipSize++;
        }

        if (removeClipIndex >= 0)
        {
            clips.RemoveAt(removeClipIndex);
            clipSize--;
        }

        GUILayout.Space(10);

        GUILayout.Label("Replace path", GUILayout.Width(120));
        EditorGUILayout.BeginHorizontal();
        replaceTextFrom = EditorGUILayout.TextField(replaceTextFrom, GUILayout.Width(columnWidth));
        GUILayout.Label("To", GUILayout.Width(20));
        replaceTextTo = EditorGUILayout.TextField(replaceTextTo, GUILayout.Width(columnWidth));

        if (GUILayout.Button("Replace", GUILayout.Width(120)))
        {
            if (string.IsNullOrEmpty(replaceTextFrom) == false)
            {
                foreach (string path in pathsKeys)
                {
                    if (path.Contains(replaceTextFrom) == false)
                        continue;

                    try
                    {
                        var newPath = path.Replace(replaceTextFrom, replaceTextTo);
                        UpdatePath(path, newPath);
                    }
                    catch (UnityException ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                }

                Refresh();
            }
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Reference path:", GUILayout.Width(columnWidth));
        GUILayout.Label("Object:", GUILayout.Width(columnWidth));
        GUILayout.Label("Count", GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();

        if (paths != null)
        {
            foreach (string path in pathsKeys)
            {
                GUICreatePathItem(path);
            }
        }

        GUILayout.Space(40);
        GUILayout.EndScrollView();
        FillModel();
    }

    void GUICreatePathItem(string path)
    {
        var tf = selectedGo.transform.Find(path);
        var obj = tf?.gameObject;

        GameObject newObj;
        ArrayList properties = (ArrayList)paths[path];

        EditorGUILayout.BeginHorizontal();

        var newPath = EditorGUILayout.TextField(path, GUILayout.Width(columnWidth));

        var standardColor = GUI.color;
        GUI.color = (obj != null) ? Color.green : Color.red;
        newObj = (GameObject)EditorGUILayout.ObjectField(obj, typeof(GameObject), true, GUILayout.Width(columnWidth));
        GUI.color = standardColor;

        var refCount = properties != null ? properties.Count : 0;
        EditorGUILayout.LabelField(refCount.ToString(), GUILayout.Width(60));

        EditorGUILayout.EndHorizontal();

        try
        {
            if (obj != newObj)
            {
                UpdatePath(path, ChildPath(newObj));
                Refresh();
            }

            if (newPath != path)
            {
                UpdatePath(path, newPath);
                Refresh();
            }
        }
        catch (UnityException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    void OnInspectorUpdate()
    {
        this.Repaint();
    }

    void FillModel()
    {
        paths = new Hashtable();
        pathsKeys = new ArrayList();

        foreach (AnimationClip animationClip in clips.Where(_ => _ != null))
        {
            FillModelWithCurves(AnimationUtility.GetCurveBindings(animationClip));
            FillModelWithCurves(AnimationUtility.GetObjectReferenceCurveBindings(animationClip));
        }
    }

    private void FillModelWithCurves(EditorCurveBinding[] curves)
    {
        foreach (EditorCurveBinding curveData in curves)
        {
            string key = curveData.path;

            if (paths.ContainsKey(key))
            {
                ((ArrayList)paths[key]).Add(curveData);
            }
            else
            {
                ArrayList newProperties = new ArrayList();
                newProperties.Add(curveData);
                paths.Add(key, newProperties);
                pathsKeys.Add(key);
            }
        }
    }

    void UpdatePath(string oldPath, string newPath)
    {
        AssetDatabase.StartAssetEditing();
        for (int iCurrentClip = 0; iCurrentClip < clips.Count; iCurrentClip++)
        {
            AnimationClip animationClip = clips[iCurrentClip];
            Undo.RecordObject(animationClip, "Animation Hierarchy Change");

            //recreating all curves one by one
            //to maintain proper order in the editor - 
            //slower than just removing old curve
            //and adding a corrected one, but it's more
            //user-friendly
            for (int iCurrentPath = 0; iCurrentPath < pathsKeys.Count; iCurrentPath++)
            {
                string path = pathsKeys[iCurrentPath] as string;
                ArrayList curves = (ArrayList)paths[path];

                for (int i = 0; i < curves.Count; i++)
                {
                    EditorCurveBinding binding = (EditorCurveBinding)curves[i];
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                    ObjectReferenceKeyframe[] objectReferenceCurve = AnimationUtility.GetObjectReferenceCurve(animationClip, binding);

                    try
                    {
                        if (curve != null)
                            AnimationUtility.SetEditorCurve(animationClip, binding, null);
                        else
                            AnimationUtility.SetObjectReferenceCurve(animationClip, binding, null);

                        if (path == oldPath)
                            binding.path = newPath;

                        if (curve != null)
                            AnimationUtility.SetEditorCurve(animationClip, binding, curve);
                        else
                            AnimationUtility.SetObjectReferenceCurve(animationClip, binding, objectReferenceCurve);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }

                    float fChunk = 1f / clips.Count;
                    float fProgress = (iCurrentClip * fChunk) + fChunk * ((float)iCurrentPath / (float)pathsKeys.Count);

                    EditorUtility.DisplayProgressBar(
                        "Animation Hierarchy Progress",
                        "How far along the animation editing has progressed.",
                        fProgress);
                }
            }
        }
        AssetDatabase.StopAssetEditing();
    }

    private void Refresh()
    {
        EditorUtility.ClearProgressBar();
        FillModel();
        this.Repaint();

    }

    string ChildPath(GameObject obj, bool sep = false)
    {
        return ChildPath(obj.transform.parent.gameObject, true) + obj.name + (sep ? "/" : "");
    }
}

#endif