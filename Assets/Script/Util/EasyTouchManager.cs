using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EasyTouchManager : MonoBehaviourEx {

	/// <summary>
	/// シングルトンですが、設定はシーンごとに行うように注意してください
	/// </summary>
	static EasyTouchManager instance = null;
	public static EasyTouchManager Instance
	{
		get
		{
			if(instance == null)
			{
				GameObject obj = GameObject.Find("EasyTouchManager");
				if( obj == null )
				{
					obj = new GameObject("EasyTouchManager");
				}

				instance = obj.GetComponent<EasyTouchManager>();

				if(instance == null)
				{
					instance = obj.AddComponent<EasyTouchManager>() as EasyTouchManager;
				}

			}
			return instance;
		}
	}

	[SerializeField]
	private List<ETCJoystick> m_csETCJoystickList = new List<ETCJoystick>();

	[SerializeField]
	private List<ETCButton> m_csETCButtonList = new List<ETCButton>();


	void Update(){
		int iCount = 0;
		foreach( ETCJoystick joystick in m_csETCJoystickList ){
			//Debug.Log ( "joystick" + iCount + "x:" + joystick.axisX.axisValue +" y:"+joystick.axisY.axisValue );
			iCount += 1;
		}

		iCount = 0;
		foreach( ETCButton button in m_csETCButtonList ){
			if (ETCAxis.AxisState.None != button.axis.axisState) {
				Debug.Log ("button" + iCount + "x:" + button.axis.axisState);
			}
			iCount += 1;
		}
	}




}
