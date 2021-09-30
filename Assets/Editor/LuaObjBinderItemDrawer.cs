using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using UnityEngine.UI;

[CustomPropertyDrawer(typeof(LuaObjBinder.LuaObj))]
public class LuaObjBinderItemDrawer : PropertyDrawer
{
    public static readonly List<KeyValuePair<Type, string>> LuaObjBindTypes = new List<KeyValuePair<Type, string>>()
    {
        new KeyValuePair<Type, string>(typeof(GameObject), "Go"),
        new KeyValuePair<Type, string>(typeof(Transform), "Trans"),
        new KeyValuePair<Type, string>(typeof(RectTransform), "RTrans"),
        new KeyValuePair<Type, string>(typeof(Canvas), "Cvs"),
        new KeyValuePair<Type, string>(typeof(LuaObjBinder), "LBinder"),
        new KeyValuePair<Type, string>(typeof(CanvasGroup), "CvsGp"),

        new KeyValuePair<Type, string>(typeof(Mask), "Msk"),
        new KeyValuePair<Type, string>(typeof(RectMask2D), "RMsk"),

        new KeyValuePair<Type, string>(typeof(LayoutElement), "LyEle"),
        new KeyValuePair<Type, string>(typeof(ContentSizeFitter), "SizeFit"),

        new KeyValuePair<Type, string>(typeof(Image), "Img"),
        new KeyValuePair<Type, string>(typeof(RawImage), "RImg"),
        new KeyValuePair<Type, string>(typeof(Text), "Txt"),
        new KeyValuePair<Type, string>(typeof(Button), "Btn"),
        new KeyValuePair<Type, string>(typeof(InputField), "Input"),
        new KeyValuePair<Type, string>(typeof(Toggle), "Tog"),
        new KeyValuePair<Type, string>(typeof(Slider), "Slider"),
    };

    static string defaultNameFormat = "m_{0}";
    static string[] bindTypeNames;

