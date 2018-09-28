using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UI;

/*
 * 
 * 根据UI的prefab中的物件命名自动生成初始化代码
 * 命名规则 name@type.variableName
 * 解释：物体名称（可有可无）@要获取该物体上的对象类型.自动生成代码的变量名
 * 如：Icon@sprite.mItemIcon
 * 生成初始化代码 UISprite mItemIcon = transform.Find("XXXX/Icon").GetComponent<UISprite>();
 * XXXX是该物体在prefab中的路径
*/

public class Variable
{
    string name = string.Empty;
    string type = string.Empty;
    string path = string.Empty;

    public Variable(string varName, string varType, string varPath)
    {
        name = varName;
        type = varType;
        path = varPath;
    }
    public string Name { get { return name; } }
    public string Type { get { return type; } }
    public string Path { get { return path; } }

}

public class UIEditor  {

    static string header = "using UnityEngine;\nusing System.Collections;\n\n/// <summary>\n/// 该文件由工具自动生成，不要修改！\n/// </summary>\n\n";
    static string func = "\tpublic T GetComponent<T>(string path) where T:Component\n\t{\n\t\tTransform child = transform.Find(path);\n\t\tif(child)\n\t\t{\n\t\t\treturn child.GetComponent<T>();\n\t\t}\n\t\treturn null;\n\t}\n";
    [MenuItem("Assets/Create UIBase", true)]
    [MenuItem("Assets/Create UIBase(FadeWindow)",true)]
    [MenuItem("Assets/Create UIBase(MoveWindow)", true)]
    [MenuItem("Assets/Create UIBase(ScaleWindow)", true)]
    static bool IsPrefab()
    {
        return (Selection.activeObject !=null &&Selection.activeObject.GetType() == typeof(GameObject));
    }

    [MenuItem("Assets/Create UIBase")]
    static void CreateBaseWindow()
    {
        CreateCSharpFile("BaseWindow");
    }
    [MenuItem("Assets/Create UIBase(FadeWindow)")]
    static void CreateFadeWindow()
    {
        CreateCSharpFile("FadeWindow");
    }
    [MenuItem("Assets/Create UIBase(MoveWindow)")]
    static void CreateMoveWindow()
    {
        CreateCSharpFile("MoveWindow");
    }
    [MenuItem("Assets/Create UIBase(ScaleWindow)")]
    static void CreateScaleWindow()
    {
        CreateCSharpFile("ScaleWindow");
    }

