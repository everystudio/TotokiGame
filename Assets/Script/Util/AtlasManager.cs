using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AtlasManager : MonoBehaviourEx {

	static AtlasManager instance = null;
	public static AtlasManager Instance
	{
		get
		{
			if(instance == null)
			{
				GameObject obj = GameObject.Find("AtlasManager");
				if( obj == null )
				{
					obj = new GameObject("AtlasManager");
				}

				instance = obj.GetComponent<AtlasManager>();

				if(instance == null)
				{
					instance = obj.AddComponent<AtlasManager>() as AtlasManager;
					instance.Initialize ();
				}

			}
			return instance;
		}
	}

	public List<UIAtlas> m_lstAtlas = new List<UIAtlas>();

	private void Initialize(){



		m_lstAtlas.Clear ();
		return;
	}


	public UIAtlas GetAtlas( string _strSpriteName ){
		foreach (UIAtlas atlas in m_lstAtlas) {
			if (atlas.GetSprite (_strSpriteName) != null) {
				return atlas;
			}
		}
		return null;
	}

}
