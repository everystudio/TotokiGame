//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utage
{

	//「Utage」のシナリオデータ用のエクセルファイルのプロジェクトデータ
	public class AdvScenarioDataProject : ScriptableObject
	{
		/// <summary>
		/// エクセルのリスト
		/// </summary>
		public List<Object> ExcelList
		{
			get { return excelList; }
			set { excelList = value; }
		}
		[SerializeField]
		List<Object> excelList = new List<Object>();

		/// <summary>
		/// エクセルのパスリスト
		/// </summary>
		public List<string> ExcelPathList
		{
			get { return UtageEditorToolKit.AssetsToPathList(excelList); }
		}


		/// <summary>
		/// コンバート先のパス
		/// </summary>
		[SerializeField]
		string convertPath;
		public string ConvertPath
		{
			get { return convertPath; }
			set { convertPath = value; }
		}

		/// <summary>
		/// コンバートファイルのバージョン
		/// </summary>
		[SerializeField]
		int convertVersion = 0;
		public int ConvertVersion
		{
			get { return convertVersion; }
			set { convertVersion = value; }
		}

		/// <summary>
		/// コンバート後にバージョンを自動で更新するか
		/// </summary>
		[SerializeField]
		bool isAutoUpdateVersionAfterConvert = false;
		public bool IsAutoUpdateVersionAfterConvert
		{
			get { return isAutoUpdateVersionAfterConvert; }
			set { isAutoUpdateVersionAfterConvert = value; }
		}


		/// <summary>
		/// インポート時に自動でコンバートをするか
		/// </summary>
		[SerializeField]
		bool isAutoConvertOnImport = false;
		public bool IsAutoConvertOnImport
		{
			get { return isAutoConvertOnImport; }
			set { isAutoConvertOnImport = value; }
		}



		public bool IsEnableImport
		{
			get
			{
				bool isEnableImport = false;
				foreach (Object asset in excelList)
				{
					if (null != asset)
					{
						isEnableImport = true;
						break;
					}
				}
				return isEnableImport;
			}
		}

		public bool IsEnableConvert
		{
			get { return IsEnableImport && !string.IsNullOrEmpty(ConvertPath); }
		}

		public void AddExcelAsset( Object asset )
		{
			excelList.Add(asset);
		}
	}
}