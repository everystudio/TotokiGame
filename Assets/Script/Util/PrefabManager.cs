using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabManager : MonoBehaviour {

	[System.Serializable]
	public class TPrefabPair {
		public string 		strPrefabName;
		public GameObject 	goPrefab;
	}

	static PrefabManager instance = null;
	public static PrefabManager Instance
    {
        get
        {
			if(instance == null)
			{
				GameObject obj = GameObject.Find("PrefabManager");
				if( obj == null )
				{
					obj = new GameObject("PrefabManager");
				}
				
				instance = obj.GetComponent<PrefabManager>();
				
				if(instance == null)
				{
					instance = obj.AddComponent<PrefabManager>() as PrefabManager;
				}
				
			}
			return instance;
		}
	}

	[SerializeField]
	private List<TPrefabPair> m_prefLoadedPrefabList = new List<TPrefabPair>();

	private bool getPrefab( string _strPrefabName , ref GameObject _goPrefab ){
		bool bRet = false;
		foreach( TPrefabPair data in m_prefLoadedPrefabList ){
			if( _strPrefabName == data.strPrefabName ){
				bRet = true;
				_goPrefab = data.goPrefab;
				break;
			}
		}
		return bRet;
	}

	public GameObject PrefabLoadInstance( string _strPrefabName ){
		GameObject goRet = null;
		if( getPrefab( _strPrefabName , ref goRet) ){
			return goRet;
		}
		goRet = Resources.Load( _strPrefabName , typeof(GameObject) ) as GameObject;

		TPrefabPair addData = new TPrefabPair();
		addData.strPrefabName = _strPrefabName;
		addData.goPrefab = goRet;

		m_prefLoadedPrefabList.Add( addData );

		return goRet;
	}

	public GameObject MakeObject( GameObject _goPrefab , GameObject _goParent ){

		Vector3 pos = Vector3.zero;
		Quaternion rot = new Quaternion();

		if( _goParent != null ){
			pos = _goParent.transform.localPosition;
			rot = _goParent.transform.rotation;
		}

		GameObject obj = Instantiate (_goPrefab, pos , rot ) as GameObject;

		if( _goParent != null ){
			obj.transform.parent = _goParent.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
		}
		return obj;

	}




}
