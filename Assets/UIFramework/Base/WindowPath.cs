using System;
using System.Collections.Generic;

public class WindowPath
{
    readonly static Dictionary<string, string> windowPath = new Dictionary<string, string>
    {
        {typeof(UI_StartUp).ToString(),"R/UI/UI_StartUp/UI_StartUp" },
        {typeof(UI_Login).ToString(),"R/UI/UI_Login/UI_Login" },
        
        {typeof(UI_Main).ToString(),"R/UI/UI_Main/UI_Main" },
      
        {typeof(UI_Confirm).ToString(),"R/UI/UI_Confirm/UI_Confirm" },
        {typeof(UI_Loading).ToString(),"R/UI/UI_Loading/UI_Loading" },
        {typeof(UI_Tips).ToString(),"R/UI/UI_Tips/UI_Tips" },

        {typeof(UI_LoadingMap).ToString(),"R/UI/UI_Loading/UI_LoadingMap" },
        {typeof(UI_Message).ToString(),"R/UI/UI_Message/UI_Message" },

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


