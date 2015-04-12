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
	/// レイヤーの管理
	/// </summary>
	[AddComponentMenu("Utage/ADV/LayerManager")]
	[RequireComponent(typeof(Node2D))]
	public class AdvLayerManager : MonoBehaviour
	{
		/// <summary>
		/// レイヤーリスト
		/// </summary>
		public Dictionary<string, AdvLayer> Layers { get { return layers; } }
		Dictionary<string, AdvLayer> layers = new Dictionary<string, AdvLayer>();

		/// <summary>
		/// 背景レイヤー
		/// </summary>
		public AdvLayer BgLayer { get { return bgLayer; } }
		AdvLayer bgLayer;

		/// <summary>
		/// キャラクターレイヤー
		/// </summary>
		public Dictionary<string, AdvLayer> CharacterLayers { get { return characterLayers; } }
		Dictionary<string, AdvLayer> characterLayers = new Dictionary<string, AdvLayer>();

		/// <summary>
		/// デフォルトのキャラクターレイヤー
		/// </summary>
		public AdvLayer DefaultCharacterLayer { get { return defaultCharacterLayer; } }
		AdvLayer defaultCharacterLayer;

		/// <summary>
		/// スプライトレイヤー
		/// </summary>
		public Dictionary<string, AdvLayer> SpriteLayers { get { return spriteLayers; } }
		Dictionary<string, AdvLayer> spriteLayers = new Dictionary<string, AdvLayer>();

		/// <summary>
		/// デフォルトのスプライトレイヤー
		/// </summary>
		public AdvLayer DefaultSpriteLayer { get { return defaultSpriteLayer; } }
		AdvLayer defaultSpriteLayer;

		/// <summary>
		/// イベントモード（キャラクター立ち絵非表示）
		/// </summary>
		public bool IsEventMode { get { return this.isEventMode; } set { isEventMode = value; } }
		bool isEventMode;

		/// <summary>
		/// スプライトを作成する際の、座標1.0単位辺りのピクセル数
		/// </summary>
		public float PixelsToUnits { get { return pixelsToUnits; } }
		[SerializeField]
		float pixelsToUnits = 100;

		[SerializeField]
		string bgSpriteName = "BG";

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="setting">レイヤー設定データ</param>
		public void InitLayerSetting(AdvLayerSetting setting)
		{
			foreach (AdvLayerSettingData item in setting.List)
			{
				AdvLayer layer = AddLayer(item);
				if (item == setting.DefaultBGLayer)
				{
					bgLayer = layer;
				}
				if (item == setting.DefaultCharacterLayer)
				{
					defaultCharacterLayer = layer;
				}

				if (item == setting.DefaultSpriteLayer)
				{
					defaultSpriteLayer = layer;
				}
			}
			if (bgLayer == null)
			{
				AdvLayerSettingData data = new AdvLayerSettingData();
				data.InitDefault("BG UtageDefault", AdvLayerSettingData.LayerType.Bg, 0);
				bgLayer = AddLayer(data);
			}
			if (defaultCharacterLayer == null)
			{
				AdvLayerSettingData data = new AdvLayerSettingData();
				data.InitDefault("Character UtageDefault", AdvLayerSettingData.LayerType.Character, 100);
				defaultCharacterLayer = AddLayer(data);
			}
			if (defaultSpriteLayer == null)
			{
				AdvLayerSettingData data = new AdvLayerSettingData();
				data.InitDefault("Sprite UtageDefault", AdvLayerSettingData.LayerType.Sprite, 200);
				defaultSpriteLayer = AddLayer(data);
			}
		}

		AdvLayer AddLayer(AdvLayerSettingData data)
		{
			AdvLayer layer = UtageToolKit.AddChildGameObjectComponent<AdvLayer>(this.transform, data.Name);
			layer.Init(data, PixelsToUnits);
			layers.Add(data.Name, layer);
			//キャラクターレイヤー登録
			if (data.Type == AdvLayerSettingData.LayerType.Character)
			{
				characterLayers.Add(data.Name, layer);
			}

			//スプライトレイヤー登録
			if (data.Type == AdvLayerSettingData.LayerType.Sprite)
			{
				spriteLayers.Add(data.Name, layer);
			}
			return layer;
		}

		/// <summary>
		/// クリア
		/// </summary>
		public void Clear()
		{
			foreach (AdvLayer layer in layers.Values)
			{
				layer.Clear();
				Destroy( layer.gameObject );
			}
			layers.Clear();

			foreach (AdvLayer layer in characterLayers.Values)
			{
				layer.Clear();
			}
			characterLayers.Clear();

			foreach (AdvLayer layer in spriteLayers.Values)
			{
				layer.Clear();
			}
			spriteLayers.Clear();
		}

		/// <summary>
		/// 名前からレイヤー取得
		/// </summary>
		/// <param name="name">取得するレイヤーの名前</param>
		/// <returns>レイヤー</returns>
		public AdvLayer GetLayer(string name)
		{
			AdvLayer layer;
			if (layers.TryGetValue(name, out layer))
			{
				return layer;
			}
			return null;
		}

		/// <summary>
		/// 背景テクスチャの設定
		/// </summary>
		/// <param name="file">テクスチャファイル</param>
		/// <param name="x">表示座標X floatのobject　nullを指定することで位置移動なし</param>
		/// <param name="y">表示座標Y floatのobject　nullを指定することで位置移動なし</param>
		/// <param name="fadeTime">フェード時間</param>
		public void SetBgSprite(AssetFile file, object x, object y, float fadeTime)
		{
			if (BgLayer == null)
			{
				Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.BgLayerIsNotDefined));
			}
			else
			{
				BgLayer.SetDefaultSprite(bgSpriteName, file, x, y, fadeTime);
			}
		}

		/// <summary>
		/// 背景表示をオフに
		/// </summary>
		/// <param name="fadeTime">フェードアウト時間</param>
		public void BgOff(float fadeTime)
		{
			BgLayer.FadeOutDefaultSprite(fadeTime);
		}

		/// <summary>
		/// スプライトの設定
		/// </summary>
		/// <param name="file">テクスチャファイル</param>
		/// <param name="layerName">レイヤー名</param>
		/// <param name="spriteName">スプライト名</param>
		/// <param name="x">表示座標X floatのobject　nullを指定することで位置移動なし</param>
		/// <param name="y">表示座標Y floatのobject　nullを指定することで位置移動なし</param>
		/// <param name="fadeTime">フェード時間</param>
		public void SetSprite(AssetFile file, string layerName, string spriteName, object x, object y, float fadeTime)
		{
			//スプライトを設定
			FindSpriteLayer(spriteName).SetSprite(file, spriteName, x, y, fadeTime);
		}

		/// <summary>
		/// スプライトの表示OFF
		/// </summary>
		/// <param name="spriteName">スプライト名</param>
		/// <param name="fadeTime">フェード時間</param>
		public void SpriteFadeOut(string spriteName, float fadeTime)
		{
			//スプライトを削除
			foreach (var item in layers.Values)
			{
				if (item.FindSprite(spriteName) != null)
				{
					item.FadeOutSprite(spriteName,fadeTime);
				}
			}
		}
		/// <summary>
		/// 全スプライトの表示OFF
		/// </summary>
		/// <param name="fadeTime">フェード時間</param>
		public void SpriteFadeOutAll(float fadeTime)
		{
			foreach (var item in spriteLayers.Values)
			{
				item.FadeOutAllSprite(fadeTime);
			}
		}

		AdvLayer FindSpriteLayer(string layerName)
		{
			AdvLayer layer = null;
			if (!string.IsNullOrEmpty(layerName))
			{
				spriteLayers.TryGetValue(layerName, out layer);
			}
			if (null == layer)
			{
				//レイヤーが名前で設定できないならデフォルトレイヤーを使う
				layer = DefaultSpriteLayer;
			}
			return layer;
		}


		/// <summary>
		/// キャラクタ－テクスチャの設定
		/// </summary>
		/// <param name="layerName">レイヤー名</param>
		/// <param name="characterName">キャラクター名</param>
		/// <param name="texture">テクスチャ名</param>
		/// <param name="x">表示座標X floatのobject　nullを指定することで位置移動なし</param>
		/// <param name="y">表示座標Y floatのobject　nullを指定することで位置移動なし</param>
		/// <param name="fadeTime">フェード時間</param>
		/// <returns>成功したらture。テクスチャがないなどで失敗したらfalse</returns>
		public bool SetCharacterSprite(string layerName, string characterName, AssetFile texture, object x, object y, float fadeTime)
		{

			//既に同名のキャラがいるなら、そのレイヤーを取得
			AdvLayer oldLayer = FindCurrentCharacterLayer(characterName);
			if (oldLayer != null && texture == null)
			{
				//テクスチャ指定がない場合、前の同名キャラのテクスチャを引き継ぐ
				if (oldLayer.DefaultSprite != null)
				{
					texture = oldLayer.DefaultSprite.CurrentTexture;
				}
			}

			if (texture == null)
			{
				//キャラのテクスチャ表示なし（エラーではない）
				return false;
			}

			//表示するレイヤーを探す
			AdvLayer layer = null;
			if (!string.IsNullOrEmpty(layerName))
			{
				characterLayers.TryGetValue(layerName, out layer);
			}
			if (null == layer)
			{
				//レイヤーが名前で設定できないなら、今のレイヤーをそのまま使う
				layer = oldLayer;
				if (null == layer)
				{
					//それもなければデフォルトレイヤーを使う
					layer = DefaultCharacterLayer;
				}
			}

			//スプライトを設定
			layer.SetDefaultSprite( characterName, texture, x, y, fadeTime);
			//レイヤーが変わる場合は、昔のスプライトを消す
			if (oldLayer != layer && oldLayer != null)
			{
				oldLayer.FadeOutDefaultSprite(fadeTime);
			}
			return true;
		}

		AdvLayer FindCurrentCharacterLayer(string characterName)
		{
			foreach( AdvLayer layer in characterLayers.Values )
			{
				if (layer.DefaultSprite != null && layer.DefaultSprite.name == characterName)
				{
					return layer;
				}
			}
			return null;
		}

		/// <summary>
		/// キャラクター表示をフェードアウト
		/// </summary>
		/// <param name="fadeTime">フェード時間</param>
		public void CharacterFadeOutAll(float fadeTime)
		{
			foreach (AdvLayer item in CharacterLayers.Values)
			{
				item.FadeOutDefaultSprite(fadeTime);
			}
		}

		/// <summary>
		/// 指定の名前のキャラクター表示をオフ
		/// </summary>
		/// <param name="characterName">キャラクター名</param>
		/// <param name="fadeTime">フェード時間</param>
		public void CharacterFadeOut(string characterName, float fadeTime)
		{
			AdvLayer layer = FindCurrentCharacterLayer(characterName);
			if (layer!=null)
			{
				layer.FadeOutDefaultSprite(fadeTime);
			}
		}

		/// <summary>
		/// 指定の名前のGameObjectを取得
		/// </summary>
		/// <param name="name"></param>
		public GameObject FindGameObject(string name)
		{
			AdvLayer layer;
			if( layers.TryGetValue( name, out layer ) )
			{
				return layer.gameObject;
			}
			foreach (var item in layers.Values)
			{
				AdvFadeSprites sprite = item.FindSprite(name);
				if (sprite != null)
				{
					return sprite.gameObject;
				}
			}
			return null;
		}

		/// <summary>
		/// セーブデータ用のバイナリを取得
		/// </summary>
		/// <returns>セーブデータのバイナリ</returns>
		public byte[] ToSaveDataBuffer()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				//バイナリ化
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					WriteSaveData(writer);
				}
				return stream.ToArray();
			}
		}

		/// <summary>
		/// セーブデータを読みこみ
		/// </summary>
		/// <param name="buffer">セーブデータのバイナリ</param>
		public void ReadSaveDataBuffer(byte[] buffer)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					ReadSaveData(reader);
				}
			}
		}


		const int VERSION = 1;
		const int VERSION_0 = 0;
		//セーブデータ用のバイナリ書き込み
		void WriteSaveData(BinaryWriter writer)
		{
			writer.Write(VERSION);
			writer.Write(isEventMode);
			writer.Write(layers.Count);
			foreach (string key in layers.Keys)
			{
				writer.Write(key);
				layers[key].WriteSaveData(writer);
			}
		}
		//セーブデータ用のバイナリ読み込み
		void ReadSaveData(BinaryReader reader)
		{
			int version = reader.ReadInt32();
			if (version >= VERSION_0 )
			{
				this.Clear();
				if (version >= VERSION)
				{
					this.isEventMode = reader.ReadBoolean();
				}

				int count = reader.ReadInt32();
				for (int i = 0; i < count; i++)
				{
					string key = reader.ReadString();
					AdvLayer layer = GetLayer(key);
					if (null != layer)
					{
						layer.ReadSaveData(reader);
					}
					else
					{
						Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.ReadLayerSaveData,key));
					}
				}
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, version));
			}
		}
	}
}