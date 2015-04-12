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
	/// リストビュー管理コンポーネント
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/ListView")]
	public class ListView : MonoBehaviour
	{

		//リストビューの表示するアイテムのプレハブ
		public ListViewItem ItemPrefab
		{
			get { return itemPrefab; }
			set { itemPrefab = value; }
		}
		[SerializeField]
		ListViewItem itemPrefab;

		//アイテムのルート
		[SerializeField]
		Transform rootItems;

		//リストビューの有効幅
		public float ClipWidthPx
		{
			get { return clipWidthPx; }
			set { clipWidthPx = value; }
		}
		[SerializeField]
		float clipWidthPx;


		//リストビューの有効高さ
		public float ClipHeightPx
		{
			get { return clipHeightPx; }
			set { clipHeightPx = value; }
		}
		[SerializeField]
		float clipHeightPx;

		//座標1.0単位辺りのピクセル数
		public int PixcelsToUnits
		{
			get { return pixcelsToUnits; }
			set { pixcelsToUnits = value; }
		}
		[SerializeField]
		int pixcelsToUnits = 100;

		//位置を整列させる場合に、初期表示位置を逆（最下段や右端）にするか
		public bool IsRepositionReverse
		{
			get { return isRepositionReverse; }
			set { isRepositionReverse = value; }
		}
		[SerializeField]
		bool isRepositionReverse = false;

		//表示範囲外にインデックスの小さいアイテム(右や上側のアイテム)があるのを知らせる表示オブジェクト
		public GameObject MinArrow
		{
			get { return minArrow; }
			set { minArrow = value; }
		}
		[SerializeField]
		GameObject minArrow;


		//表示範囲外にインデックスの大きいアイテム(左や下側のアイテム)があるのを知らせる表示オブジェクト
		public GameObject MaxArrow
		{
			get { return maxArrow; }
			set { maxArrow = value; }
		}
		[SerializeField]
		GameObject maxArrow;

		//全てのアイテムが表示範囲内にある場合、スクロール処理を向こうにするか
		public bool EgonreScrollWithAllItemInClip
		{
			get { return egonreScrollWithAllItemInClip; }
			set { egonreScrollWithAllItemInClip = value; }
		}
		[SerializeField]
		bool egonreScrollWithAllItemInClip = true;

		//スクロールが範囲外にきた時に、跳ね返る演出にかける時間
		public float TimeMoveReflection
		{
			get { return timeMoveReflection; }
			set { timeMoveReflection = value; }
		}
		[SerializeField]
		float timeMoveReflection = 0.2f;
		
		//スクロールのタッチを離したときに、慣性で動き続ける演出の時間
		public float TimeInertia
		{
			get { return timeInertia; }
			set { timeInertia = value; }
		}
		[SerializeField]
		float timeInertia = 1.5f;

		/// <summary>
		/// インデックスの小さいアイテム(右や上側のアイテム)が、境界線の外側にあるか
		/// </summary>
		public bool IsOuterMin { get { return isOuterMin; } }
		bool isOuterMin;

		/// <summary>
		/// インデックスの大きいアイテム(左や下側のアイテム)が、境界線の外側にあるか
		/// </summary>
		public bool IsOuterMax { get { return isOuterMax; } }
		bool isOuterMax;

		/// <summary>
		/// インデックスの小さいアイテム(右や上側のアイテム)の端、境界線の内側か
		/// </summary>
		public bool IsInnerMin { get { return isInnerMin; } }
		bool isInnerMin;

		/// <summary>
		/// インデックスの大きいアイテム(左や下側のアイテム)の端、境界線の内側か
		/// </summary>
		public bool IsInnerMax { get { return isInnerMax; } }
		bool isInnerMax;

		/// <summary>
		/// インデックスの小さいアイテム(右や上側のアイテム)の端が、境界線の内側にある程度食い込んでいるか
		/// </summary>
		public bool IsInnerMargineMin { get { return isInnerMargineMin; } }
		bool isInnerMargineMin;

		/// <summary>
		/// インデックスの大きいアイテム(左や下側のアイテム)の端が、境界線の内側にある程度食い込んでいるか
		/// </summary>
		bool IsInnerMargineMax { get { return isInnerMargineMax; } }
		bool isInnerMargineMax;

		int itemNum;	//アイテム数
		System.Action<GameObject, int> CallbackCreateItem;	//アイテムが作成されたときのコールバック

		Vector4 itemsRect;		//アイテム前回の矩形
		Vector2 itemSize;		//アイテム一つのサイズ
		bool isAllItemInClip;	//アイテムがすべてクリップの範囲内か
		float vel;				//移動速度

		//タイプ
		public enum LitViewType
		{
			Holizon,	//横に並べる
			Vertical,	//縦に並べる
		};
		public LitViewType Type
		{
			get { return type; }
			set { type = value; }
		}
		[SerializeField]
		LitViewType type;


		Transform cachedTransform;
		Transform CachedTransform { get { if (null == cachedTransform) cachedTransform = this.transform; return cachedTransform; } }

		//クリップのサイズ(座標系単位)
		float ClipWidth { get { return ClipWidthPx / PixcelsToUnits; } }
		float ClipHeight { get { return ClipHeightPx / PixcelsToUnits; } }

		//アイテム矩形のサイズ(座標系単位)
		float ItemsWidth { get { return itemsRect.z - itemsRect.x; } }
		float ItemsHeight { get { return itemsRect.y - itemsRect.w; } }

		//クリッピングの座標
		float ClipLeft { get { return CachedTransform.position.x - ClipWidth / 2; } }
		float ClipRight { get { return CachedTransform.position.x + ClipWidth / 2; } }
		float ClipTop { get { return CachedTransform.position.y + ClipHeight / 2; } }
		float ClipBottom { get { return CachedTransform.position.y - ClipHeight / 2; } }

		//クリッピング座標から、アイテム全体矩形の距離
		float DistLeft { get { return itemsRect.x - ClipLeft; } }
		float DistRight { get { return itemsRect.z - ClipRight; } }
		float DistTop { get { return itemsRect.y - ClipTop; } }
		float DistBottom { get { return itemsRect.w - ClipBottom; } }

		void Awake()
		{
			cachedTransform = this.transform;
			if (rootItems == this.transform)
			{
				Debug.LogError("rootItems can't set self GameObject");
				rootItems = null;
			}
			if (rootItems == null)
			{
				rootItems = UtageToolKit.AddChildGameObject(this.transform, "rootItems").transform;
			}
			InitItmeSize();
		}

		/// <summary>
		/// 開く
		/// </summary>
		/// <param name="itemNum">アイテムの数</param>
		/// <param name="callbackCreateItem">アイテムが作成されるときに呼ばれるコールバック</param>
		public void Open(int itemNum, System.Action<GameObject, int> callbackCreateItem)
		{
			this.itemNum = itemNum;
			this.CallbackCreateItem = callbackCreateItem;
			CreateItems();
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public void Close()
		{
			ClearItems();
		}

		/// <summary>
		/// 各アイテムの位置を初期化
		/// </summary>
		public void Reposition()
		{
			InitItemsRect();
			float move = -GetRepositionLength(!IsRepositionReverse);
			ScrollSub(move);

			isAllItemInClip = (Type == LitViewType.Holizon) ? (ItemsWidth <= ClipWidth) : (ItemsHeight <= ClipHeight);
		}

		//アイテムを作成
		void CreateItems()
		{
			ClearItems();
			Vector3 offset = Vector3.zero;
			for (int i = 0; i < itemNum; ++i)
			{
				GameObject go = UtageToolKit.AddChildPrefab(rootItems.transform, ItemPrefab.gameObject, offset);
				switch (Type)
				{
					case LitViewType.Vertical:
						offset.y -= (go.GetComponent<Collider2D>() as BoxCollider2D).size.y;
						break;
					case LitViewType.Holizon:
					default:
						offset.x += (go.GetComponent<Collider2D>() as BoxCollider2D).size.x;
						break;
				}
				CreateItem(go, i);
			}
			Reposition();
		}

		//アイテムの作成
		void CreateItem(GameObject go, int index)
		{
			ListViewItem button = go.GetComponent<ListViewItem>();
			button.InitListItem(this, this.gameObject, index);
			if (null != CallbackCreateItem) CallbackCreateItem(go, index);
		}
		//アイテムを全消去
		void ClearItems()
		{
			if( rootItems != null ){
				UtageToolKit.DestroyChildren(rootItems.transform);
			}
		}

		//アイテムのサイズを取得
		void InitItmeSize()
		{
			foreach (Transform trans in rootItems.transform)
			{
				BoxCollider2D col = trans.gameObject.GetComponent<Collider2D>() as BoxCollider2D;
				itemSize = col.size;
				break;
			}
		}

		//アイテム全体の範囲を取得
		void InitItemsRect()
		{
			if (rootItems.childCount <= 0)
			{
				itemsRect = new Vector4(0, 0, 0, 0);
			}
			else
			{
				float left = float.MaxValue;
				float top = float.MinValue;
				float right = float.MinValue;
				float bottom = float.MaxValue;
				foreach (Transform trans in rootItems.transform)
				{
					BoxCollider2D col = trans.gameObject.GetComponent<Collider2D>() as BoxCollider2D;
					left = Mathf.Min(left, trans.position.x + col.offset.x - col.size.x / 2);
					top = Mathf.Max(top, trans.position.y + col.offset.y + col.size.y / 2);
					right = Mathf.Max(right, trans.position.x + col.offset.x + col.size.x / 2);
					bottom = Mathf.Min(bottom, trans.position.y + col.offset.y - col.size.y / 2);
				}
				itemsRect = new Vector4(left, top, right, bottom);
			}
		}


		/// <summary>
		/// スクロールが有効か
		/// </summary>
		/// <returns></returns>
		public bool IsScrollEnable()
		{
			return !(EgonreScrollWithAllItemInClip && isAllItemInClip);
		}

		/// <summary>
		/// リストビューの移動（スクロール）
		/// リストビューアイテムから使うので、普通は使わないこと
		/// </summary>
		/// <param name="move">移動ぶんの距離</param>
		/// <returns>スクロールしたか。スクロール範囲外になった場合はflaseを返す</returns>
		public bool Scroll(Vector2 move)
		{
			if (!IsScrollEnable()) return false;

			switch (Type)
			{
				case LitViewType.Holizon:
					return Scroll(move.x);
				case LitViewType.Vertical:
				default:
					return Scroll(move.y);
			}
		}
		//リストビューの移動（スクロール）
		bool Scroll(float move)
		{
			StopAllCoroutines();
			ScrollSub(move);
			bool isBreak = (IsInnerMargineMin || IsInnerMargineMax);
			return !isBreak;
		}

		//リストビューの移動（スクロール）
		void ScrollSub(float move)
		{
			switch (Type)
			{
				case LitViewType.Holizon:
					rootItems.transform.Translate(new Vector3(move, 0, 0));
					itemsRect.x += move;
					itemsRect.z += move;
					break;
				case LitViewType.Vertical:
					rootItems.transform.Translate(new Vector3(0, move, 0));
					itemsRect.y += move;
					itemsRect.w += move;
					break;
			}
			vel = (Time.deltaTime == 0) ? 0 : move / Time.deltaTime;
			CheckClip();
		}

		//アイテムの両端と境界線の位置関係をチェック
		void CheckClip()
		{
			float margineOuter = 0.25f;	//アイテムの両端と、境界線の外側の許容マージン
			float margineInner = 1.0f;	//アイテムの両端と、境界線の内側の許容マージン
			float margine;
			switch (Type)
			{
				case LitViewType.Holizon:
					margine = margineOuter * itemSize.x;
					isOuterMin = (DistLeft < -margine);
					isOuterMax = (margine < DistRight);

					isInnerMin = (DistLeft > 0);
					isInnerMax = (DistLeft < 0 && DistRight < 0);

					margine = margineInner * itemSize.x;
					isInnerMargineMin = (margine < DistLeft) && (0 < DistRight);
					isInnerMargineMax = (DistRight < -margine);
					break;
				case LitViewType.Vertical:
					margine = margineOuter * itemSize.y;
					isOuterMin = (margine < DistTop);
					isOuterMax = (DistBottom < -margine);

					isInnerMin = (DistTop < 0);
					isInnerMax = (DistTop > 0 && DistBottom > 0);

					margine = margineInner * itemSize.y;
					isInnerMargineMin = (DistTop < -margine) && (DistBottom < 0);
					isInnerMargineMax = (margine < DistBottom);
					break;
			}

			//アロー表示をON・OFFする
			if (null != MinArrow) MinArrow.SetActive(isOuterMin);
			if (null != MaxArrow) MaxArrow.SetActive(isOuterMax);
		}

		/// <summary>
		/// スクロールを終了（慣性でのスクロール開始）
		/// リストビューアイテムから使うので、普通は使わないこと
		/// </summary>
		public void ScrollEnd()
		{
			if (EgonreScrollWithAllItemInClip && isAllItemInClip)
			{
				return;
			}

			InitItemsRect();
			CheckClip();
			//範囲をはみ出ていて、逆方向に引っ張られるか
			bool isReflection = MoveReflection();
			if (!isReflection)
			{
				//慣性で進む
				StartCoroutine(CoScrollEnd());
			}
		}
		//スクロール終了後、慣性で動く
		IEnumerator CoScrollEnd()
		{
			yield return StartCoroutine(CoMoveInertia(vel));
			MoveReflection();
		}

		//範囲をはみ出してるなら、逆方向に引っ張る
		bool MoveReflection()
		{
			float move = 0;
			if (IsInnerMin || IsInnerMax)
			{
				move = -GetRepositionLength(IsInnerMin);
			}
			if (Mathf.Abs(move) > 0.0001f)
			{
				StartCoroutine(CoMoveSin(move, TimeMoveReflection));
				return true;
			}
			else
			{
				return false;
			}
		}

		//位置の初期化のための距離を取得
		float GetRepositionLength(bool isMin)
		{
			switch (Type)
			{
				case LitViewType.Holizon:
					return (isMin) ? DistLeft : Mathf.Max(DistLeft, DistRight);
				case LitViewType.Vertical:
				default:
					return (isMin) ? DistTop : Mathf.Min(DistTop, DistBottom);
			}
		}


		//サインカーブで一定距離を指定時間移動する
		IEnumerator CoMoveSin(float move, float time)
		{
			float elapsedTime = 0;	//経過時間
			float moved = 0;		//移動量

			bool isEnd = false;
			while (true)
			{
				elapsedTime += Time.deltaTime;
				if (elapsedTime >= time)
				{
					elapsedTime = time;
					isEnd = true;
				}
				float delta = Mathf.Sin((elapsedTime / time) * Mathf.PI / 2);
				delta = delta * move - moved;
				ScrollSub(delta);
				moved += delta;
				if (isEnd) break;
				yield return 0;
			}
		}

		//慣性で移動する(だんだん減速する)
		IEnumerator CoMoveInertia(float vel)
		{
			float time = TimeInertia <= 0 ? 1 : TimeInertia;
			float vel0 = vel;
			float accel = -vel / time;	//減速のための加速度
			while (true)
			{
				float delta = vel * Time.deltaTime;
				ScrollSub(delta);
				if ((Mathf.Abs(vel) < 0.0001f) || (vel0 * vel <= 0) || IsInnerMin || IsInnerMax)
				{
					break;
				}
				vel += Time.deltaTime * accel;
				yield return 0;
			}
		}
	}
}