    static void CreateCSharpFile(string baseWindow)
    {
        GameObject ui = Selection.activeGameObject;
        if(!ui)
        {
            Debug.Log("请选择一个UI Prefab");
            return;
        }
       
        Dictionary<string, Variable> variableDir = ParseUIPrefab(ui);

        if (variableDir == null || variableDir.Count ==0)
            return;

        string fullpath = GetCSharpFileName(ui);
        if (File.Exists(fullpath))
            File.Delete(fullpath);

        try
        {
            FileStream fs = new FileStream(fullpath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(WriteString(ui.name, baseWindow, variableDir));
            sw.Close();
        }
        catch(System.Exception e)
        {
            throw e;
        }

        Debug.Log("Done!!");
        AssetDatabase.Refresh();
    }

    static string WriteString(string className,string baseWindow, Dictionary<string, Variable> variableDir)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(header);
        builder.Append("public class ");
        builder.Append(className + "Base");
        builder.Append(" : "+ baseWindow + " {\n\n");
        foreach (var v in variableDir)
        {
            builder.Append(string.Format("\t[SerializeField]private {0} _{1};\n", v.Value.Type, v.Key));
            builder.Append(string.Format("\tpublic {0} {1}{{get{{if(_{2}==null){{_{3}= GetComponent<{4}>(\"{5}\");}}return _{6}; }} }} \n", v.Value.Type, v.Key,v.Key, v.Key,v.Value.Type,v.Value.Path, v.Key));
        }
        builder.Append("\n\n");
        builder.Append(func);
        builder.Append("\n\n");
        builder.Append("\t[ContextMenu(\"Serialize\")]private void Serialize()\n");
        builder.Append("\t{\n");

        foreach (var v in variableDir)
        {
            builder.Append(string.Format("\t\t_{0} = GetComponent<{1}>(\"{2}\");\n",v.Value.Name,v.Value.Type,v.Value.Path));
        }

        builder.Append("\t}\n");

        builder.Append("}\n");

        return builder.ToString();
    }
    static Dictionary<string, Variable> ParseUIPrefab(GameObject ui)
    {
        Dictionary<string, Variable> variableDir = new Dictionary<string, Variable>();

        if (ui)
        {
            Transform[] childs = ui.GetComponentsInChildren<Transform>(true);

            foreach (var t in childs)
            {
                Transform child = t;
                if (child == ui.transform)
                    continue;

                string name = child.name;
                if (!name.Contains("@"))
                    continue;

                int index = name.IndexOf('@');
                if (index >= name.Length - 1)
                    continue;

                string variableNameAndType = name.Substring(index + 1);

                string variableName, type, path;

                if (variableNameAndType.Contains("."))
                {
                    string[] nameAndTypes = variableNameAndType.Split('.');
                    if(nameAndTypes.Length != 2)
                    {
                        variableDir.Clear();
                        Debug.LogError(string.Format("命名错误：{0}", name));
                        return variableDir;
                    }

                    type = nameAndTypes[0];
                    variableName = nameAndTypes[1];

                }else
                {
                    type = "Transform";
                    variableName = variableNameAndType;
                }

                System.Type variableType = GetType(type);
                if(variableType==null)
                {
                    variableDir.Clear();
                    Debug.LogError(string.Format("命名错误,没定义该类型：{0}", name));
                    return variableDir;
                }

                if(!child.GetComponent(variableType))
                {
                    variableDir.Clear();
                    Debug.LogError(string.Format("给定的物体{0}没有{1}类型的组件", name, variableType));
                    return variableDir;
                }
              
                path = name;

                while (child.parent && child.parent != ui.transform)
                {
                    path = string.Format("{0}/{1}", child.parent.name, path);
                    child = child.parent;
                }

                if(!ui.transform.Find(path))
                {
                    Debug.LogError(string.Format("根据路径 path = {0}未能找到物体！", path));
                    variableDir.Clear();

                    return variableDir;
                }

                if(!ui.transform.Find(path).GetComponent(variableType))
                {
                    Debug.LogError(string.Format("根据路径 path = {0}未能找到物体上的{1}组件！", path, variableType.ToString()));
                    variableDir.Clear();
                    return variableDir;
                }

                if (variableDir.ContainsKey(variableName))
                {
                    Debug.LogError(string.Format("重复变量名：{0}, path = {1}", variableName, path));
                    variableDir.Clear();
                    return variableDir;
                }
                else
                {
                    if (!string.IsNullOrEmpty(variableName) && !string.IsNullOrEmpty(variableType.ToString()) && !string.IsNullOrEmpty(path))
                    {
                        variableDir.Add(variableName, new Variable(variableName,variableType.ToString(),path));
                    }
                    else
                    {
                        variableDir.Clear();
                        Debug.LogError(string.Format("命名错误：{0}", name));
                        return variableDir;
                    }
                }
            }
        }

        return variableDir;
    }


    static System.Type GetType(string type)
    {
        switch (type)
        {
            case "Gameobject":return typeof(RectTransform);
            case "Transform": return typeof(RectTransform);
            case "Canvas": return typeof(Canvas);
            case "Text": return typeof(Text);
            case "Image":return typeof(Image);
            case "Button": return typeof(Button);
            case "Toggle": return typeof(Toggle);
            case "RawImage": return typeof(RawImage);
            case "Slider": return typeof(Slider);
            case "Scrollbar": return typeof(Scrollbar);
            case "Dropdown": return typeof(Dropdown);
            case "InputField": return typeof(InputField);
            case "ScrollView": return typeof(ScrollRect);
            default: return null;
        }
    }

    static string GetCSharpFileName(GameObject ui)
    {
        string path = Application.dataPath;
        path = path.Substring(0, path.LastIndexOf('/') + 1);
        path = Path.Combine(path, AssetDatabase.GetAssetPath(ui));
        path = path.Substring(0, path.LastIndexOf('/') + 1);
        path = path + "Scripts/Base/";
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path = path + ui.name + "Base.cs";
        return path;
    }
}
