using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public delegate void OnExitHandler();

public class BaseWindow : MonoBehaviour {

    private string mPath;

    /// <summary>
    /// 界面预设路径
    /// </summary>
    public string path { get { return mPath; } set { mPath = value; } }

    /// <summary>
    /// 界面推出的回调
    /// </summary>
    public event OnExitHandler onExit;

    private bool mPause = false;
    public bool isPause { get { return mPause; } }

    protected WindowType mWindowType = WindowType.Normal;
    public WindowType windowType { get { return mWindowType; } }

    protected bool mUseMask = false;

    private CanvasGroup mPanel;
    public CanvasGroup panel
    {
        get
        {
            if(mPanel==null)
            {
                mPanel = GetComponent<CanvasGroup>();

                if(mPanel == null)
                {
                    mPanel = gameObject.AddComponent<CanvasGroup>();
                }
            }
            return mPanel;
        }
    }

    private GameObject mMask;
    public GameObject mask
    {
        get {
            if (mMask == null) CreateMask();
            return mMask;
        }
    }

    void CreateMask()
    {
        GameObject go = new GameObject("Mask");
      
        go.transform.SetParent(transform);
        go.transform.SetAsFirstSibling();
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;

        mMask = go;

        Image image = go.AddComponent<Image>();
        image.raycastTarget = true;
        image.color = new Color(0, 0, 0, 30f/255);
        
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.pivot = Vector2.one * 0.5f;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        
    }


    /// <summary>
    /// 界面被创建出来，只在创建完成调用一次
    /// </summary>
    public virtual void OnEnter()
    {
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.pivot = Vector2.one * 0.5f;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMax = Vector2.zero;
        rect.offsetMin = Vector2.zero;

        if(mUseMask)
        {
            CreateMask();
        }
    }

    /// <summary>
    /// 界面暂停
    /// </summary>
    public virtual void OnPause()
    {
        if(panel)
        {
            panel.alpha = 0;
            SetTouchEnable(false);
        }
     
        mPause = true;
    }

    /// <summary>
    /// 界面继续
    /// </summary>
    public virtual void OnResume()
    {
        mPause = false;

        if (panel)
        {
            panel.alpha = 1;
            SetTouchEnable(true);
        }

        transform.SetAsLastSibling();
    }

    public virtual void SetTouchEnable(bool enable)
    {
        if (panel)
        {
            panel.blocksRaycasts = enable;
        }
    }

    /// <summary>
    /// 界面退出，只在界面被销毁时调用一次
    /// </summary>
    public virtual void OnExit()
    {
        Destroy(gameObject);

        if(onExit!=null)
        {
            onExit();
        }
    }

    /// <summary>
    /// 不需要主动调用，绑定到界面的关闭或返回按钮就行
    /// </summary>
    protected virtual void Close()
    {
        WindowManager.GetSingleton().Close(this);
    }
}
