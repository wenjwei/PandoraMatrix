using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using com.tencent.pandora;

[CustomEditor(typeof(GameSimulator))]
public class GameSimulatorInspector : Editor
{
    private GameSimulator _gameSimulator;
    private int _selectedId = 0;
    private List<string> _userNameList = new List<string>();
    private string userInfoName;

    private List<string> _defaultPropertyNameList = new List<string>()
    {
        "QQAppid",
        "WXAppid",
        "TTPPAppid",
        "referenceResolution",
        "font"
    };

    private List<string> _userInfoItemNameList = new List<string>()
    {
        "name",
        "loginPlatform",
        "isProductEnvironment",
        "openId",
        "roleId",
        "partition",
        "accountType",
        "accessToken",
        "payToken",
        "gameName",
        "area",
        "userInfoExtend"
    };

    private void OnEnable()
    {
        _gameSimulator = (GameSimulator)target;
    }
    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        DrawDropdownMenu();
        DrawTips();
        DrawUserInfo("userInfo");
        DrawDefaultProperties();
        this.serializedObject.ApplyModifiedProperties();
    }
    private void DrawDropdownMenu()
    {
        _userNameList.Clear();
        //这里第一次执行时，userInfo还没初始化
        if (_gameSimulator.userInfo != null)
        {
            for (int i = 0, length = _gameSimulator.userInfo.Length; i < length; i++)
            {
                userInfoName = _gameSimulator.userInfo[i].name;
                if (string.IsNullOrEmpty(userInfoName))
                {
                    userInfoName = "Element " + i.ToString();
                }
                _userNameList.Add(userInfoName);
            }
        }

        if (_userNameList.Count != 0)
        {
            _selectedId = PlayerPrefs.GetInt("selectedId");
            _selectedId = EditorGUILayout.Popup("选择UserInfo", _selectedId, _userNameList.ToArray());
            PlayerPrefs.SetInt("selectedId", _selectedId);
        }
    }

    private void DrawArrayControlButton(SerializedProperty property)
    {
        EditorGUILayout.BeginHorizontal();
        GUIStyle buttonStylePlus = new GUIStyle("ShurikenPlus");
        GUIStyle buttonStyleMinus = new GUIStyle("ShurikenMinus");

        buttonStylePlus.overflow = new RectOffset(2, 2, 2, 2);
        buttonStyleMinus.overflow = new RectOffset(2, 2, 2, 2);
        GUILayout.Space(Screen.width - 80);
        if (GUILayout.Button("", buttonStylePlus))
        {
            //最大限制为10
            if (property.arraySize < 10)
            {
                property.arraySize++;
            }
            Repaint();
        }
        GUILayout.Space(15);
        if (GUILayout.Button("", buttonStyleMinus))
        {
            if (property.arraySize > 0)
            {
                property.arraySize--;
            }
            Repaint();
        }
        EditorGUILayout.EndHorizontal();
    }

    //只在没有UserInfo信息时展示
    private void DrawTips()
    {
        if (_userNameList.Count > 0)
        {
            return;
        }
        EditorGUILayout.HelpBox("第一次使用GameSimulator时，需要先添加User Info信息。\n 展开User Info，点击 \"+\" 进行添加， 点击 \"-\" 进行删除", MessageType.Info);
    }

    private void DrawUserInfo(string propertyName)
    {
        //自定义属性
        EditorGUILayout.BeginVertical(GUI.skin.box);
        var property = this.serializedObject.FindProperty(propertyName);
        EditorGUI.indentLevel++;
        if (EditorGUILayout.PropertyField(property))
        {
            EditorGUI.indentLevel++;
            for (int i = 0, size = property.arraySize; i < size; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                if (EditorGUILayout.PropertyField(element))
                {
                    EditorGUI.indentLevel++;
                    foreach (var item in _userInfoItemNameList)
                    {
                        var childElement = element.FindPropertyRelative(item);
                        if (childElement.displayName == "User Info Extend")
                        {
                            DrawUserInfoExtend(childElement);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(childElement, true);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            DrawArrayControlButton(property);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
        EditorGUILayout.Separator();
    }

    private void DrawUserInfoExtend(SerializedProperty property)
    {
        if (EditorGUILayout.PropertyField(property))
        {
            EditorGUI.indentLevel++;
            for (int i = 0, size = property.arraySize; i < size; i++)
            {
                var element = property.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, true);
            }
            DrawArrayControlButton(property);
            EditorGUILayout.Separator();
            EditorGUI.indentLevel--;
        }

    }

    //绘制默认的
    private void DrawDefaultProperties()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        foreach (var item in _defaultPropertyNameList)
        {
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty(item), true);
        }
        EditorGUILayout.EndVertical();

    }
}