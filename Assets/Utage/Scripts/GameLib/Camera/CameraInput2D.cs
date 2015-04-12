//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Utage
{

	/// <summary>
	/// 2D用の入力制御処理
	/// </summary>
	[AddComponentMenu("Utage/Lib/Camera/CameraInput")]
	[ExecuteInEditMode]
	public class CameraInput2D : MonoBehaviour
	{
		[SerializeField]
		bool multiTouchEnabled = false;
		[SerializeField]
		bool isEnableMouseButtonLeft = true;
		[SerializeField]
		bool isEnableMouseButtonRight = false;
		[SerializeField]
		bool isEnableMouseButtonCenter = false;

		Camera cachedCamera;
		TouchData2D[] touchesMouse;
		List<TouchData2D> touches = new List<TouchData2D>();

		void Start()
		{
			cachedCamera = this.GetComponent<Camera>();
			touchesMouse = new TouchData2D[3];
			for (int i = 0; i < 3; ++i)
			{
				touchesMouse[i] = new TouchData2D(this);
			}
		}
		void Update()
		{
			MouseOperation();
			TouchOperation();
		}

		//マウスの入力処理
		void MouseOperation()
		{
			if( UtageToolKit.IsPlatformStandAloneOrEditor() || Application.isWebPlayer )
			{
				if (null == touchesMouse) return;
				if (!Input.mousePresent) return;

				Vector3 point = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
				//有効なボタンだけ処理する
				if (isEnableMouseButtonLeft) MouseOperation(0, point);
				if (isEnableMouseButtonRight) MouseOperation(1, point);
				if (isEnableMouseButtonCenter) MouseOperation(2, point);
				MouseMove();
			}
		}

		//マウスの入力処理
		void MouseOperation(int mouseID, Vector3 point)
		{
			touchesMouse[mouseID].UpdatePoint(Input.GetMouseButton(mouseID), Input.GetMouseButtonDown(mouseID), point.x, point.y, cachedCamera.cullingMask);
		}
		//マウスの移動のみの処理
		void MouseMove()
		{
		}

		//タッチの入力処理
		void TouchOperation()
		{
			if (null == touches) return;

			int max = Input.touchCount;
			if (!multiTouchEnabled || !Input.multiTouchEnabled)
			{
				max = Mathf.Min(1, max);
			}

			//有効なボタンだけ処理する
			for (int i = 0; i < max; ++i)
			{
				TouchOperation(i);
			}
		}
		//タッチの入力処理
		void TouchOperation(int id)
		{
			Touch touch = Input.touches[id];
			Vector3 point = cachedCamera.ScreenToWorldPoint(touch.position);

			bool isPressed = false;
			bool isTrig = false;
			switch (touch.phase)
			{
				case TouchPhase.Began:
					isPressed = true;
					isTrig = true;
					break;
				case TouchPhase.Moved:
				case TouchPhase.Stationary:
					isPressed = true;
					break;
				case TouchPhase.Canceled:
				case TouchPhase.Ended:
					break;
			}

			while(id >= touches.Count)
			{
				touches.Add( new TouchData2D(this) );
			}

			touches[id].UpdatePoint(isPressed, isTrig, point.x, point.y, cachedCamera.cullingMask);
		}
	}
}