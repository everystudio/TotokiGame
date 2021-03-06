using System;
using UnityEngine;
using System.Collections;

namespace Utage
{

	/// <summary>
	/// ボタン
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Utage/Lib/UI/Button")]
	[RequireComponent(typeof(BoxCollider2D))]
	[RequireComponent(typeof(UiEffect))]
	public class Button : MonoBehaviour
	{
		/// <summary>
		/// SEを鳴らすためのコールバック。
		/// 独自のサウンド処理をする場合はこれを設定。
		/// </summary>
		public static Action<AudioClip> CallbackPlaySE
		{
			get { return Button.callbackPlaySE; }
			set { Button.callbackPlaySE = value; }
		}
		static Action<AudioClip> callbackPlaySE;

		/// <summary>
		/// ボタンを押したときのメッセージの送り先
		/// </summary>
		public GameObject Target
		{
			get { return target; }
			set { target = value; }
		}
		[SerializeField]
		GameObject target;

		/// <summary>
		/// ボタンを押したときに送られるメッセージ
		/// </summary>
		public string FunctionName
		{
			get { return functionName; }
			set { functionName = value; }
		}
		[SerializeField]
		string functionName = "OnTap";

		/// <summary>
		/// ボタンのインデックス(基本は使わないが、ラジオボタンやリストビューなどで使うのを想定。その他SendMessageされたときのボタンの識別などに)
		/// </summary>
		public int Index
		{
			get { return index; }
			set { index = value; }
		}
		[SerializeField]
		int index;

		/// <summary>
		/// コライダーのサイズを表示スプライトの大きさに合わせて自動設定するか
		/// </summary>
		public bool IsAutoResizeCollider2D
		{
			get { return isAutoResizeCollider2D; }
			set
			{
				isAutoResizeCollider2D = value;
				if (IsAutoResizeCollider2D) ResizeCollider();
			}
		}
		[SerializeField]
		bool isAutoResizeCollider2D = true;


		/// <summary>
		/// ボタンを押したときに鳴らすSE
		/// </summary>
		public AudioClip Se
		{
			get { return se; }
			set { se = value; }
		}
		[SerializeField]
		AudioClip se;


		Transform cachedTransform;
		Transform CachedTransform { get { if (null == cachedTransform) cachedTransform = this.transform; return cachedTransform; } }

		/// <summary>
		/// ボックスコライダー
		/// </summary>
		public BoxCollider2D BoxCollider2D
		{
			get
			{
				if (null == boxCollider2D)
				{
					boxCollider2D = GetComponent<BoxCollider2D>();
				}
				return boxCollider2D;
			}
		}
		BoxCollider2D boxCollider2D;

		/// <summary>
		/// エフェクト
		/// </summary>
		public UiEffect UiEffect
		{
			get
			{
				if (null == uiEffect)
				{
					uiEffect = GetComponent<UiEffect>();
				}
				return uiEffect;
			}
		}
		UiEffect uiEffect;

		/// <summary>
		/// 起動時
		/// </summary>
		protected virtual void Awake()
		{
			if (Target == null) Target = gameObject;
			if (IsAutoResizeCollider2D) ResizeCollider();
		}

		/// <summary>
		/// コライダーのサイズを表示スプライトに合わせて設定する
		/// </summary>
		public void ResizeCollider()
		{
			Sprite2D.ResizeCollider(BoxCollider2D);
			BoxCollider2D.isTrigger = true;
		}

		/// <summary>
		/// クリック処理されたとき
		/// </summary>
		/// <param name="touch">タッチ入力データ</param>
		protected virtual void OnClick(TouchData2D touch)
		{
			if (Se)
			{
				///コールバックが登録されていればそれを使う
				if (CallbackPlaySE != null)
				{
					CallbackPlaySE(Se);
				}
				else
				{
					///Utage標準のサウンド再生を使う
					SoundManager.GetInstance().PlaySE(Se);
				}
			}
			UtageToolKit.SafeSendMessage(this, Target, FunctionName);
		}
	}
}