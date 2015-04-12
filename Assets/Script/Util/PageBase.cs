using UnityEngine;
using System.Collections;

public abstract class PageBase : MonoBehaviourEx
{
	protected GameObject m_goRoot;
	/*
	public PageBase( GameObject _goRoot = null ){
		Debug.Log ("SetPageBase Constract");
		m_goRoot = _goRoot;
		return;
	}

	public PageBase(){
		m_goRoot = this.gameObject;
	}
	*/
	/*
		外から呼ばれる
	*/
	public void Fadein ()
	{
		m_bIsFadein = false;
		m_bIsFadeout = false;
		m_bIsEnd = false;

		// すでに呼ばれてたら機能しません
		Initialize ();

		callFadein ();
		return;
	}

	public void Fadeout ()
	{
		callFadeout ();
	}

	protected virtual void init(){
		return;
	}

	private bool m_bInitialized = false;
	public void Initialize(){
		if (m_bInitialized == false) {
			m_goRoot = this.gameObject;
			init ();
			m_bInitialized = true;
		}
	}
	/*
		子供のクラスが実装する
	*/
	protected abstract void callFadein ();

	protected abstract void callFadeout ();

	private bool m_bIsFadein = false;

	public bool IsFadein {
		get{ return m_bIsFadein; }
	}
	protected void onFinishFadein(){
		m_bIsFadein = true;
	}

	private bool m_bIsFadeout = false;

	public bool IsFadeout {
		get{ return m_bIsFadeout; }
	}
	protected void onFinishFadeout(){
		m_bIsFadeout = true;
	}

	protected bool m_bIsEnd = false;
	public bool IsEnd {
		get{ return m_bIsEnd; }
	}

	private CtrlEasySkit m_csEasySkit = null;
	public CtrlEasySkit EasySkit {
		get { return m_csEasySkit; }
		set { m_csEasySkit = value; }
	}


}
