using UnityEngine;
using System.Collections;

public class SpriteAnimationBase : MonoBehaviour {

	protected float m_fTime;			// 現在の時間
	public float m_fInterval;			// アニメーションを切り替える経過時間
	protected int m_iIndex;				// アニメーションの番号
	private int m_iIndexMax;			// アニメーションのコマ最大数
	public bool m_bIsLoop = true;		// ループするかどうか

	protected SpriteRenderer m_SpriteRenderer;
	public Sprite[] m_SpriteArr;

	public enum STEP
	{
		NONE		= 0,
		INIT		,
		ANIMATION	,
		END			,
		MAX			,
	}
	public STEP m_eStep;
	protected STEP m_eStepPre;

	public void Init( SpriteRenderer _SpriteRenderer ){
		m_SpriteRenderer = _SpriteRenderer;

		m_fTime = 0.0f;
		m_iIndex = 0;

		m_eStep = STEP.ANIMATION;
		m_eStepPre = STEP.MAX;
	}

	void Update () {

		bool bInit = false;
		if (m_eStepPre != m_eStep) {
			m_eStepPre  = m_eStep;
			bInit = true;
		}

		float fDelta = Time.deltaTime;

		switch (m_eStep) {
		case STEP.NONE:
			break;
		case STEP.ANIMATION:
			if (bInit) {
				m_fTime = 0.0f;
				m_iIndex = 0;

				m_iIndexMax = m_SpriteArr.Length;
				if (m_iIndexMax == 0) {
					Debug.LogError ("Zero SpriteArr!!");
				}
			}
			m_fTime += fDelta;
			int iAddIndex = (int)(m_fTime / m_fInterval);
			if (0 < iAddIndex) {
				m_fTime -= m_fInterval * iAddIndex;
				int iTempFrame = m_iIndex + iAddIndex;
				if (m_iIndexMax <= iTempFrame) {
					if (m_bIsLoop == false ) {
						iTempFrame = m_iIndexMax - 1;
						m_eStep = STEP.END;
					}
				}
				m_iIndex = iTempFrame % m_iIndexMax;
				m_SpriteRenderer.sprite = m_SpriteArr [m_iIndex];
			}
			break;

		case STEP.END:
			break;
		default:
			break;
		}
	
	}

	public void Disp(){
		m_SpriteRenderer.sprite = m_SpriteArr [m_iIndex];
	}
}
