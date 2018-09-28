using UnityEngine;


[AddComponentMenu("Tween/Tween Anchored Position3D")]
class TweenAnchoredPosition : UITweener
{
    public Vector3 from;
    public Vector3 to;

    RectTransform mTrans;
    //UIRect mRect;

    public RectTransform cachedTransform { get { if (mTrans == null) mTrans = GetComponent<RectTransform>(); return mTrans; } }


    /// <summary>
    /// Tween's current value.
    /// </summary>

    public Vector3 value
    {
        get
        {
            return cachedTransform.anchoredPosition3D;
        }
        set
        {

            cachedTransform.anchoredPosition3D = value;
        }
    }

    void Awake() { }

    /// <summary>
    /// Tween the value.
    /// </summary>

    protected override void OnUpdate(float factor, bool isFinished) { value = from * (1f - factor) + to * factor; }

    /// <summary>
    /// Start the tweening operation.
    /// </summary>

    static public TweenAnchoredPosition Begin(GameObject go, float duration, Vector3 pos)
    {
        TweenAnchoredPosition comp = UITweener.Begin<TweenAnchoredPosition>(go, duration);
        comp.from = comp.value;
        comp.to = pos;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="duration"></param>
    /// <param name="formPos"></param>
    /// <param name="toPos"></param>
    /// <param name="worldSpace"></param>
    /// <returns></returns>
    static public TweenAnchoredPosition Begin(GameObject go, float duration, Vector3 formPos, Vector3 toPos)
    {
        TweenAnchoredPosition comp = UITweener.Begin<TweenAnchoredPosition>(go, duration);
        comp.from = formPos;
        comp.to = toPos;

        if (duration <= 0f)
        {
            comp.Sample(1f, true);
            comp.enabled = false;
        }
        return comp;
    }
}

