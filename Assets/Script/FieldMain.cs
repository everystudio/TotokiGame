using UnityEngine;
using System.Collections;

public class FieldMain : MonoBehaviour {

	public enum STEP {
		INIT		= 0,
		IDLE		,
		SKIT_LOAD	,
		SKIT		,
		END			,
		MAX			,
	}
	public STEP m_eStep;
	public STEP m_eStepPre;

	public CtrlPlayerField m_csPlayerField;

	public CtrlEasySkit m_csEasySkit;
	public bool m_bIsEnd;
	// Use this for initialization
	void Start () {
		m_eStep = STEP.SKIT_LOAD;
		m_eStep = STEP.INIT;
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
		case STEP.INIT:
			m_eStep = STEP.IDLE;
			break;
		case STEP.IDLE:

			if(Input.GetMouseButtonDown(0)){
				RaycastHit hit;
				//カメラからみたマウス位置のレイ
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				//レイを投射してオブジェクトを検出
				if(Physics.Raycast(ray,out hit)){
//				print(hit.collider.gameObject.name);
					Debug.Log ( hit.collider.gameObject.name +":"+ hit.point);

					m_csPlayerField.Move (hit.point);
				}
			}

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
