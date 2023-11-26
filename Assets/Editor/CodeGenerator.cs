using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// v1
    /// 生成查找component的代码，如下
    /// Button button = go.Find("path/to/the/target/button").GetComponent<Button>();
    ///
    /// 采用控件名字后缀匹配的方式：
    /// _tr: Transform
    /// _btn: Button
    /// _img: Image
    /// _anim: Animation
    /// _txt: Text
    /// _tmp: TMP_Text
    ///
    /// 点击生成后，将以选中的object的名字生成一个class，该object下，所有符合后缀的component都将
    /// 以public字段的形式生成到该class下，同时通过Find方式找到目标控件为其赋值
    /// </summary>
    public static class CodeGenerator
    {
        private const string HotfixCodeTargetDir = "Assets/../Hotfix/Hotfix/Scripts/Generated";
        private const string HotfixCodeTargetNamespace = "Gaia.Hotfix";

        private const string UnityCodeTargetDir = "Assets/Scripts/Generated";
        private const string UnityCodeTargetNamespace = "Gaia";

        private static string codeTargetDir;
        private static string codeTargetNamespace;

        private static readonly Dictionary<string, string> SuffixMap = new()
        {
            {"_tr", "Transform"},
            {"_rect", "RectTransform"},
            {"_btn", "Button"},
            {"_img", "Image"},
            {"_txt", "Text"},
            {"_tmp", "TMP_Text"},
            {"_anim", "Animation"}
        };

        [MenuItem("Tools/生成【获取控件代码到Hotfix】")]
        private static void GenHotfixFindComponentCode()
        {
            codeTargetDir = HotfixCodeTargetDir;
            codeTargetNamespace = HotfixCodeTargetNamespace;

            GenFindComponentCode();
        }

        [MenuItem("Tools/生成【获取控件代码到Unity】")]
        private static void GenUnityFindComponentCode()
        {
            codeTargetDir = UnityCodeTargetDir;
            codeTargetNamespace = UnityCodeTargetNamespace;

            GenFindComponentCode();
        }

        private static void GenFindComponentCode()
        {
            var root = Selection.activeGameObject;
            if (root == null)
            {
                Debug.LogWarning("no selected game object");
                return;
            }

            var className = root.name;
            foreach (var kv in SuffixMap)
            {
                if (className.EndsWith(kv.Key)) className = className.Replace(kv.Key, "");
            }
            var sbUsing = new StringBuilder();
            var sbFields = new StringBuilder();
            var sbFinders = new StringBuilder();

            var allPath = new List<string>();
            GetChildrenPath(root.transform, "", allPath);

            var useTmp = false;

            foreach (var p in allPath)
            {
                var childName = p;

                var separatorIndex = p.LastIndexOf("/", StringComparison.Ordinal);
                if (separatorIndex >= 0)
                {
                    childName = p.Substring(separatorIndex + 1);
                }

                var underLineIndex = childName.LastIndexOf("_", StringComparison.Ordinal);
                if (underLineIndex < 0) continue;

                var suffix = childName.Substring(underLineIndex);

                if (SuffixMap.TryGetValue(suffix, out var typeName))
                {
                    if (suffix == "_tmp") useTmp = true;

                    sbFields.AppendLine($"\t\tpublic {typeName} {childName};");
                    sbFinders.AppendLine($"\t\t\t{childName} = root.Find(\"{p}\").GetComponent<{typeName}>();");
                }
                else
                {
                    Debug.LogWarning($"undefined suffix found:{suffix}, path:{p}");
                }
            }

            if (useTmp) sbUsing.AppendLine("using TMPro;");

            var template = File.ReadAllText(Path.Join(Application.dataPath, "Editor/CodeTemplate.txt"));

            var replace = new Dictionary<string, string>
            {
                {"__Using__", sbUsing.ToString()},
                {"__NameSpace__", codeTargetNamespace},
                {"__ClassName__", className},
                {"__Components__", sbFields.ToString()},
                {"__FindComponents__", sbFinders.ToString()},
            };

            foreach (var kv in replace)
            {
                template = template.Replace(kv.Key, kv.Value);
            }

            // AppLogger.Log(template);

            if (!Directory.Exists(codeTargetDir))
            {
                Directory.CreateDirectory(codeTargetDir);
            }

            File.WriteAllText($"{codeTargetDir}/{className}Part.cs", template);

            Debug.Log("generate finish");
        }

        private static T GetComponent<T>(Transform t, string path)
        {
            var c = t.Find(path).GetComponent<T>();
            return c;
        }

        private static void GetChildrenPath(Transform t, string parentPath, List<string> childrenPath)
        {
            var count = t.childCount;
            for (var i = 0; i < count; ++i)
            {
                var child = t.GetChild(i);

                if (child.name.Contains("Ignore")) continue;

                var path = $"{parentPath}{child.name}";
                childrenPath.Add(path);

                GetChildrenPath(child, $"{path}/", childrenPath);
            }
        }
    }
}