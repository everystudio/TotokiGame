using UnityEngine;
using System.Collections;

namespace Utage
{

	/// <summary>
	/// スライダー（HPバーの表示などに使う）
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Utage/Lib/UI/Slider")]
	[RequireComponent(typeof(BoxCollider2D))]
	public class Slider : MonoBehaviour
	{
		/// <summary>
		/// スライダーの方向
		/// </summary>
		enum Direction
		{
			Horizontal,
			Vertical,
		}
		[SerializeField]
		Direction direction = Direction.Horizontal;

		/// <summary>
		/// バーのスプライト（注！　バーに使うスプライトは、pivotをLeftかBottomにすること）
		/// </summary>
		[SerializeField]
		Sprite2D bar;

		/// <summary>
		/// スライダーの値が変化したときのメッセージの送り先
		/// </summary>
		public GameObject Target
		{
			get { return target; }
			set { target = value; }
		}
		[SerializeField]
		GameObject target;

		/// <summary>
		/// スライダーの値が変化したときに送られるメッセージ
		/// </summary>
		public string FunctionName
		{
			get { return functionName; }
			set { functionName = value; }
		}
		[SerializeField]
		string functionName = "OnSliderChange";

		/// <summary>
		/// スライダーの値(0～1.0)
		/// </summary>
		public float SliderValue { get { return sliderValue; } set { SetSliderValue(value); } }
		[SerializeField]
		float sliderValue;


		/// <summary>
		/// バーの基本サイズ
		/// </summary>
		float DefaultBarWidth
		{
			get
			{
				InitDefautlBarSize();
				return defaultBarWidth;
			}
		}
		float defaultBarWidth;

		/// <summary>
		/// バーの基本高さ
		/// </summary>
		float DefaultBarHeight
		{
			get
			{
				InitDefautlBarSize();
				return defaultBarHeight;
			}
		}
		float defaultBarHeight;

		/// <summary>
		/// バーのサイズの基本サイズを初期化
		/// </summary>
		void InitDefautlBarSize() 
		{
			if (isInitedDefautlBarSize) return;
			if (bar==null) return;

			defaultBarWidth = bar.Width;
			defaultBarHeight = bar.Height;
			isInitedDefautlBarSize = true;
		}
		bool isInitedDefautlBarSize;

		/// <summary>
		/// ボックスコライダー
		/// </summary>
		public BoxCollider2D BoxCollider2D
		{
			get	{if (null == boxCollider2D)	boxCollider2D = GetComponent<BoxCollider2D>();return boxCollider2D;}
		}
		BoxCollider2D boxCollider2D;


		void OnDrag(TouchData2D touch)
		{
			SetSliderValue(CalcSliderValue(touch.TouchPoint));
		}

		void OnTouchUp(TouchData2D touch)
		{
		}

		void SetSliderValue(float v)
		{
			if (Mathf.Approximately(sliderValue, v))
			{
				//ほぼ同じ値
				return;
			}

			sliderValue = v;
			if (null != bar)
			{
				switch (direction)
				{
					case Direction.Horizontal:
						bar.Width = DefaultBarWidth * SliderValue;
						break;
					case Direction.Vertical:
					default:
						bar.Height = DefaultBarHeight * SliderValue;
						break;
				}
			}
			UtageToolKit.SafeSendMessage(sliderValue, Target, FunctionName, false);
		}


		float CalcSliderValue(Vector2 point)
		{
			float t = 0;
			float min = 0;
			float max = 0;
			Vector3 pos = this.transform.position;
			switch (direction)
			{
				case Direction.Horizontal:
					min = (pos.x - BoxCollider2D.offset.x) - BoxCollider2D.size.x / 2;
					max = (pos.x - BoxCollider2D.offset.x) + BoxCollider2D.size.x / 2;
					t = point.x;
					break;
				case Direction.Vertical:
				default:
					min = (pos.y - BoxCollider2D.offset.y) - BoxCollider2D.size.y / 2;
					max = (pos.y - BoxCollider2D.offset.y) + BoxCollider2D.size.y / 2;
					t = point.y;
					break;
			}
			return Mathf.InverseLerp(min, max, t);
		}
	}
}