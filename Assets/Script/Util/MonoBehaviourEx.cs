using UnityEngine;
using System.Collections;

public class MonoBehaviourEx : MonoBehaviour {

	private Transform m_myTransform;
	protected Transform myTransform{
		get {
			if (m_myTransform == null) {
				m_myTransform = transform;
			}
			return m_myTransform;
		}
	}

	protected void SetPos( GameObject _obj , float _fX , float _fY ){
		_obj.transform.localPosition = new Vector3( _fX , _fY , 0.0f );
		return;
	}

	/**
	 * 戻り値：TweenAlpha 何も引っかからなかったらnullが返る
	 * 
	 * _goObj  親になるGameObject
	 * _fTime  TweenAlphaで変化にかかる時間
	 * _fAlpha 終了時のAlpha
	 * */
	protected TweenAlpha TweenAlphaAll( GameObject _goObj , float _fTime , float _fAlpha ){
		TweenAlpha ta = null;
		UISprite[] arrSprite = _goObj.GetComponentsInChildren<UISprite> ();
		foreach( UISprite sprite in arrSprite ){
			ta = TweenAlpha.Begin (sprite.gameObject, _fTime, _fAlpha);
		}
		UILabel[] arrLabel = _goObj.GetComponentsInChildren<UILabel> ();
		foreach( UILabel label in arrLabel ){
			ta = TweenAlpha.Begin (label.gameObject, _fTime, _fAlpha);
		}

		return ta;
	}


}
