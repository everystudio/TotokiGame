using UnityEngine;
using System.Collections;

public class ButtonBase : MonoBehaviourEx {

	[SerializeField]
	private bool m_bPushed;
	public bool Pushed{
		get{
			bool bRet = m_bPushed;
			m_bPushed = false;
			return bRet;
		}
	}

	public void Init( int _iIndex ){
		m_iIndex = _iIndex;
		m_bPushed = false;
	}

	public void ClearTrigger(){
		m_bPushed = false;
	}

	[SerializeField]
	int m_iIndex;
	public int Index{
		get{ return m_iIndex; }
		set{ m_iIndex = value; }
	}

	virtual public void OnClick(){
		//Debug.Log( "Pushed OnClick");
		m_bPushed = true;
	}

}
