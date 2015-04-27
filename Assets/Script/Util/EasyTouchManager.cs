using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class TEasyTouchJoystick{
	public string strName;
	public ETCJoystick csJoyStick;
}


[System.Serializable]
public class TEasyTouchButton{
	public string strName;
	public ETCButton csButton;
}

public class EasyTouchManager : MonoBehaviourEx {

	public ETCJoystick GetJoystick( string _strName , ref Vector2 _v2Value ){
		foreach (TEasyTouchJoystick data in m_JoystickList) {
			if (data.strName.Equals (_strName) == true) {
				_v2Value.x = data.csJoyStick.axisX.axisValue;
				_v2Value.y = data.csJoyStick.axisY.axisValue;
				return data.csJoyStick;
			}
		}
		_v2Value = Vector2.zero;
		return null;
	}
	public ETCJoystick GetJoystick( string _strName , ref Vector3 _v3Value ){
		Vector2 temp = new Vector2 (0.0f , 0.0f);

		ETCJoystick ret = GetJoystick (_strName, ref temp);

		_v3Value.x = temp.x;
		_v3Value.y = temp.y;

		return ret;
	}

	public ETCAxis.AxisState GetButton( string _strName ){

		foreach (TEasyTouchButton button in m_ButtonList) {
			if (button.strName.Equals (_strName) == true) {
				return button.csButton.axis.axisState;
			}
		}
		return ETCAxis.AxisState.None;
	}

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
	private List<TEasyTouchJoystick> m_JoystickList = new List<TEasyTouchJoystick> ();

	[SerializeField]
	private List<TEasyTouchButton> m_ButtonList = new List<TEasyTouchButton>();


	void Update(){
		int iCount = 0;
		foreach( TEasyTouchJoystick joystick in m_JoystickList ){
			//Debug.Log ( "joystick" + iCount + "x:" + joystick.axisX.axisValue +" y:"+joystick.axisY.axisValue );
			iCount += 1;
		}

		iCount = 0;
		foreach( TEasyTouchButton button in m_ButtonList ){
			if (ETCAxis.AxisState.None != button.csButton.axis.axisState) {
				Debug.Log ("button" + iCount + "x:" + button.csButton.axis.axisState);
			}
			iCount += 1;
		}
	}




}
