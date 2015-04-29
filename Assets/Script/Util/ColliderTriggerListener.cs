using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderTriggerListener : MonoBehaviour {

	public enum STEP {
		NONE		= 0,
		SEARCH		,
		END			,
		MAX			,
	};
	public STEP m_eStep;
	public STEP m_eStepPre;

	public int m_iSearchFrame;
	public int m_iCount;

	void Start(){
		m_eStep = STEP.NONE;
		m_eStepPre = STEP.MAX;
		return;
	}

	public void Search(){
		m_eStep = STEP.SEARCH;
		enabled = true;
		m_ColliderEnterList.Clear ();
		return;
	}

	List<Collider> m_ColliderEnterList = new List<Collider>();
	public List<Collider> ColliderEnterList {
		get { return m_ColliderEnterList; }
	}

	void OnTriggerEnter(Collider _collider ){
		m_ColliderEnterList.Add (_collider);
		Debug.Log ("OnTriggerEnter:" + _collider.gameObject);
		return;
	}

	void OnTriggerStay( Collider _collider ){
		//Debug.Log ("OnTriggerStay:"+_collider.gameObject);
	}

	void OnCollisionEnter(Collision collision) {
		//Debug.Log ("OnCollisionEnter:" + collision.gameObject);
	}

	void Update(){
		bool bInit = false;
		if (m_eStepPre != m_eStep) {
			m_eStepPre = m_eStep;
			bInit = true;
		}
		switch (m_eStep) {
		case STEP.NONE:
			m_eStep = STEP.END;
			break;
		case STEP.SEARCH:
			if (bInit) {
				m_iCount = 0;
			}
			if (m_iSearchFrame <= m_iCount) {
				m_eStep = STEP.END;
			}
			break;
		case STEP.END:
			if (bInit) {
				enabled = false;
			}
			break;
		}
		return;
	}

}



