using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(LuaObjBinder), true)]
public class LuaObjBinderInspector : Editor
{
    private ReorderableList _luaObjArray;
    string typeStr = "Type";
    string objectStr = "Object";
    string nameStr = "Name";
    LuaObjBinder binder;

    private void OnEnable()
    {
        binder = target as LuaObjBinder;
        _luaObjArray = new ReorderableList(serializedObject, serializedObject.FindProperty("m_LuaObjs"), true, true, true, true);

        _luaObjArray.drawHeaderCallback = (Rect rect) =>
        {
            GUI.Label(new Rect(rect)
            {
                x = rect.x + rect.width * 0.08f,
            }, typeStr);

            GUI.Label(new Rect(rect)
            {
                x = rect.x + rect.width * 0.37f,
            }, objectStr);

            GUI.Label(new Rect(rect)
            {
                x = rect.x + rect.width * 0.77f,
            }, nameStr);
        };

        //定义元素的高度
        _luaObjArray.elementHeight = 20;

        _luaObjArray.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
        {
            //根据index获取对应元素
            SerializedProperty item = _luaObjArray.serializedProperty.GetArrayElementAtIndex(index);
            rect.height -= 4;
            rect.y += 2;
            EditorGUI.LabelField(rect, (index + 1).ToString());
            rect.x += 25;
            rect.width -= 25;
            EditorGUI.PropertyField(rect, item);

            //允许元素重复
            /*var typeIndexProperty = item.FindPropertyRelative("typeIndex");
            var objProperty = item.FindPropertyRelative("obj");
            var nameProperty = item.FindPropertyRelative("name");
            int preIndex = binder.LuaObjs.FindIndex(o => o.obj == objProperty.objectReferenceValue);
            if (preIndex >= 0 && preIndex != index)
            {
                objProperty.objectReferenceValue = null;
                nameProperty.stringValue = string.Empty;
            }*/
        };

        _luaObjArray.onReorderCallback = (ReorderableList list) =>
        {
            Debug.LogError("调整组件顺序后需要重新生成Lua代码！！");
        };

        _luaObjArray.onRemoveCallback = (ReorderableList list) =>
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //自动布局绘制列表
        _luaObjArray.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
