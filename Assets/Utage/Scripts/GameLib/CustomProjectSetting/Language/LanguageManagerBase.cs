//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{
	public enum LanguageName
	{
		English,
		Japanese,
	}

	/// <summary>
	/// 表示言語切り替え用のクラス
	/// </summary>
	public abstract class LanguageManagerBase : ScriptableObject
	{
		static LanguageManagerBase instance;
		/// <summary>
		/// シングルトンなインスタンスの取得
		/// </summary>
		/// <returns></returns>
		public static LanguageManagerBase Instance { get{return instance;} set{ instance = value; }}

		const string Auto = "Auto";

		/// <summary>
		/// 設定言語
		/// </summary>
		public string Language{
			get { return language; }
		}
		[SerializeField]
		protected string language = Auto;

		/// <summary>
		/// 現在の設定言語
		/// </summary>
		public string CurrentLanguage
		{
			get { return currentLanguage; }
			set
			{
				if (!IsInit) Init();
				if (currentLanguage != value)
				{
					currentLanguage = value;
					RefreshCurrentLanguage();
				}
			}
		}
		protected string currentLanguage;

		/// <summary>
		/// 対応言語
		/// </summary>
		public List<string> Languages{ get{ return languageList;}}
		protected List<string> languageList = new List<string>();

		//デフォルト言語
		public string DefaultLanguage { get { return defaultLanguage; } }
		[SerializeField]
		protected string defaultLanguage = "Japanese";

		//データ
		[SerializeField]
		protected List<TextAsset> languageData;

		//現在設定されている言語のリスト
		protected Dictionary<string,Language> languegeDataTbl = new Dictionary<string,Language>();

		/// <summary>
		/// 初期化フラグ
		/// </summary>
		public bool IsInit { get { return languegeDataTbl.Count > 0; } }

		/// <summary>
		/// 指定のキーのテキストを、指定のデータの、設定された言語に翻訳して取得
		/// </summary>
		/// <param name="dataName">データ名</param>
		/// <param name="key">テキストのキー</param>
		/// <returns>翻訳したテキスト</returns>
		public string LocalizeText( string dataName, string key )
		{
			if (!IsInit) Init();

			Language language;
			if (languegeDataTbl.TryGetValue(dataName, out language))
			{
				return language.LocalizeText(key);
			}
			else
			{
				Debug.LogError(dataName + " is not found");
				return key;
			}
		}

		/// <summary>
		/// 指定のキーのテキストを、全データ内から検索して、設定された言語に翻訳して取得
		/// </summary>
		/// <param name="key">テキストのキー</param>
		/// <returns>翻訳したテキスト</returns>
		public string LocalizeText(string key)
		{
			if (!IsInit) Init();

			foreach (Language language in languegeDataTbl.Values)
			{
				if (language.ContainsKey(key))
				{
					return language.LocalizeText(key);
				}
			}
			Debug.LogError(key + " is not found");
			return key;
		}


		void Awake()
		{
			Init();
		}

		/// <summary>
		/// インスペクターから値が変更された場合
		/// </summary>
		void OnValidate()
		{
			Init();
		}

		void Init()
		{
			languegeDataTbl.Clear();
			languageList.Clear();
			if (languageData == null) return;

			foreach (var item in languageData)
			{
				if(item!=null)
				{
					languegeDataTbl.Add(item.name, new Language(item));
				}
			}
			foreach (var item in languegeDataTbl.Values)
			{
				foreach (var lang in item.Languages)
				{
					if (!languageList.Contains(lang))
					{
						languageList.Add(lang);
					}
				}
			}

			//システムの言語に変更
			currentLanguage = (language == Auto) ? Application.systemLanguage.ToString() : language;
			RefreshCurrentLanguage();
		}

		protected abstract void RefreshCurrentLanguage();
	}
}
