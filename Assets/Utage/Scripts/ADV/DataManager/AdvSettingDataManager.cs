//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using System.Collections;

namespace Utage
{

	/// <summary>
	/// 設定データの管理
	/// </summary>
	public partial class AdvSettingDataManager : ScriptableObject
	{
		/// <summary>
		/// 基本設定データ
		/// </summary>
		public AdvBootSetting SettingData { get { return this.settingData ?? (settingData = new AdvBootSetting()); } }
		AdvBootSetting settingData;

		/// <summary>
		/// シナリオファイル設定
		/// </summary>
		public AdvScenarioSetting ScenarioSetting { get { return this.scenarioSetting ?? (scenarioSetting = new AdvScenarioSetting()); } }
		[SerializeField]
		AdvScenarioSetting scenarioSetting;

		/// <summary>
		/// キャラクターテクスチャ設定
		/// </summary>
		public AdvCharacterSetting CharacterSetting { get { return this.characterSetting ?? (characterSetting = new AdvCharacterSetting()); } }
		[SerializeField]
		AdvCharacterSetting characterSetting;

		/// <summary>
		/// テクスチャ設定
		/// </summary>
		public AdvTextureSetting TextureSetting { get { return this.textureSetting ?? (textureSetting = new AdvTextureSetting()); } }
		[SerializeField]
		AdvTextureSetting textureSetting;

		/// <summary>
		/// サウンドファイル設定
		/// </summary>
		public AdvSoundSetting SoundSetting { get { return this.soundSetting ?? (soundSetting = new AdvSoundSetting()); } }
		[SerializeField]
		AdvSoundSetting soundSetting;

		/// <summary>
		/// レイヤー設定
		/// </summary>
		public AdvLayerSetting LayerSetting { get { return this.layerSetting ?? (layerSetting = new AdvLayerSetting()); } }
		[SerializeField]
		AdvLayerSetting layerSetting;

		/// <summary>
		/// パラメーター設定
		/// </summary>
		public AdvParamSetting DefaultParam { get { return this.defaultParam ?? (defaultParam = new AdvParamSetting()); } }
		[SerializeField]
		AdvParamSetting defaultParam;

		/// <summary>
		/// シーン回想設定
		/// </summary>
		public AdvSceneGallerySetting SceneGallerySetting { get { return this.sceneGallerySetting ?? (sceneGallerySetting = new AdvSceneGallerySetting()); } }
		[SerializeField]
		AdvSceneGallerySetting sceneGallerySetting;
		

		/// <summary>
		/// データをクリア
		/// </summary>
		public void Clear()
		{
			settingData = new AdvBootSetting();
			LayerSetting.Clear();
			CharacterSetting.Clear();
			TextureSetting.Clear();
			SoundSetting.Clear();
			ScenarioSetting.Clear();
			DefaultParam.Clear();
			SceneGallerySetting.Clear();
		}

		/// <summary>
		/// 起動時の初期化
		/// </summary>
		/// <param name="rootDirResource">ルートディレクトリのリソース</param>
		public void BootInit(string rootDirResource)
		{
			SettingData.BootInit(rootDirResource);
			LayerSetting.BootInit(SettingData);
			ScenarioSetting.BootInit(SettingData);
			CharacterSetting.BootInit(SettingData);
			TextureSetting.BootInit(SettingData);
			SoundSetting.BootInit(SettingData);
			SceneGallerySetting.BootInit(SettingData);
		}

		/// <summary>
		/// 全リソースをバックグラウンドでダウンロード
		/// </summary>
		public void DownloadAll()
		{
			CharacterSetting.DownloadAll();
			TextureSetting.DownloadAll();
			SoundSetting.DownloadAll();
			SceneGallerySetting.DownloadAll();
		}

		//****************************　CSVのロード用　****************************//

