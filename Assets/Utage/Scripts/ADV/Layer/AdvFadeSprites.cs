//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.IO;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// フェード切り替え機能つきのスプライト表示
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/FadeSprites")]
	public class AdvFadeSprites : Node2D
	{
		float pixelsToUnits;

		/// <summary>
		/// 現在のスプライト
		/// </summary>
		public Sprite2D CurrentSprite { get { return currentSprite; } }
		Sprite2D currentSprite;

		/// <summary>
		/// 現在のテクスチャ
		/// </summary>
		public AssetFile CurrentTexture { get { return currentSprite == null ? null : currentSprite.TextureFile; } }

		Sprite2D fadeOutSprite;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="pixelsToUnits"></param>
		public void Init(float pixelsToUnits)
		{
			this.pixelsToUnits = pixelsToUnits;
		}

		/// <summary>
		/// テクスチャからスプライト作成
		/// 前に別のテクスチャが表示されていれば、それをフェードアウトさせる
		/// </summary>
		/// <param name="texture">テクスチャ</param>
		/// <param name="fadeTime">フェード時間</param>
		public void SetTexture(AssetFile texture, float fadeTime)
		{
			//テクスチャが同じなら、変化なし
			if (CurrentTexture == texture) return;

			//フェードアウト中のスプライトは消す
			if (fadeOutSprite != null)	//destoryされたコンポーネントはnull判定になるはず･･･
			{
				fadeOutSprite.FadeOut(0, true);
				fadeOutSprite = null;
			}
			
			if (currentSprite != null)
			{
				//既にスプライトがあるならフェードアウトさせる
				fadeOutSprite = currentSprite;
				///表示順は手前にする
				fadeOutSprite.LocalOrderInLayer = fadeOutSprite.LocalOrderInLayer + 1;
				fadeOutSprite.FadeOut(fadeTime, true);

				//テクスチャからスプライト作成
				currentSprite = CreateSprite(texture);
			}
			else
			{
				//新規スプライトがあるならフェードインさせる
				//テクスチャからスプライト作成
				currentSprite = CreateSprite(texture);
				currentSprite.FadeIn(fadeTime);
			}
		}

		/// <summary>
		/// セーブデータ用のバイナリ書き込み
		/// </summary>
		/// <param name="writer">バイナリライター</param>
		public void Write(BinaryWriter writer)
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
			foreach (var tween in tweenArray)
			{
				if (tween.IsEndlessLoop) tween.Write(writer);
			}

			string nameCurrenTexture = "";
			if (CurrentSprite != null)
			{
				if (CurrentSprite.TextureFile != null)
				{
					nameCurrenTexture = CurrentSprite.TextureFile.FileName;
				}
			}
			writer.Write(nameCurrenTexture);
		}

		/// <summary>
		/// セーブデータ用のバイナリ読みこみ
		/// </summary>
		/// <param name="reader">バイナリリーダー</param>
		public void Read(BinaryReader reader)
		{
			UtageToolKit.ReadLocalTransform(this.transform, reader);
			this.LocalColor = UtageToolKit.ReadColor(reader);

			//Tweenがある場合は、Tween情報を読み込む
			int tweenCount = reader.ReadInt32();
			for (int i = 0; i < tweenCount; ++i)
			{
				iTweenPlayer tween = this.gameObject.AddComponent<iTweenPlayer>() as iTweenPlayer;
				tween.Read(reader, pixelsToUnits);
			}

			string nameCurrenTexture = reader.ReadString();
			AssetFile texture = AssetFileManager.Load(nameCurrenTexture, this);
			currentSprite = CreateSprite(texture);
			texture.Unuse(this);
		}

		Sprite2D CreateSprite(AssetFile texture)
		{

			Sprite2D sprite = UtageToolKit.AddChildGameObjectComponent<Sprite2D>(this.transform, System.IO.Path.GetFileNameWithoutExtension(texture.FileName));
			sprite.SetTextureFile(texture, pixelsToUnits);
			return sprite;
		}
	}
}