    [MenuItem("GameObject/UITemplate/BindToLuaObj", priority = -105)]
    static void BindToLuaObj()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
            return;
        foreach(GameObject obj in Selection.gameObjects)
        {
            var binders = obj.GetComponentsInParent<LuaObjBinder>(true);
            if (binders == null || binders.Length <= 0) continue;
            var binder = binders[0];
            if (binder)
            {
                UnityEngine.Object _obj = null;
                string prefix = "";
                for (int i = LuaObjBindTypes.Count - 1; i >= 0; i--)
                {
                    Type type = LuaObjBindTypes[i].Key;
                    if (type == typeof(GameObject))
                    {
                        _obj = obj;
                    }
                    else if(type == typeof(Transform))
                    {
                        _obj = obj.transform;
                    }
                    else
                    {
                        _obj = obj.GetComponent(type);
                    }
                    if(_obj)
                    {
                        prefix = LuaObjBindTypes[i].Value;
                        break;
                    }
                }
                if (_obj)
                {
                    if (!CheckRepeatObject(binder, _obj))
                    {
                        if (!string.IsNullOrEmpty(prefix))
                        {
                            prefix = prefix + "_";
                        }
                        AddBindObj(binder, _obj, string.Format(defaultNameFormat, prefix + _obj.name));
                    }
                }
                else
                {
                    Debug.LogError("Error No Target Component Found! Object With Name:" + obj.name);
                }
            }
            else
            {
                Debug.LogError("Bind Failed! No LuaObjBinder Found In Parents! Object With Name:" + obj.name);
            }
        }
    }


    [MenuItem("GameObject/UITemplate/UnBindToLuaObj", priority = -104)]
    static void UnBindToLuaObj()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            return;
        }
        foreach (GameObject obj in Selection.gameObjects)
        {
            var binders = obj.GetComponentsInParent<LuaObjBinder>(true);
            if (binders == null || binders.Length <= 0) continue;
            var binder = binders[0];
            if (binder)
            {
                RemoveBindGameObject(binder, obj);
            }
        }
    }

    [MenuItem("GameObject/UITemplate/PrintLuaObjBindsId", priority = -106)]
    [MenuItem("Assets/UITemplate/PrintLuaObjBindsId", priority = -106)]
    static void PrintLuaObjBindsId()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            return;
        }
        var binder = Selection.gameObjects[Selection.gameObjects.Length - 1].GetComponent<LuaObjBinder>();
        Debug.Log(GenerateBindsId(binder));
    }


    public static void AddBindObj(LuaObjBinder binder, UnityEngine.Object obj, string name)
    {
        if (binder.LuaObjs.FindIndex(o => o.obj == obj) >= 0)
        {
            Debug.LogError("Add Node Repeated When Bind Lua Object! < " + obj.name + " > Name With: " + name);
            return;
        }

        Dictionary<string, bool> nameMap = new Dictionary<string, bool>();
        foreach (var node in binder.LuaObjs)
        {
            if (!nameMap.ContainsKey(node.name))
            {
                nameMap.Add(node.name, true);
            }
        }

        if (nameMap.ContainsKey(name))
        {
            for (int i = 1; i < binder.LuaObjs.Count; i++)
            {
                string newName = string.Format("{0}_{1}", name, i);
                if (!nameMap.ContainsKey(newName))
                {
                    name = newName;
                    break;
                }
            }
        }

        if (nameMap.ContainsKey(name))
        {
            Debug.LogError("Repeat Name When Bind Lua Object! < " + obj.name + " > Name With:" + name);
        }

        binder.LuaObjs.Add(new LuaObjBinder.LuaObj() { obj = obj, name = name });
        EditorUtility.SetDirty(binder);
    }

    public static bool CheckRepeatObject(LuaObjBinder binder, UnityEngine.Object obj)
    {
        return binder.LuaObjs.FindIndex(o => o.obj == obj) >= 0;
    }

    public static void RemoveBindGameObject(LuaObjBinder binder, GameObject go)
    {
        binder.LuaObjs.RemoveAll(o =>
        {
            if (o.obj != null)
            {
                Type type = o.obj.GetType();
                GameObject _go = null;
                if (type == typeof(GameObject))
                {
                    _go = o.obj as GameObject;
                }
                else if (type == typeof(Transform))
                {
                    _go = (o.obj as Transform).gameObject;
                }
                else
                {
                    _go = (o.obj as Component).gameObject;
                }
                return _go == go;
            }
            return false;
        });
    }

    public static string GenerateBindsId(LuaObjBinder binder)
    {
        System.Text.StringBuilder binds = new System.Text.StringBuilder();
        if (binder.LuaObjs.Count > 0)
        {
            binds.Append("{");
            for (int i = 0; i < binder.LuaObjs.Count; i++)
            {
                var luaObj = binder.LuaObjs[i];
                if (luaObj.obj == null)
                {
                    Debug.LogError(string.Format("Null Object in Node {0} When Generate BindsId!", i));
                    continue;
                }
                int num;
                string str = string.Empty;
                if (int.TryParse(luaObj.name, out num))
                {
                    str = string.Format("\n    [{0}] = {1},", luaObj.name, i + 1);
                }
                else
                {
                    str = string.Format("\n    [\"{0}\"] = {1},", luaObj.name, i + 1);
                }
                binds.Append(str);
                for (int j = 0; j < (40 - str.Length); j++)
                {
                    binds.Append(" ");
                }
                binds.Append(" ");
                str = string.Format("--  < {0} >", luaObj.obj.GetType().Name);
                binds.Append(str);
                for (int j = 0; j < (30 - str.Length); j++)
                {
                    binds.Append(" ");
                }
                binds.Append(" ");
                binds.Append(luaObj.obj.name);
            }
            binds.Append("\n}");
        }
        return binds.ToString();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            if(bindTypeNames == null)
            {
                bindTypeNames = new string[LuaObjBindTypes.Count];
                for(int i = 0; i< LuaObjBindTypes.Count; i++)
                {
                    var _type = LuaObjBindTypes[i].Key;
                    bindTypeNames[i] = _type.Name;
                }
            }

            //设置属性名宽度
            EditorGUIUtility.labelWidth = 60;
            position.height = EditorGUIUtility.singleLineHeight;

            var typeRect = new Rect(position)
            {
                width = position.width * 0.2f,
            };

            var objRect = new Rect(position)
            {
                width = position.width * 0.35f,
                x = position.x + position.width * 0.25f,
            };

            var nameRect = new Rect(position)
            {
                width = position.width * 0.35f,
                x = position.x + position.width * 0.65f,
            };
            //var typeIndexProperty = property.FindPropertyRelative("typeIndex");
            var objProperty = property.FindPropertyRelative("obj");
            var nameProperty = property.FindPropertyRelative("name");

            int selectedIndex = 0;//typeIndexProperty.intValue;
            Type oldType = null;
            if (objProperty.objectReferenceValue != null)
            {
                oldType = objProperty.objectReferenceValue.GetType();
                selectedIndex = LuaObjBindTypes.FindIndex(o => o.Key == oldType);
            }

            selectedIndex = EditorGUI.Popup(typeRect, selectedIndex, bindTypeNames);
            //typeIndexProperty.intValue = selectedIndex;

            Type type = null;
            if (selectedIndex >= 0 && selectedIndex < LuaObjBindTypes.Count)
            {
                type = LuaObjBindTypes[selectedIndex].Key;
            }

            if (type == null)
            {
                objProperty.objectReferenceValue = null;
            }
            else if (oldType != null && oldType != type)
            {
                GameObject go;
                if (oldType == typeof(GameObject))
                {
                    go = objProperty.objectReferenceValue as GameObject;
                }
                else if (oldType == typeof(GameObject))
                {
                    go = (objProperty.objectReferenceValue as Transform).gameObject;
                }
                else
                {
                    go = (objProperty.objectReferenceValue as Component).gameObject;
                }

                if (type == typeof(GameObject))
                {
                    objProperty.objectReferenceValue = go.gameObject;
                }
                else if (type == typeof(Transform))
                {
                    objProperty.objectReferenceValue = go.transform;
                }
                else
                {
                    objProperty.objectReferenceValue = go.GetComponent(type);
                }
            }
            var oldObj = objProperty.objectReferenceValue;
            objProperty.objectReferenceValue = EditorGUI.ObjectField(objRect, objProperty.objectReferenceValue, type, true);

            if (objProperty.objectReferenceValue != null && (oldObj != objProperty.objectReferenceValue || string.IsNullOrEmpty(nameProperty.stringValue)))
            {
                string prefix = "";
                if (selectedIndex >= 0 && selectedIndex < LuaObjBindTypes.Count)
                {
                    if (!string.IsNullOrEmpty(LuaObjBindTypes[selectedIndex].Value))
                    {
                        prefix = LuaObjBindTypes[selectedIndex].Value + "_";
                    }
                }
                nameProperty.stringValue = string.Format(defaultNameFormat, prefix + objProperty.objectReferenceValue.name);
            }
            nameProperty.stringValue = EditorGUI.TextField(nameRect, string.Empty, nameProperty.stringValue);
        }
    }
}
