//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2016 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Tween the object's alpha. Works with both UI widgets as well as renderers.
/// </summary>

[AddComponentMenu("Tween/Tween Alpha")]
public class TweenAlpha : UITweener
{
	[Range(0f, 1f)] public float from = 1f;
	[Range(0f, 1f)] public float to = 1f;

	bool mCached = false;
    CanvasGroup mCanvasGroup;
    Graphic mGraphic;
	
	void Cache ()
	{
		mCached = true;

        mCanvasGroup = GetComponent<CanvasGroup>();

        if (mCanvasGroup == null)
        {
            mGraphic = transform.GetComponent<Graphic>();
            if (mGraphic == null)
            {
                mCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
	}

	/// <summary>
	/// Tween's current value.
	/// </summary>

	public float value
    {
        get
        {
            if (!mCached) Cache();

            if (mCanvasGroup)
                return mCanvasGroup.alpha;
            else
                return mGraphic.color.a;

        }
        set
        {
            if (!mCached) Cache();
            if (mCanvasGroup)
            {
                mCanvasGroup.alpha = value;
            }
            else
            {
                var color = mGraphic.color;
                color.a = value;
                mGraphic.color = color;
            }
        }
    }
	/// <summary>
	/// Tween the value.
	/// </summary>

	protected override void OnUpdate (float factor, bool isFinished) { value = Mathf.Lerp(from, to, factor); }

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenAlpha Begin (GameObject go, float duration, float alpha)
	{
		TweenAlpha comp = UITweener.Begin<TweenAlpha>(go, duration);
		comp.from = comp.value;
		comp.to = alpha;

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	public override void SetStartToCurrentValue () { from = value; }
	public override void SetEndToCurrentValue () { to = value; }
}
