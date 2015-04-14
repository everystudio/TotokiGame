using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimationManager))]
[RequireComponent(typeof(SpriteRenderer))]
public class CtrlAnimation : MonoBehaviour {

	private SpriteRenderer m_SpriteRenderer;
	private AnimationManager m_AnimationManager;

	// Use this for initialization
	void Start () {
		m_AnimationManager = GetComponent<AnimationManager> ();
		m_SpriteRenderer = GetComponent<SpriteRenderer> ();

		m_AnimationManager.Init (m_SpriteRenderer);
		m_AnimationManager.StartAnimation (0);
		return;
	}

	public void SetMotion( CtrlPlayerBase.STATE _eState ){
		m_AnimationManager.StartAnimation ((int)_eState);
	}
	



}








