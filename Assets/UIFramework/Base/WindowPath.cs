using System;
using System.Collections.Generic;

public class WindowPath
{
    readonly static Dictionary<string, string> windowPath = new Dictionary<string, string>
    {
        {typeof(UI_Main).ToString(),"Assets/UI/UI_Main/UI_Main.prefab" },
        {typeof(UI_ScaleWindow).ToString(),"Assets/UI/UI_ScaleWindow/UI_ScaleWindow.prefab" },
        {typeof(UI_FadeWindow).ToString(),"Assets/UI/UI_FadeWindow/UI_FadeWindow.prefab" },
        {typeof(UI_MoveWindow).ToString(),"Assets/UI/UI_MoveWindow/UI_MoveWindow.prefab" },
        {typeof(UI_PopWindow).ToString(),"Assets/UI/UI_PopWindow/UI_PopWindow.prefab" },
        {typeof(UI_Dialog).ToString(),"Assets/UI/UI_Dialog/UI_Dialog.prefab" },
    };

    public static string Get<T>() where T: BaseWindow
    {
        string type = typeof(T).ToString();

        if(windowPath.ContainsKey(type))
        {
            return windowPath[type];
        }
        return "";
    }
}


