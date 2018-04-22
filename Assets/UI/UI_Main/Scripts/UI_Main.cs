using UnityEngine;
using System.Collections;

public class UI_Main : BaseWindow
{
    public UI_Main()
    {
        mWindowType = WindowType.Root;
    }
    // Use this for initialization
    void Start ()
   {

        Transform fadeWindow = transform.Find("FadeWindow");
        UIEventListener.Get(fadeWindow.gameObject).onClick = (go) => {
            WindowManager.GetSingleton().Open<UI_FadeWindow>();
        };

        Transform scaleWindow = transform.Find("ScaleWindow");
        UIEventListener.Get(scaleWindow.gameObject).onClick = (go) =>
        {
            WindowManager.GetSingleton().Open<UI_ScaleWindow>();
        };


        Transform moveWindow = transform.Find("MoveWindow");
        UIEventListener.Get(moveWindow.gameObject).onClick = (go) =>
        {
            WindowManager.GetSingleton().Open<UI_MoveWindow>();
        };

        Transform popWindow = transform.Find("PopWindow");
        UIEventListener.Get(popWindow.gameObject).onClick = (go) =>
        {
            WindowManager.GetSingleton().Open<UI_PopWindow>();
        };

     
    }


	
	// Update is called once per frame
	void Update () {
	
	}
}
