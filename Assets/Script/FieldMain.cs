using UnityEngine;
using System.Collections;

public class FieldMain : MonoBehaviour {

	public enum STEP {
		IDLE		= 0,
		SKIT_LOAD	,
		SKIT		,
		END			,
		MAX			,
	}
	public STEP m_eStep;
	public STEP m_eStepPre;

	public CtrlEasySkit m_csEasySkit;
	public bool m_bIsEnd;
	// Use this for initialization
	void Start () {
		m_eStep = STEP.SKIT_LOAD;
		m_eStepPre = STEP.MAX;
		m_bIsEnd = false;
		GameObject prefEasySkit = PrefabManager.Instance.PrefabLoadInstance ("EasySkit/PrefEasySkit");

		Debug.Log (prefEasySkit);
		GameObject adv = PrefabManager.Instance.MakeObject (prefEasySkit, null);
		m_csEasySkit = adv.GetComponent<CtrlEasySkit> ();

	}
	
	// Update is called once per frame
	void Update () {
		bool bInit = false;
		if (m_eStepPre != m_eStep) {
			m_eStepPre  = m_eStep;
			bInit = true;
		}

		switch (m_eStep) {
		case STEP.IDLE:
			break;
		case STEP.SKIT_LOAD:
			if (bInit) {
				m_csEasySkit.LoadData (
					Define.UtageScenarioRoot + "/app/story_game/sister/Boot.csv",
					Define.UtageResourcesRoot + "/app/story_game/Resources"
				);
			}
			if (m_csEasySkit.IsLoadEnd ()) {
				m_eStep = STEP.SKIT;
			}
			break;
		case STEP.SKIT:
			if (bInit) {
				string strScenarioLabel = "sister_01";
				m_csEasySkit.StartScenario (strScenarioLabel);
			}
			if (m_csEasySkit.IsStopScenario) {
				m_eStep = STEP.END;
			}
			break;
		case STEP.END:
			if (bInit) {
				m_bIsEnd = true;
			}
			break;
		default:
			break;
		}
		return;
	}


}