		/// <summary>
		/// CSVファイルからロードして初期化
		/// </summary>
		/// <param name="parent">コルーチンをまわすためのMonoBehaviour</param>
		/// <param name="url">ファイルパス</param>
		/// <param name="version">シナリオバージョン（-1以下で必ずサーバーからデータを読み直す）</param>
		/// <returns></returns>
		public IEnumerator LoadCsvAsync(MonoBehaviour parent, string url,int version )
		{
			//起動ファイルの読み込み
			{
				AssetFile file = AssetFileManager.GetFileCreateIfMissing(url);
				if (version < 0)
				{
					file.Version = file.CacheVersion + 1;
				}
				AssetFileManager.Load(file, this);
				while (!file.IsLoadEnd) yield return 0;
				SettingData.InitFromCsv(file.Csv, url);
				file.Unuse(this);
			}

			parent.StartCoroutine(ScenarioSetting.LoadCsvAsync(SettingData.ScenarioSettingUrlList));
			parent.StartCoroutine(CharacterSetting.LoadCsvAsync(SettingData.CharacterSettingUrlList));
			parent.StartCoroutine(TextureSetting.LoadCsvAsync(SettingData.TextureSettingUrlList));
			parent.StartCoroutine(SoundSetting.LoadCsvAsync(SettingData.SoundSettingUrlList));
			parent.StartCoroutine(DefaultParam.LoadCsvAsync(SettingData.ParamSettingUrlList));
			parent.StartCoroutine(LayerSetting.LoadCsvAsync(SettingData.LayerSettingUrlList));
			parent.StartCoroutine(SceneGallerySetting.LoadCsvAsync(SettingData.SceneGallerySettingUrlList));


			while (!IsLoadEndCsv())
			{
				yield return 0;
			}
		}

		bool IsLoadEndCsv()
		{
			return (
				ScenarioSetting.IsLoadEnd
				&& CharacterSetting.IsLoadEnd
				&& TextureSetting.IsLoadEnd
				&& SoundSetting.IsLoadEnd
				&& DefaultParam.IsLoadEnd
				&& LayerSetting.IsLoadEnd
				&& SceneGallerySetting.IsLoadEnd
				);
		}

		//****************************　エクセルのロード用　****************************//

		public const string SheetNameBoot = "Boot";
		public const string SheetNameScenario = "Scenario";
		public const string SheetNameCharacter = "Character";
		public const string SheetNameTexture = "Texture";
		public const string SheetNameSound = "Sound";
		public const string SheetNameLayer = "Layer";
		public const string SheetNameParam = "Param";
		public const string SheetNameSceneGallery = "SceneGallery";

		/// <summary>
		/// 起動設定用のエクセルシートか判定
		/// </summary>
		/// <param name="sheetName">シート名</param>
		/// <returns>起動用ならtrue。違うならfalse</returns>
		public static bool IsBootSheet(string sheetName)
		{
			switch (sheetName)
			{
				case SheetNameBoot:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// シナリオ設定用のエクセルシートか判定
		/// </summary>
		/// <param name="sheetName">シート名</param>
		/// <returns>起動用ならtrue。違うならfalse</returns>
		public static bool IsScenarioSettingSheet(string sheetName)
		{
			switch (sheetName)
			{
				case SheetNameScenario:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// 設定用のエクセルシートか判定
		/// </summary>
		/// <param name="sheetName">シート名</param>
		/// <returns>設定用ならtrue。違うならfalse</returns>
		public static bool IsSettingsSheet(string sheetName)
		{
			switch (sheetName)
			{
				case SheetNameScenario:
				case SheetNameCharacter:
				case SheetNameTexture:
				case SheetNameSound:
				case SheetNameLayer:
				case SheetNameParam:
				case SheetNameSceneGallery:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// 設定データのエクセルシートを読み込む
		/// </summary>
		/// <param name="sheetName">シート名</param>
		/// <param name="grid">シートのStringGridデータ</param>
		public void ParseFromExcel(string sheetName, StringGrid grid)
		{
			switch (sheetName)
			{
				case SheetNameBoot:
					SettingData.InitFromStringGrid(grid);
					break;
				case SheetNameScenario:
					ScenarioSetting.InitFromStringGrid(grid);
					break;
				case SheetNameCharacter:
					CharacterSetting.InitFromStringGrid(grid);
					break;
				case SheetNameTexture:
					TextureSetting.InitFromStringGrid(grid);
					break;
				case SheetNameSound:
					SoundSetting.InitFromStringGrid(grid);
					break;
				case SheetNameLayer:
					LayerSetting.InitFromStringGrid(grid);
					break;
				case SheetNameParam:
					DefaultParam.InitFromStringGrid(grid);
					break;
				case SheetNameSceneGallery:
					SceneGallerySetting.InitFromStringGrid(grid);
					break;
				default:
					Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.NotSettingSheet, sheetName));
					break;
			}
		}
	}
}