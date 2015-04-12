﻿//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------
using System.Collections;
using System.Collections.Generic;


namespace Utage
{

	/// <summary>
	/// シリアライズ可能な自作Dictionary用のキーバリュー
	/// ファイルのロード機能つき
	/// </summary>
	[System.Serializable]
	public abstract partial class SerializableDictionaryFileReadKeyValue : SerializableDictionaryKeyValue
	{
		/// <summary>
		/// 文字列グリッドの行データから、データを初期化
		/// </summary>
		/// <param name="row">初期化するための文字列グリッドの行データ</param>
		/// <returns>成否。空のデータの場合などはfalseが帰る</returns>
		public abstract bool InitFromStringGridRow(StringGridRow row);
	}


	/// <summary>
	/// シリアライズ可能な自作Dictionary
	/// ファイルのロード機能つき
	/// </summary>
	/// <typeparam name="T">キーバリューを定義したクラス</typeparam>
	[System.Serializable]
	public abstract partial class SerializableDictionaryFileRead<T> : SerializableDictionary<T>
		where T : SerializableDictionaryFileReadKeyValue, new()
	{
		/// <summary>
		/// ロードが終了したか
		/// </summary>
		public bool IsLoadEnd { get { return this.isLoadEnd; } }
		bool isLoadEnd;

		/// <summary>
		/// 文字列グリッドから、データ初期化
		/// </summary>
		/// <param name="grid">初期化するための文字列グリッド</param>
		public virtual void InitFromStringGrid(StringGrid grid)
		{
			Clear();
			ParseBeign();
			ParseFromStringGrid(grid);
			ParseEnd();
		}

		/// <summary>
		/// 文字列グリッドから、データ解析
		/// </summary>
		/// <param name="grid"></param>
		protected virtual void ParseFromStringGrid(StringGrid grid)
		{
			foreach (StringGridRow row in grid.Rows)
			{
				if (row.RowIndex < grid.DataTopRow) continue;			//データの行じゃない
				if (row.IsEmpty) continue;								//データがない
				T data = new T();
				if (data.InitFromStringGridRow(row))
				{
					Add(data);
				}
			}
		}

		/// <summary>
		/// 解析の前処理
		/// </summary>
		protected virtual void ParseBeign() { }


		/// <summary>
		/// 解析の後処理
		/// </summary>
		protected virtual void ParseEnd() { }


		/// <summary>
		/// CSV設定ファイルをロードして、データ作成
		/// </summary>
		/// <param name="filePathInfoList">ロードするパスのリスト</param>
		/// <returns></returns>
		public virtual IEnumerator LoadCsvAsync(List<AssetFilePathInfo> filePathInfoList)
		{
			isLoadEnd = false;
			Clear();
			ParseBeign();

			List<AssetFile> fileList = new List<AssetFile>();

			foreach (AssetFilePathInfo filePathInfo in filePathInfoList)
			{
				fileList.Add(AssetFileManager.Load(filePathInfo.Path, filePathInfo.Version,  this));
			}
			foreach (AssetFile file in fileList)
			{
				while (!file.IsLoadEnd) yield return 0;
				if (!file.IsLoadError)
				{
					ParseFromStringGrid(file.Csv);
				}
				file.Unuse(this);
			}

			ParseEnd();
			isLoadEnd = true;
		}
	};
}
