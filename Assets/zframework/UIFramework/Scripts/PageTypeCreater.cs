using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class PageTypeCreater
{
#if UNITY_EDITOR
    const string key = "UIManagerCachedBindNeededObject";
    static List<TypeBindingConfiguration> binds = new List<TypeBindingConfiguration>();

    [MenuItem("Assets/UIManager/Create Page Type")]
    static void CreatScriptForPagePrefab()
    {
        string TypeNameProvider(GameObject target)
        {
            var name = target.name;
            if (name.EndsWith("Panel") || name.EndsWith("Page"))
            {
                if (name.EndsWith("Panel"))
                {
                    name = name.Substring(0, name.Length - 5) + "Page";
                }
            }
            return name;
        }
        GenerateTypes("Assets/Scripts/UI/Pages", TypeNameProvider);
    }
    [MenuItem("Assets/UIManager/Create Panel Type")]
    static void CreatScriptForPanel()
    {
        string TypeNameProvider(GameObject target) => target.name;
        GenerateTypes("Assets/Scripts/UI/Panels", TypeNameProvider, false);
    }

    private static void GenerateTypes(string path, Func<GameObject, string> typeNameProvider, bool overwrite = true)
    {
        if (EditorApplication.isCompiling)
        {
            Debug.Log($"{nameof(PageTypeCreater)}: 编译时无法做此类操作！");
            return;
        }
        EditorApplication.LockReloadAssemblies();
        var objs = Selection.gameObjects;
        binds.Clear();
        foreach (var item in objs)
        {
            var name = typeNameProvider.Invoke(item);
            //注意：完全复写 已存在的文件
            var saveto = $"{path}/{name}.cs";
            if (overwrite || !System.IO.File.Exists(saveto))
            {
                var scr = @$"using zFramework.UI;
public class {name} : BasePage
{{
}}";
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                try
                {
                    System.IO.File.WriteAllText(saveto, scr, System.Text.Encoding.UTF8);
                }
                catch (Exception)
                {
                    EditorApplication.UnlockReloadAssemblies();
                    AssetDatabase.Refresh();
                }

                var bind = new TypeBindingConfiguration
                {
                    type = name,
                    guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item)),
                };
                binds.Add(bind);
            }
        }
        // 后处理步骤：将生成的类型存到 EditorPrefs 以便从新加载 Assembly 后在预制体上挂脚本
        if (binds.Count > 0)
        {
            EditorPrefs.SetString(key, JsonUtility.ToJson(new Serialization<TypeBindingConfiguration>(binds), false));
        }
        EditorApplication.UnlockReloadAssemblies();
        AssetDatabase.Refresh();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void AttachComponents()
    {
        if (EditorPrefs.HasKey(key))
        {
            var json = EditorPrefs.GetString(key);
            Serialization<TypeBindingConfiguration> obj = JsonUtility.FromJson<Serialization<TypeBindingConfiguration>>(json);
            var list = obj.ToList();

            if (null != list && list.Count > 0)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var defaultAssembly = assemblies.First(assembly => assembly.GetName().Name == "Assembly-CSharp");
                foreach (var item in list)
                {
                    var go = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(item.guid)) as GameObject;
                    if (go.GetComponent(item.type)) continue; //如果类型已然挂载则跳过
                    var type = defaultAssembly.GetType(item.type);
                    if (null != type)
                    {
                        go.AddComponent(type);
                        EditorUtility.SetDirty(go);
                    }
                }
            }
            EditorPrefs.DeleteKey(key);
            Debug.Log($"{nameof(PageTypeCreater)}:  完成 Page 的 Type 绑定！");
        }
    }
    [Serializable]
    class TypeBindingConfiguration
    {
        public string guid;
        public string type;
    }
#endif
}
