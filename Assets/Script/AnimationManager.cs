using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour {

	public GameObject[] m_prefSpriteAnimationBaseArr;
	public SpriteAnimationBase m_AnimationBase;
	private int m_iAnimationMax;

	public int m_iAnimationIndex;
	public int m_iAnimationIndexPre;
	private SpriteRenderer m_SpriteRenderer;

	/// <summary>
	/// Sets the sprite renderer.
	/// </summary>
	/// <param name="_spriteRenderer">_sprite renderer.</param>
	public void Init( SpriteRenderer _spriteRenderer ){
		m_iAnimationMax = m_prefSpriteAnimationBaseArr.Length;
		m_iAnimationIndex = 0;
		m_iAnimationIndexPre = 0;
		m_SpriteRenderer = _spriteRenderer;
		return;
	}

	public bool StartAnimation( int _iIndex ){
		if (m_iAnimationMax <= _iIndex) {
			return false;
		}

		if (m_AnimationBase != null) {
			Destroy (m_AnimationBase.gameObject);
		}

		GameObject obj = PrefabManager.Instance.MakeObject (m_prefSpriteAnimationBaseArr [_iIndex], gameObject);
		m_AnimationBase = obj.GetComponent<SpriteAnimationBase> ();
		m_AnimationBase.Init (m_SpriteRenderer);

		m_iAnimationIndex = _iIndex;

		return true;
	}


}






