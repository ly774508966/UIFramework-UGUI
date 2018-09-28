using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WindowManager : MonoBehaviour
{
    public const int DESIGN_WIDTH = 1334;
    public const int DESIGN_HEIGHT = 750;

    static WindowManager mInstance;
    public static WindowManager GetSingleton()
    {
        if(mInstance == null)
        {
            uiLayer = LayerMask.NameToLayer("UI");
            blurLayer = LayerMask.NameToLayer("Blur"); //如果没有该层请创建


            GameObject go = new GameObject(typeof(WindowManager).ToString());
            DontDestroyOnLoad(go);
            go.layer = uiLayer;
            mInstance = go.AddComponent<WindowManager>();

           
            GameObject cameraGo = new GameObject("Camera");
            cameraGo.layer = uiLayer;
            cameraGo.transform.SetParent(go.transform);
            mInstance.mCamera =  cameraGo.AddComponent<Camera>();
            mInstance.mCamera.clearFlags = CameraClearFlags.Depth;
            mInstance.mCamera.depth = 10;
            mInstance.mCamera.orthographic = true;
            mInstance.mCamera.orthographicSize = 5;
            mInstance.mCamera.cullingMask = 1 << uiLayer;

             GameObject canvasGo = new GameObject("Canvas");
            canvasGo.layer = uiLayer;
            canvasGo.transform.SetParent(go.transform);
            canvasGo.AddComponent<RectTransform>();

            mInstance.mCanvas = canvasGo.AddComponent<Canvas>();
            mInstance.mCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            mInstance.mCanvas.worldCamera = mInstance.mCamera;
             

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.scaleFactor = 1;
            scaler.referenceResolution = new Vector2(DESIGN_WIDTH, DESIGN_HEIGHT);
            scaler.referencePixelsPerUnit = 100;

            GraphicRaycaster raycaster = canvasGo.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;


            GameObject eventGo = new GameObject("EventSystem");
            eventGo.layer = uiLayer;

            eventGo.transform.SetParent(go.transform);
            mInstance.mEventSystem = eventGo.AddComponent<EventSystem>();
            mInstance.mEventSystem.sendNavigationEvents = true;
            mInstance.mEventSystem.pixelDragThreshold = 5;
           

            StandaloneInputModule inputModule = eventGo.AddComponent<StandaloneInputModule>();


        }

        return mInstance;
    }

    public static int uiLayer;
    public static int blurLayer;

    public Camera mCamera;
    public Canvas mCanvas;
    public EventSystem mEventSystem;
 
    public static void SetTouchable(bool touchable)
    {
        
        if (mInstance.mEventSystem)
        {
            mInstance.mEventSystem.enabled = touchable;
        }
    }

    private Stack<BaseWindow> mWindowStack = new Stack<BaseWindow>();
    private Stack<BaseWindow> mTmpWindowStack = new Stack<BaseWindow>();

    public void Open<T>(Action<T> callback = null) where T : BaseWindow
    {
        SetTouchable(false);

        T t = Get<T>();

        if (t)
        {
            mTmpWindowStack.Clear();

            while (mWindowStack.Count > 0)
            {
                BaseWindow window = mWindowStack.Pop();
                if (window == t)
                {
                    break;
                }
                else
                {
                    mTmpWindowStack.Push(window);
                }
            }

            while (mTmpWindowStack.Count > 0)
            {
                BaseWindow window = mTmpWindowStack.Pop();

                mWindowStack.Push(window);
            }

            mTmpWindowStack.Clear();

            Push(t, callback);
        }
        else
        {

            string path = WindowPath.Get<T>();

            if (string.IsNullOrEmpty(path) == false)
            {
                /*string tmpAssetBundleName = "assetbundle.unity3d";
                    AssetManager.GetSingleton().Load(tmpAssetBundleName, path, (varGo) => {
                    if (varGo)
                    {
                        GameObject go = AssetManager.Instantiate(tmpAssetBundleName, path, varGo);

                        NGUITools.SetLayer(go, uiLayer);

                        Transform tran = go.transform.Find(typeof(T).ToString());

                        tran.SetParent(uiRoot.transform);

                        Destroy(go);

                        tran.localPosition = Vector3.zero;
                        tran.localRotation = Quaternion.identity;
                        tran.localScale = Vector3.one;

                        tran.gameObject.SetActive(true);


                        t = tran.GetComponent<T>();

                        if (t == null) t = tran.gameObject.AddComponent<T>();

                        if (t.windowType == WindowType.Root)
                        {
                            BaseWindow window = Get(WindowType.Root);

                            if (window != null)
                            {
                                Destroy(tran.gameObject);
                                SetTouchable(true);

                                return;
                            }
                        }

                        t.path = path;

                        t.OnEnter();

                        Push(t, callback);
                    }
                    else
                    {
                        SetTouchable(true);
                    }
                });
                */

                GameObject asset = Resources.Load<GameObject>(path);

                if (asset)
                {
                    GameObject go = Instantiate<GameObject>(asset);

                    //NGUITools.SetLayer(go, uiLayer);

                    Transform tran = go.transform.Find(typeof(T).ToString());

                    tran.SetParent(mCanvas.transform);

                    Destroy(go);

                    tran.localPosition = Vector3.zero;
                    tran.localRotation = Quaternion.identity;
                    tran.localScale = Vector3.one;

                    tran.gameObject.SetActive(true);


                    t = tran.GetComponent<T>();

                    if (t == null) t = tran.gameObject.AddComponent<T>();

                    if(t.windowType == WindowType.Root)
                    {
                        BaseWindow window = Get(WindowType.Root);

                        if(window!=null)
                        {
                            Destroy(tran.gameObject);
                            SetTouchable(true);

                            return;
                        }
                    }

                    t.path = path;

                    t.OnEnter();

                    Push(t, callback);
                }
                else
                {
                    SetTouchable(true);
                }
                
            }
            else
            {
                SetTouchable(true);
            }
        }
    }

    private void Push<T>(T t, Action<T> callback) where T : BaseWindow
    {
        if (t)
        {
            if (mWindowStack.Count > 0)
            {
                //打开Root 关闭其他的
                if (t.windowType == WindowType.Root)
                {
                    while (mWindowStack.Count > 0)
                    {
                        BaseWindow window = mWindowStack.Pop();

                        if (window)
                        {
                            if (window != t)
                            {
                                window.OnExit();
                            }
                        }
                    }
                }
                else if (t.windowType == WindowType.Pop)
                {
                    //Pop类型的不需要暂停上一个窗口
                }
                else
                {
                    //暂停上一个界面
                    BaseWindow window = mWindowStack.Peek();

                    if (window && window.isPause == false)
                    {
                        window.OnPause();
                    }
                }
            }

            SetLayer(t.transform);

            mWindowStack.Push(t);
        }

        SetTouchable(true);

        if (callback != null)
        {
            callback(t);
        }

        if (t)
        {
            t.OnResume();
        }
    }

    public T Get<T>() where T :BaseWindow
    {
        if(mWindowStack == null)
        {
            mWindowStack = new Stack<BaseWindow>();
        }

        var it = mWindowStack.GetEnumerator();

        while(it.MoveNext())
        {
            Type type = it.Current.GetType();
            if(type == typeof(T))
            {
                return it.Current as T;
            }
        }

        return null;
    }

    /// <summary>
    /// 关闭最上面的UI,不会关闭Root窗口
    /// </summary>
    public void Close<T>() where T:BaseWindow
    {
        T t = Get<T>();
        if (t)
        {
            Close(t);
        }
    }

    public void Close(BaseWindow baseWindow)
    {
        if (baseWindow == null ||mWindowStack == null) return;
        if (mTmpWindowStack == null) mTmpWindowStack = new Stack<BaseWindow>();

        SetTouchable(false);

        while (mWindowStack.Count > 0)
        {
            BaseWindow window = mWindowStack.Pop();
            if (window != baseWindow)
            {
                mTmpWindowStack.Push(mWindowStack.Pop());
            }
            else
            {
                break;
            }
        }

        baseWindow.OnExit();

        while (mTmpWindowStack.Count > 0)
        {
            mWindowStack.Push(mTmpWindowStack.Pop());
        }

        if (mWindowStack.Count > 0)
        {
            BaseWindow window = mWindowStack.Peek();

            if (window && window.isPause)
            {
                window.OnResume();
            }
        }

        SetTouchable(true);
    }



    private BaseWindow Get(WindowType windowType) 
    {
        var it = mWindowStack.GetEnumerator();

        while(it.MoveNext())
        {
            if(it.Current.windowType == windowType)
            {
                return it.Current;
            }
        }
        return null;
    }

    private void SetLayer(Transform parent)
    {
       
        if(parent)
        {
            if (parent.gameObject.layer == uiLayer)
            {
                return;
            }
            parent.gameObject.layer = uiLayer;

            for(int i = 0, max = parent.childCount; i < max; ++i)
            {
                SetLayer(parent.GetChild(i));
            }
        }
    }

    /// <summary>
    /// 暂停所有窗口
    /// </summary>
    public void Hide()
    {
        var it = mWindowStack.GetEnumerator();

        while(it.MoveNext())
        {
            if (it.Current.isPause == false)
            {
                it.Current.OnPause();
            }
        }
    }

    /// <summary>
    /// 显示栈顶的窗口
    /// </summary>
    public void Show()
    {
        if (mWindowStack.Count > 0)
        {
            BaseWindow window = mWindowStack.Pop();
            if (window)
            {
                if (window.windowType == WindowType.Pop)
                {
                    if (mWindowStack.Count > 0)
                    {
                        mTmpWindowStack.Clear();
                        while (mWindowStack.Count > 0)
                        {
                            BaseWindow w = mWindowStack.Pop();
                            if (w.windowType == WindowType.Normal
                                || w.windowType == WindowType.Root)
                            {
                                mTmpWindowStack.Push(w);
                                break;
                            }
                            else if (w.windowType == WindowType.Pop)
                            {
                                mTmpWindowStack.Push(w);
                            }
                        }

                        while (mTmpWindowStack.Count > 0)
                        {
                            BaseWindow w = mTmpWindowStack.Pop();
                            mWindowStack.Push(w);
                            w.OnResume();
                        }
                        mTmpWindowStack.Clear();

                    }
                }

                mWindowStack.Push(window);

                window.OnResume();

            }
        }
    }

    public void CloseAll()
    {
        while(mWindowStack.Count>0)
        {
            BaseWindow window = mWindowStack.Pop();

            if(window)
            {
                window.OnExit();
            }
        }
        mWindowStack.Clear();
    }

  
    public bool TouchUI()
    {
        bool touchedUI = false;
        if (Application.isMobilePlatform)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                touchedUI = true;
            }
        }
        else if (EventSystem.current.IsPointerOverGameObject())
        {
            touchedUI = true;
        }
        return touchedUI;
    }
}

