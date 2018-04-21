using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WindowManager : MonoBehaviour
{
    public const int DESIGN_WIDTH = 1280;
    public const int DESIGN_HEIGHT = 720;

    static WindowManager mInstance;
    public static WindowManager GetSingleton()
    {
        if(mInstance == null)
        {
            GameObject go = new GameObject(typeof(WindowManager).ToString());
            DontDestroyOnLoad(go);
            mInstance = go.AddComponent<WindowManager>();

            uiLayer = LayerMask.NameToLayer("UI");
            blurLayer = LayerMask.NameToLayer("Blur"); //如果没有该层请创建

            GameObject canvasGo = new GameObject("Canvas");
            canvasGo.transform.SetParent(go.transform);
            canvasGo.AddComponent<RectTransform>();

            mInstance.mCanvas = canvasGo.AddComponent<Canvas>();
            mInstance.mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1;
            scaler.referencePixelsPerUnit = 100;

            GraphicRaycaster raycaster = canvasGo.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;


            GameObject eventGo = new GameObject("EventSystem");
            eventGo.transform.SetParent(go.transform);
            mInstance.mEventSystem = eventGo.AddComponent<EventSystem>();
            mInstance.mEventSystem.sendNavigationEvents = true;
            mInstance.mEventSystem.pixelDragThreshold = 5;
           

            StandaloneInputModule inputModule = eventGo.AddComponent<StandaloneInputModule>();

       

            //GameObject blurGo = Instantiate(uiCamera.gameObject) as GameObject;
            //blurGo.transform.SetParent(uiRoot.transform);
            //blurCamera = blurGo.GetComponent<UICamera>();
            /*
                        camera = blurCamera.GetComponent<Camera>();
                        camera.clearFlags = CameraClearFlags.Depth;
                        //NGUITools.MakeMask(blurCamera.GetComponent<Camera>(), blurLayer);
                        //NGUITools.SetLayer(blurGo, blurLayer);
                        camera.depth = 0;*/
            ///blurGo.AddComponent<BlurEffect>();
            //blurCamera.enabled = false;
            //blurEffect.enabled = false;

        }

        return mInstance;
    }

    public static int uiLayer;
    public static int blurLayer;

    public Canvas mCanvas;
    public EventSystem mEventSystem;
    // public static UICamera blurCamera;
    // public static UIRoot uiRoot;
    public static BlurEffect blurEffect
    {
        get
        {
            /*
            if(blurCamera)
            {
                if(blurCamera.GetComponent<BlurEffect>()==null)
                {
                    blurCamera.gameObject.AddComponent<BlurEffect>();
                }
                return blurCamera.GetComponent<BlurEffect>();
            }
            */
            return null;
        }
    }
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

                GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

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
                else if(t.windowType == WindowType.Pop)
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

            t.OnResume();
        }

        SetTouchable(true);

        if (callback != null)
        {
            callback(t);
        }
    }

    public T Get<T>()where T :BaseWindow
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
    public void Close()
    {
        if (mWindowStack == null) return;

        if(mWindowStack.Count > 0)
        {
            SetTouchable(false);

            BaseWindow window = mWindowStack.Peek();

            if(window && window.windowType != WindowType.Root)
            {
                mWindowStack.Pop();
                window.OnExit();
            }

       
            if(mWindowStack.Count >0)
            {
                window = mWindowStack.Peek();

                if (window && window.isPause)
                {
                    window.OnResume();
                }
            }

            SetTouchable(true);
        }
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
                    if(mWindowStack.Count > 0)
                    {
                        mTmpWindowStack.Clear();
                        while (mWindowStack.Count > 0)
                        {
                            var  w = mWindowStack.Peek();
                            if(w.windowType != WindowType.Pop)
                            {
                                w.OnResume();
                                break;
                            }
                            else
                            {
                                w = mWindowStack.Pop();
                                w.OnResume();
                                mTmpWindowStack.Push(w);
                            }
                        }

                        while(mTmpWindowStack.Count > 0)
                        {
                            mWindowStack.Push(mTmpWindowStack.Pop());
                        }
                        mTmpWindowStack.Clear();

                    }
                }

                mWindowStack.Push(window);
                if (window.isPause)
                {
                    window.OnResume();
                }
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

    public void SetBlur()
    {
        if(mWindowStack.Count > 0)
        {
            BaseWindow w = mWindowStack.Pop();
            //NGUITools.SetLayer(w.gameObject, uiLayer);

            if(mWindowStack.Count > 0)
            {
                BaseWindow b = mWindowStack.Peek();
                //NGUITools.SetLayer(b.gameObject, blurLayer);
            }

            mWindowStack.Push(w);
        }
       // blurEffect.enabled = mWindowStack.Count > 1;
    }


}

