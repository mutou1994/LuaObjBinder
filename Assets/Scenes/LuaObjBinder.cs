using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using LuaInterface;

public class LuaObjBinder : MonoBehaviour
{
    [Serializable]
    public struct LuaObj
    {
        [SerializeField]
        public string name;
        [SerializeField]
        public UnityEngine.Object obj;
    }

    [SerializeField]
    List<LuaObj> m_LuaObjs = new List<LuaObj>();

    //[NoToLua]
    public List<LuaObj> LuaObjs 
    {
        get
        {
            return m_LuaObjs;
        }
    }

    /*public LuaTable GenLuaObjBindsTbl()
    {
        if (m_LuaObjs == null || m_LuaObjs.Count == 0) return null;

        var L = LuaClient.GetMainState();
        int oldTop = L.LuaGetTop();
        L.LuaNewTable();
        for(int i = 0; i < m_LuaObjs.Count; i++)
        {
            var luaObj = m_LuaObjs[i];
            L.LuaPushInteger(i + 1);
            L.PushObject(LuaObj.obj);
            L.LuaSetTable(oldTop + 1);
        }
        LuaTable tbl = L.CheckLuaTable(oldTop + 1);
        L.LuaSetTop(oldTop);
        return tbl;
    }*/

    private void OnDestroy()
    {
        m_LuaObjs = null;
    }
}
