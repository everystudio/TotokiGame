//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace Utage
{

	/// <summary>
	/// シナリオの設定データ
	/// </summary>
	[System.Serializable]
	public partial class AdvScenarioSettingData : SerializableDictionaryFileReadKeyValue
	{
		/// <summary>シナリオファイル</summary>
		public string ScenaioFile { get { return this.Key; } }

		/// <summary>バージョン</summary>
		public int Version { get { return this.version; } }
		[SerializeField]
		int version;

		// TODO:
		/// <summary>回想モードがオープンされているか？工事中</summary>
		public bool IsGalleryOpen { get { return this.isGalleryOpen; } set { this.isGalleryOpen = value; } }
		bool isGalleryOpen;


		/// <summary>
		/// StringGridの一行からデータ初期化
		/// </summary>
		/// <param name="row">初期化するためのデータ</param>
		/// <returns>成否</returns>
		public override bool InitFromStringGridRow(StringGridRow row)
		{
			string key = AdvParser.ParseCell<string>(row,AdvColumnName.FileName);
			InitKey(key);
			this.version = AdvParser.ParseCellOptional<int>(row, AdvColumnName.Version, 0);
			return true;
		}
	}

	/// <summary>
	/// シナリオの設定データ
	/// </summary>
	[System.Serializable]
	public partial class AdvScenarioSetting : SerializableDictionaryFileRead<AdvScenarioSettingData>
	{

		string defaultDir;
		string defaultExt;

		/// <summary>
		/// 起動時の初期化
		/// </summary>
		/// <param name="settingData">設定データ</param>
		public void BootInit(AdvBootSetting settingData)
		{
			this.defaultDir = settingData.ScenarioDirInfo.defaultDir;
			this.defaultExt = settingData.ScenarioDirInfo.defaultExt;
			foreach (AdvScenarioSettingData data in List)
			{
				AssetFile file = AssetFileManager.GetFileCreateIfMissing(ScenaioFileToPath(data.ScenaioFile));
				file.Version = data.Version;
			}
		}

		/// <summary>
		/// ファイル名をパスに
		/// </summary>
		/// <param name="fileName">ファイル名</param>
		/// <returns>ファイルパス</returns>
		public string ScenaioFileToPath(string scenaioFile)
		{
			//既に絶対URLならそのまま
			if (UtageToolKit.IsAbsoluteUri(scenaioFile))
			{
				return scenaioFile;
			}
			else
			{
				//拡張子がなければデフォルト拡張子を追加
				if (string.IsNullOrEmpty(System.IO.Path.GetExtension(scenaioFile)))
				{
					scenaioFile += defaultExt;
				}
				return defaultDir + scenaioFile;
			}
		}

//#if UNITY_EDITOR

		/// <summary>
		/// エクセルからCSVファイルにコンバートする際に、シナリオ設定データをマージして作成する
		/// </summary>
		/// <param name="grid">シナリオ設定データ</param>
		/// <param name="scenarioSheetDictionary">シナリオデータ</param>
		/// <returns>マージしたシナリオ設定データ</returns>
		public static StringGrid MargeScenarioData(StringGrid grid, StringGridDictionary scenarioSheetDictionary, int version )
		{
			if (grid == null)
			{
				grid = new StringGrid(AdvSettingDataManager.SheetNameScenario,CsvType.Csv);
				grid.AddRow(new List<string> { AdvParser.Localize(AdvColumnName.FileName), AdvParser.Localize(AdvColumnName.Version) });
				grid.ParseHeader();
			}

			List<string> addScnenarioList = new List<string>();
			foreach (string sheetName in scenarioSheetDictionary.Keys)
			{
				bool isFind = false;
				foreach (StringGridRow row in grid.Rows)
				{
					if (AdvParser.ParseCell<string>(row,AdvColumnName.FileName) == sheetName)
					{
						isFind = true;
					}
				}
				if (!isFind)
				{
					addScnenarioList.Add(sheetName);
				}
			}
			foreach (string sheetName in addScnenarioList)
			{
				grid.AddRow(new List<string> { sheetName, ""+version });
			}
			return grid;
		}
//#endif
	}
}