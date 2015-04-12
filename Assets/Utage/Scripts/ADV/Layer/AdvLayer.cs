//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// レイヤー
	/// Unityで定義されているレイヤーではなく、あくまでUtage内でのレイヤー情報
	/// 具体的には、描画の中心位置と描画順のデータをもつ描画グループ
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/Layer")]
	public class AdvLayer : Node2D
	{
		/// <summary>
		/// レイヤーデータ
		/// </summary>
		public AdvLayerSettingData LayerData { get { return this.layerData; } }
		AdvLayerSettingData layerData;

		/// <summary>
		/// スプライトを作成する際の、座標1.0単位辺りのピクセル数
		/// </summary>
		float pixelsToUnits;

		/// <summary>
		/// 全スプライト
		/// </summary>
		List<AdvFadeSprites> sprites = new List<AdvFadeSprites>();

		/// <summary>
		/// デフォルトのスプライト
		/// </summary>
		public AdvFadeSprites DefaultSprite
		{
			get
			{
				return defaultSprite;
			}
			set
			{
				defaultSprite = value;
			}
		}
		AdvFadeSprites defaultSprite;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="layerData">レイヤーデータ</param>
		/// <param name="pixelsToUnits">スプライトを作成する際の、座標1.0単位辺りのピクセル数</param>
		public void Init(AdvLayerSettingData layerData, float pixelsToUnits)
		{
			this.layerData = layerData;
			this.pixelsToUnits = pixelsToUnits;
			ResetLayer();
		}
		//レイヤー情報から初期状態に
		void ResetLayer()
		{
			this.LocalOrderInLayer = LayerData.Order;
			this.transform.localPosition = new Vector3(LayerData.Center.x / pixelsToUnits, LayerData.Center.y / pixelsToUnits, -1.0f*LayerData.Order / 1000);
			if (!string.IsNullOrEmpty(LayerData.LayerMask))
			{
				this.gameObject.layer = LayerMask.NameToLayer(LayerData.LayerMask);
			}
		}

		/// <summary>
		/// 全て消去
		/// </summary>
		public void Clear()
		{
			ResetLayer();
			sprites.Clear();
			UtageToolKit.DestroyChildren(this.transform);
			DefaultSprite = null;
		}

		/// <summary>
		/// デフォルトのスプライトのテクスチャ設定
		/// </summary>
		/// <param name="name">スプライト名</param>
		/// <param name="texture">テクスチャファイル</param>
		/// <param name="fadeTime">フェード時間</param>
		public void SetDefaultSprite(string name, AssetFile texture, object x, object y, float fadeTime)
		{
			if (DefaultSprite == null)
			{
				//まだデフォルトスプライトがない場合はスプライトを作成して設定
				DefaultSprite = SetSprite(texture, name, x, y, fadeTime);
			}
			else
			{
				//デフォルトスプライトがある場合は既存のスプライトを使いまわして名前を変える
				SetSprite(texture, DefaultSprite.name, x, y, fadeTime);
				DefaultSprite.name = name;
			}
		}

		/// <summary>
		/// デフォルトのスプライトをフェードアウト
		/// </summary>
		/// <param name="fadeTime">フェード時間</param>
		public void FadeOutDefaultSprite(float fadeTime)
		{
			if (DefaultSprite != null)
			{
				FadeOutSprite(DefaultSprite.name, fadeTime);
			}
		}


		/// <summary>
		/// スプライトの設定
		/// </summary>
		/// <param name="texture">テクスチャファイル</param>
		/// <param name="spriteName">スプライト名</param>
		/// <param name="x">表示座標X floatのobject　nullを指定することで位置移動なし</param>
		/// <param name="y">表示座標Y floatのobject　nullを指定することで位置移動なし</param>
		/// <param name="fadeTime">フェード時間</param>
		/// <returns>設定したスプライト</returns>
		public AdvFadeSprites SetSprite(AssetFile texture, string spriteName, object x, object y, float fadeTime)
		{
			AdvFadeSprites sprite = GetSpriteCreateIfMissing(spriteName);
			sprite.SetTexture(texture, fadeTime);
			if (x != null || y != null)
			{
				Vector3 pos = sprite.transform.localPosition;
				if (x != null && x is float) pos.x = (float)x / pixelsToUnits;
				if (y != null && y is float) pos.y = (float)y / pixelsToUnits;
				sprite.transform.localPosition = pos;
			}
			return sprite;
		}

		/// <summary>
		/// スプライトのフェードアウト
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <param name="fadeTime">フェード時間</param>
		public void FadeOutSprite(string spriteName, float fadeTime)
		{
			var sprite = FindSprite(spriteName);
			if (sprite == null) return;

			if (defaultSprite == sprite)
			{
				defaultSprite = null;
			}
			sprite.FadeOut(fadeTime, true);
			sprites.Remove(sprite);
		}

		/// <summary>
		/// 全スプライトをフェードアウト
		/// </summary>
		/// <param name="fadeTime">フェード時間</param>
		public void FadeOutAllSprite(float fadeTime)
		{
			foreach (var sprite in sprites)
			{
				sprite.FadeOut(fadeTime, true);
			}
			sprites.Clear();
			defaultSprite = null;
		}


		//スプライトを名前から検索
		public AdvFadeSprites FindSprite(string spriteName)
		{
			foreach (var sprite in sprites)
			{
				if (sprite.name == spriteName)
				{
					return sprite;
				}
			}
			return null;
		}
		
		//スプライトを名前から検索（なければ作成）
		AdvFadeSprites GetSpriteCreateIfMissing(string spriteName)
		{
			var sprite = FindSprite(spriteName);
			if (sprite == null)
			{
				sprite = UtageToolKit.AddChildGameObjectComponent<AdvFadeSprites>(this.transform, spriteName);
				sprite.Init(pixelsToUnits);
				sprite.LocalOrderInLayer = sprites.Count;
				sprites.Add(sprite);
			}
			return sprite;
		}

		/// <summary>
		/// セーブデータ用のバイナリ書き込み
		/// </summary>
		/// <param name="writer">バイナリライター</param>
		public void WriteSaveData(BinaryWriter writer)
		{
			UtageToolKit.WriteLocalTransform(this.transform, writer);
			UtageToolKit.WriteColor(this.LocalColor, writer);

			//無限ループのTweenがある場合は、Tween情報を書き込む
			iTweenPlayer[] tweenArray = this.gameObject.GetComponents<iTweenPlayer>() as iTweenPlayer[];
			int tweenCount = 0;
			foreach (var tween in tweenArray)
			{
				if (tween.IsEndlessLoop) ++tweenCount;
			}
			writer.Write(tweenCount);
			foreach( var tween in tweenArray )
			{
				if (tween.IsEndlessLoop) tween.Write(writer);
			}

			//各スプライトの書き込み
			writer.Write(sprites.Count);
			foreach (var sprite in sprites)
			{
				writer.Write(sprite.name);
				sprite.Write(writer);
			}
			writer.Write( DefaultSprite != null ? DefaultSprite.name : "");
		}

		/// <summary>
		/// セーブデータ用のバイナリ読みこみ
		/// </summary>
		/// <param name="reader">バイナリリーダー</param>
		public void ReadSaveData(BinaryReader reader)
		{
			UtageToolKit.ReadLocalTransform(this.transform, reader);
			this.LocalColor = UtageToolKit.ReadColor(reader);
			//Tweenがある場合は、Tween情報を読み込む
			int tweenCount = reader.ReadInt32();
			for (int i = 0; i < tweenCount; ++i)
			{
				iTweenPlayer tween = this.gameObject.AddComponent<iTweenPlayer>() as iTweenPlayer;
				tween.Read(reader,pixelsToUnits);
			}

			//各スプライトの読み込み
			int count = reader.ReadInt32();
			for (int i = 0; i < count; ++i)
			{
				string name = reader.ReadString();
				AdvFadeSprites sprite = GetSpriteCreateIfMissing(name);
				sprite.Read(reader);
			}
			string defaultSpriteName = reader.ReadString();
			if (!string.IsNullOrEmpty(defaultSpriteName))
			{
				DefaultSprite = FindSprite(defaultSpriteName);
			}
		}
	}
}
