using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.IO;
using CustomUI;

public class CompSearchEditor : EditorWindow
{
    private List<GameObject> _candidates = new List<GameObject>();
    private List<GameObject> _buttonCandidates = new List<GameObject>();
    private List<GameObject> _eventTriggerCandidates = new List<GameObject>();
    private GameObject _selectedCandidate;
    private ButtonSoundsEditorFilter _selectedFilter;

    private AudioSource _audioSource;
    private AudioClip _clickSound;
    private Vector2 _scrollPosition;

    #region Initialization

    [MenuItem("Tool/CompSearchEditor")]
    public static void OpenEditor()
    {
        CompSearchEditor window = GetWindow<CompSearchEditor>();
        window.titleContent = new GUIContent("Comp Search Editor");
        window.Initialize();
        window.Show();
    }

    private void Initialize()
    {
        RefreshCandidates();
    }

    #endregion

    private void RefreshCandidates()
    {
        _candidates.Clear();
        _buttonCandidates.Clear();
        _eventTriggerCandidates.Clear();    

        string path = Application.dataPath + "/Resources";
        string[] allFiles = Directory.GetFiles(path, "*.prefab", SearchOption.AllDirectories);
        StringBuilder sbb = new StringBuilder();

        for (int i = 0; i < allFiles.Length; i++)
        {
            string filename = allFiles[i];

            int idx = filename.IndexOf("Assets/");
            if (idx != -1)
            {
                filename = filename.Substring(idx, filename.Length - idx);
            }

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(filename, typeof(GameObject));
            if (obj is GameObject)
            {
                GameObject go = obj as GameObject;
                UGUIButton[] wrap = go.GetComponentsInChildren<UGUIButton>(true);
                for (int j = 0; j < wrap.Length; j++)
                {
                    //sbb.AppendLine(string.Format("{0} {1}", filename, wrap[j].name));
                    _candidates.Add(wrap[j].gameObject);
                    _buttonCandidates.Add(wrap[j].gameObject);
                }
                Button[] btn = go.GetComponentsInChildren<Button>(true);
                for(int k =0; k<btn.Length; k++)
                {
                    _candidates.Add(btn[k].gameObject);
                    _eventTriggerCandidates.Add(btn[k].gameObject);
                }
            }
        }        

        Func<GameObject, string> orderByFunc = _ => GetTransformPath(_.transform);
        _candidates = _candidates.OrderBy(orderByFunc).ToList();
        _buttonCandidates = _buttonCandidates.OrderBy(orderByFunc).ToList();
        _eventTriggerCandidates = _eventTriggerCandidates.OrderBy(orderByFunc).ToList();
    }

    private string GetTransformPath(Transform tr)
    {
        string path = tr.root.name;
        if (tr != tr.root)
            path += "/" + AnimationUtility.CalculateTransformPath(tr, tr.root);
        return path;
    }

    public void OnGUI()
    {
        RefreshCandidates();

        GUILayout.BeginVertical();        
        DrawMiddlePanel();
        GUILayout.EndVertical();
    } 

    #region Middle panel

    private void DrawMiddlePanel()
    {
        GUILayout.BeginVertical();

        if (IsNeedFilter())
        {
            DrawFilterPanel();
            GUILayout.Space(5f);
        }

        GUILayout.BeginHorizontal();
        DrawButtonsScrollView();
        DrawSelectedButtonInfoPanel();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private bool IsNeedFilter()
    {
        return _buttonCandidates.Any() && _eventTriggerCandidates.Any();
    }

    private void DrawFilterPanel()
    {
        GUILayout.BeginHorizontal();

        if (GUILayout.Toggle(_selectedFilter == ButtonSoundsEditorFilter.CustomButtons, "Custom Buttons", EditorStyles.toolbarButton, GUILayout.Width(100f)))
            _selectedFilter = ButtonSoundsEditorFilter.CustomButtons;

        if (GUILayout.Toggle(_selectedFilter == ButtonSoundsEditorFilter.OriginButtons, "Origin Buttons", EditorStyles.toolbarButton, GUILayout.Width(100f)))
            _selectedFilter = ButtonSoundsEditorFilter.OriginButtons;

        GUILayout.EndHorizontal();
    }

    private void DrawButtonsScrollView()
    {
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.MinWidth(215));
        foreach (GameObject candidate in GetFilteredCandidates())
        {
            MarkSelectedCandidate(candidate);
        }
        GUILayout.EndScrollView();
    }

    private List<GameObject> GetFilteredCandidates()
    {
        if (IsNeedFilter())
        {
            return _selectedFilter == ButtonSoundsEditorFilter.CustomButtons
                ? _buttonCandidates
                : _eventTriggerCandidates;
        }
        return _candidates;
    }

    private void MarkSelectedCandidate(GameObject candidate)
    {
        GUIStyle labelStyle = EditorStyles.label;
        Color originalColor = labelStyle.normal.textColor;
        if (candidate == _selectedCandidate)
            labelStyle.normal.textColor = new Color(0f, 0.5f, 0.5f);

        if (GUILayout.Button(candidate.name, labelStyle, GUILayout.Width(125)))
            SelectButton(candidate);

        labelStyle.normal.textColor = originalColor;
    }

    private void SelectButton(GameObject candidate)
    {
        Selection.activeObject = candidate;
        _selectedCandidate = candidate;
    }

    private void DrawSelectedButtonInfoPanel()
    {
        if (_selectedCandidate != null)
        {
            GUILayout.BeginVertical(GUILayout.Width(300));

            Image image = _selectedCandidate.GetComponent<Image>();
            if (image != null && image.sprite != null)
                GUILayout.Box(image.sprite.texture);

            Text textComponent = _selectedCandidate.GetComponentInChildren<Text>();
            if (textComponent != null)
                GUILayout.Label(string.Format("Text: '{0}'", textComponent.text));

            GUILayout.Label("Path:" + GetTransformPath(_selectedCandidate.transform), EditorStyles.wordWrappedLabel, GUILayout.Width(300));

            GUILayout.EndVertical();
        }
    }

    #endregion

    private void DrawTip(string message)
    {
        GUI.skin.label.normal.textColor = Color.red;
        GUILayout.Label(message);
        GUI.skin.label.normal.textColor = Color.black;
    }

    private enum ButtonSoundsEditorFilter
    {
        CustomButtons,
        OriginButtons
    }
}
