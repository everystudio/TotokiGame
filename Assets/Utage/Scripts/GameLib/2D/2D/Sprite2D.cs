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
	/// スプライト（階層構造の影響をうけるように拡張したもの）
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Utage/Lib/2D/Sprite")]
	[RequireComponent(typeof(SpriteRenderer))]
	public class Sprite2D : Node2D
	{
		/// <summary>
		/// スプライト
		/// </summary>
		public Sprite Sprite { get { return sprite; } set { sprite = value; RefreshSprite(); } }
		[SerializeField]
		Sprite sprite;

		/// <summary>
		/// サイズのタイプ
		/// </summary>

		public enum SpriteSizeType
		{
			/// <summary>デフォルト(テクスチャそのままのサイズ)</summary>
			Default,

			/// <summary>指定した値で</summary>
			Custom,

			/// <summary>横を画面の幅まで伸ばす</summary>
			StrechHolizon,

			/// <summary>縦を画面の高さまで伸ばす</summary>
			StrechVertical,

			/// <summary>画面全体の大きさまで伸ばす</summary>
			StrechBoth,
		};

		/// <summary>
		/// サイズのタイプ
		/// </summary>
		public SpriteSizeType SizeType { get { return sizeType; } set { sizeType = value; RefreshSprite(); } }
		[SerializeField]
		SpriteSizeType sizeType;


		/// <summary>
		/// テクスチャファイル
		/// </summary>
		public AssetFile TextureFile { get { return this.textureFile; } }
		AssetFile textureFile;


		/// <summary>
		/// 表示スプライトのサイズ
		/// </summary>
		public Vector2 Size { get { return new Vector2(Width, Height); } set { customSize = value; RefreshSprite(); } }

		[SerializeField]
		Vector2 customSize = Vector2.zero;
		
		/// <summary>
		/// 表示スプライトの幅
		/// </summary>
		public float Width
		{
			get
			{
				switch (SizeType)
				{
					case SpriteSizeType.Custom:
					case SpriteSizeType.StrechHolizon:
					case SpriteSizeType.StrechVertical:
					case SpriteSizeType.StrechBoth:
						return customSize.x;
					case SpriteSizeType.Default:
					default:
						return BaseSize.x;
				}
			}
			set
			{
				customSize.x = value;
				if (sizeType == SpriteSizeType.Default)
				{
					sizeType = SpriteSizeType.Custom;
					if (customSize.y == 0)
					{
						customSize.y = BaseSize.y;
					}
				}
				RefreshSprite();
			}
		}

		/// <summary>
		/// 表示スプライトの高さ
		/// </summary>
		public float Height
		{
			get
			{
				switch (SizeType)
				{
					case SpriteSizeType.Custom:
					case SpriteSizeType.StrechHolizon:
					case SpriteSizeType.StrechVertical:
					case SpriteSizeType.StrechBoth:
						return customSize.y;
					case SpriteSizeType.Default:
					default:
						return BaseSize.y;
				}
			}
			set
			{
				customSize.y = value;
				if (sizeType == SpriteSizeType.Default)
				{
					sizeType = SpriteSizeType.Custom;
					if (customSize.x == 0)
					{
						customSize.x = BaseSize.x;
					}
				}
				RefreshSprite();
			}
		}

		/// <summary>
		/// スプライトの基本のサイズ
		/// </summary>
		public Vector2 BaseSize { get { return baseSize; } }
		[SerializeField]
		Vector2 baseSize = new Vector2(-1, -1);

		/// <summary>
		/// スプライトの基本のサイズにかけるスケール
		/// </summary>
		public Vector2 BaseScale { get { return baseScale; } }
		[SerializeField]
		Vector2 baseScale = Vector2.one;

		/// <summary>
		/// 更新をかける
		/// </summary>
		public override void RefreshCustom()
		{
			RefreshSprite();
		}

		/// <summary>
		/// スプライトの更新
		/// </summary>
		protected void RefreshSprite()
		{
			CachedSpriteRenderer.sprite = Sprite;
			if (null != Sprite)
			{
				baseSize = new Vector2(sprite.rect.width * BaseScale.x, sprite.rect.height * BaseScale.y);
				CameraManager cam = CameraManager.GetInstance();
				if (cam != null)
				{
					float w = cam.CurrentWidth;
					float h = cam.CurrentHeight;
					switch (SizeType)
					{
						case SpriteSizeType.StrechHolizon:
							customSize.y = h;
							break;
						case SpriteSizeType.StrechVertical:
							customSize.x = w;
							break;
						case SpriteSizeType.StrechBoth:
							customSize = new Vector2(w, h);
							break;
					}
				}

				CachedTransform.localScale = new Vector3(Width / BaseSize.x, Height / BaseSize.y, 1); ;
			}
		}
		
		/// <summary>
		/// テクスチャファイルを設定
		/// </summary>
		/// <param name="file">テクスチャファイル</param>
		/// <param name="pixelsToUnits">スプライトを作成する際の、座標1.0単位辺りのピクセル数</param>
		public void SetTextureFile(AssetFile file, float pixelsToUnits)
		{
			this.textureFile = file;

			//直前のファイルがあればそれを削除
			ClearTextureFile();

			textureFile.AddReferenceComponet(this.gameObject);
			if (textureFile.IsLoadEnd)
			{
				SetTextureFileSprite(textureFile, pixelsToUnits);
			}
			else
			{
				StartCoroutine(CoWaitTextureFileLoading(pixelsToUnits));
			}
		}

		IEnumerator CoWaitTextureFileLoading(float pixelsToUnits)
		{
			while (!textureFile.IsLoadEnd) yield return 0;
			SetTextureFileSprite(textureFile, pixelsToUnits);
		}

		void SetTextureFileSprite(AssetFile file, float pixelsToUnits)
		{
			baseScale = Vector2.one * file.SpriteInfo.scale;
			Sprite = file.GetSprite(pixelsToUnits);
		}
	
		/// <summary>
		/// テクスチャファイルを設定
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <param name="pixelsToUnits">スプライトを作成する際の、座標1.0単位辺りのピクセル数</param>
		public void SetTextureFile(string path, float pixelsToUnits)
		{
			AssetFile file = AssetFileManager.Load(path, this);
			SetTextureFile(file,pixelsToUnits);
			file.Unuse(this);
		}

		/// <summary>
		/// テクスチャファイルをクリア
		/// </summary>
		public void ClearTextureFile()
		{
			AssetFileReference reference = this.GetComponent<AssetFileReference>();
			if (reference != null)
			{
				Destroy(reference);
			}
			Sprite = null;
		}

		/// <summary>
		/// 子オブジェクトを含めたスプライトサイズに合わせた2DBoxコライダーを追加する（既にコライダーがある場合はサイズを変える）
		/// </summary>
		/// <param name="go">コライダーを付与するGameObjecct</param>
		/// <returns>追加ずみの2DBoxコライダー</returns>
		static public BoxCollider2D AddCollider2D(GameObject go)
		{
			if (go != null)
			{
				BoxCollider2D col = go.GetComponent<BoxCollider2D>();
				if (col == null) col = go.AddComponent<BoxCollider2D>();
				ResizeCollider(col);
				return col;
			}
			return null;
		}

		/// <summary>
		/// 子オブジェクトを含めた全てのスプライトのBoundsでコライダーをリサイズ
		/// </summary>
		/// <param name="col">リサイズするコライダー</param>
		static public void ResizeCollider(BoxCollider2D col)
		{
			Bounds bounds = CalcSpritesBounds(col.gameObject);
			col.offset = bounds.center;
			col.size = bounds.size;
		}

		/// <summary>
		/// 全子オブジェクトを含めた全てのスプライトを囲むBoundsを求める
		/// </summary>
		/// <param name="go">ルートになるGameObjecct</param>
		/// <returns>全てのスプライトを囲むBounds</returns>
		static public Bounds CalcSpritesBounds(GameObject go)
		{
			Bounds worldBounds = new Bounds();
			bool isFirst = true;

			Sprite2D[] sprite2D = go.GetComponentsInChildren<Sprite2D>(true);
			foreach (Sprite2D sprite in sprite2D)
			{
				if (sprite.Sprite != null)
				{
					AddSpriteBunds(sprite.transform, sprite.Sprite, ref worldBounds, ref isFirst);
				}
			}
			SpriteRenderer[] sprites = go.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (SpriteRenderer sprite in sprites)
			{
				if (sprite.sprite != null)
				{
					AddSpriteBunds(sprite.transform, sprite.sprite, ref worldBounds, ref isFirst);
				}
			}

			Matrix4x4 localMat = go.transform.worldToLocalMatrix;
			Bounds localBounds = new Bounds();
			localBounds.Encapsulate(localMat.MultiplyPoint3x4(worldBounds.min));
			localBounds.Encapsulate(localMat.MultiplyPoint3x4(worldBounds.max));
			return localBounds;
		}
		static void AddSpriteBunds(Transform trans, Sprite sprite, ref Bounds bounds, ref bool isFirst)
		{
			if (isFirst)
			{
				bounds.SetMinMax(trans.TransformPoint(sprite.bounds.min), trans.TransformPoint(sprite.bounds.max));
				isFirst = false;
			}
			else
			{
				bounds.Encapsulate(trans.TransformPoint(sprite.bounds.min));
				bounds.Encapsulate(trans.TransformPoint(sprite.bounds.max));
			}
		}
	}
}
