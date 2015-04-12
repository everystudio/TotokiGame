using UnityEngine;
using System.Collections;
using Utage;

public class CtrlEasySkit : MonoBehaviour {

	[SerializeField]
	private AdvEngine			m_csAdvEngine;

	[SerializeField]
	private AdvEngineStarter	m_csAdvEngineStarter;


	public void StartScenario( string _strLabel ){
		m_csAdvEngine.JumpScenario(_strLabel);
		return;
	}

	public void JumpScenario( string _strLabel ){
		m_csAdvEngine.JumpScenario(_strLabel);
		return;
	}

	public bool IsStopScenario {
		get{ return m_csAdvEngine.IsStopScenario;}
	}

	public void LoadData( string _strScenarioData , string _strResoureData , int _iVersion =-1){

		Debug.Log( _strScenarioData + "version="+_iVersion );


		m_csAdvEngine.BootFromCsv(_strScenarioData , _strResoureData , _iVersion );
	}

	public bool IsLoadEnd(){
		return !m_csAdvEngine.IsWaitBootLoading;
	}

}
