//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System;
using System.IO;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// 管理中のファイル情報
	/// これはシステム内部で使うので外から使うことは想定していない
	/// </summary>
	[System.Serializable]
	public class AssetFileInfo : SerializableDictionaryKeyValue
	{

		/// <summary>
		/// ファイルパス
		/// </summary>
		public string FilePath { get { return filePath; } }
		string filePath;

		/// <summary>
		/// バージョン
		/// </summary>
		public int Version { get { return version; } set { version = value; } }
		int version;

		/// <summary>
		/// キャッシュファイルのバージョン
		/// </summary>
		public int CacheVersion { get { return cacheVersion; } }
		int cacheVersion = -1;

		/// <summary>
		/// キャッシュパス
		/// </summary>
		public string CachePath { get { return this.cachePath; } }
		string cachePath = "";

		/// <summary>
		/// 昔のキャッシュパス
		/// </summary>
		public string OldCachePath { get { return this.oldCachePath; } }
		string oldCachePath = "";

		/// <summary>
		/// ファイルタイプ
		/// </summary>
		public AssetFileType FileType { get { return this.fileType; } }
		AssetFileType fileType;

		/// <summary>
		/// ファイルのおき場所のタイプ
		/// </summary>
		public AssetFileStrageType StrageType
		{
			get { return this.strageType; }
		}
		AssetFileStrageType strageType;

		/// <summary>
		/// 暗号化のタイプ
		/// </summary>
		public AssetFileCryptType CryptType { get { return this.cryptType; } }
		AssetFileCryptType cryptType;

		/// <summary>
		/// 暗号化ありか
		/// </summary>
		public bool IsCrypt { get { return CryptType != AssetFileCryptType.None; } }

		/// <summary>
		/// ロードフラグ
		/// </summary>
		public AssetFileLoadFlags LoadFlags { get { return this.loadFlags; } }
		AssetFileLoadFlags loadFlags;

		/// <summary>
		/// ロードフラグを追加
		/// </summary>
		public void AddLoadFlag(AssetFileLoadFlags flags)
		{
			loadFlags |= flags;
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <param name="type">ファイルタイプ</param>
		/// <param name="strageType">置き場所のタイプ</param>
		/// <param name="isCrypt">キャッシュ時に暗号化するか</param>
		public AssetFileInfo(string path, AssetFileType type, AssetFileStrageType strageType, bool isCrypt)
		{
			InitKey(path);
			this.fileType = type;
			this.strageType = strageType;
			this.cryptType = isCrypt ? AssetFileCryptType.Utage : AssetFileCryptType.None;
			this.filePath = Key;
			this.audioType = ExtensionUtil.GetAudioType(FilePath);
			CheckCsvLoadFlag();
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="fileVersion">バイナリリーダー</param>
		public AssetFileInfo(BinaryReader reader)
		{
			Read(reader);
			this.filePath = Key;
			this.audioType = ExtensionUtil.GetAudioType(FilePath);
			CheckCsvLoadFlag();
		}

		void CheckCsvLoadFlag()
		{
			if (FileType == AssetFileType.Csv && ExtensionUtil.IsCsv(FilePath))
			{
				loadFlags |= AssetFileLoadFlags.Csv;
			}
		}


		/// <summary>
		/// オーディオのタイプ
		/// </summary>
		public AudioType AudioType{ get{ return audioType; } }
		AudioType audioType;
		
		/// <summary>
		/// ストリーミングにするか
		/// </summary>
		public bool IsStreamingType
		{
			get
			{
				bool isStreaming = ((loadFlags & AssetFileLoadFlags.Streaming) != AssetFileLoadFlags.None);
				bool isSound = (FileType == AssetFileType.Sound);
				bool notCrypt = !IsCrypt;
				bool webNocache = (StrageType == AssetFileStrageType.WebNocache);	//サーバーからの直接ロードは、回線切断を考慮してストリーミングをしない
				return isStreaming && isSound && notCrypt && !webNocache;
			}
		}
		
		/// <summary>
		/// 3Dサウンドか？
		/// </summary>
		public bool IsAudio3D { get { return (loadFlags & AssetFileLoadFlags.Audio3D) != AssetFileLoadFlags.None; } }

		/// <summary>
		/// テクスチャにミップマップを使うか？
		/// </summary>
		public bool IsTextureMipmap { get { return (loadFlags & AssetFileLoadFlags.TextureMipmap) != AssetFileLoadFlags.None; } }

		/// <summary>
		/// CSVをロードする際にTSV形式でロードするか？
		/// </summary>
		public bool IsCsv { get { return (loadFlags & AssetFileLoadFlags.Csv) != AssetFileLoadFlags.None; } }

		/// <summary>
		/// キャッシュデータを削除
		/// </summary>
		public void DeleteCache()
		{
			if (!string.IsNullOrEmpty(CachePath))
			{
				if (System.IO.File.Exists(CachePath))
				{
					System.IO.File.Delete(CachePath);
				}
			}
			this.cacheVersion = -1;
			this.cachePath = "";
		}

		/// <summary>
		/// 古いキャッシュデータを削除
		/// </summary>
		public void DeleteOldCacheFile()
		{
			if (!string.IsNullOrEmpty(OldCachePath))
			{
				if (System.IO.File.Exists(OldCachePath))
				{
					System.IO.File.Delete(OldCachePath);
				}
			}
		}


		/// <summary>
		/// キャッシュファイルがあるか
		/// </summary>
		public bool ExistCahce
		{
			get
			{
				return (this.cacheVersion >= 0 && !string.IsNullOrEmpty(this.cachePath));
			}
		}

		/// <summary>
		/// キャシュ書き込みの準備
		/// </summary>
		/// <param name="id">キャッシュ番号</param>
		/// <param name="cacheRootDir">キャッシュのディレクトリ</param>
		/// <param name="isDebugFileName">デバッグ用のファイル名か？(ファイル名を隠蔽しないか)</param>
		/// <returns>キャッシュファイルパス</returns>
		public string ReadyToWriteCache(int id, string cacheRootDir, bool isDebugFileName)
		{
			oldCachePath = cachePath;
			//キャッシュ書き込みするものはパスを作る
			if (StrageType == AssetFileStrageType.Web)
			{
				if (isDebugFileName)
				{
					//デバッグ用に、DL元と同じファイル構成を再現
					cachePath = cacheRootDir + new Uri(FilePath).Host + new Uri(FilePath).AbsolutePath;
				}
				else
				{
					//キャッシュファイルIDで管理
					cachePath = cacheRootDir + (id);
				}
				cacheVersion = version;
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.NoChacheTypeFile));
				cachePath = "";
			}
			return cachePath;
		}

		/// <summary>
		/// 現在のバージョンがキャッシュされているか
		/// </summary>
		public bool IsCaching
		{
			get
			{
				if (StrageType == AssetFileStrageType.Web)
				{
					return cacheVersion >= version;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// 現在のバージョンをキャッシュに書きこむ必要があるか
		/// </summary>
		public bool IsWriteNewCache
		{
			get
			{
				if (StrageType == AssetFileStrageType.Web)
				{
					return cacheVersion < version;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// WWWでロードするパス
		/// </summary>
		public string LoadWWWPath
		{
			get
			{
				if (IsCaching)
				{
					return "file://" + CachePath;
				}
				else if (strageType== AssetFileStrageType.Resources)
				{
					return FilePath;
				}
				else
				{
					string url;


					// WebモードでStreamingAssetsを含むパスを持っている場合、
					// ローカルのデータを利用するものとする

					switch (strageType)
					{
						case AssetFileStrageType.Web:
							if( 0 <= FilePath.IndexOf( "StreamingAssets" ) ){
								if( Application.platform == RuntimePlatform.Android ){
									//url = System.IO.Path.Combine( Application.streamingAssetsPath, FilePath );
									url = FilePath;
								}
								else{
									//url = "file://" + System.IO.Path.Combine( Application.streamingAssetsPath, FilePath );
									url = "file://" + FilePath;
								}
								return url;
							}
							else {
								url = ToDownloadUrl();
							}
							break;
						case AssetFileStrageType.WebNocache:
							url = ToDownloadUrl();
							break;
						case AssetFileStrageType.StreamingAssets:
							if( Application.platform == RuntimePlatform.Android )
							{
								url = System.IO.Path.Combine( Application.streamingAssetsPath, FilePath );
							}
							else
							{
								url = "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, FilePath);
							}
							break;
						default:
							url = FilePath;
							break;
					};

					return UtageToolKit.EncodeUrl(url);
				}
			}
		}

		//ダウンロードするURL
		string ToDownloadUrl()
		{
			if (FileType == AssetFileType.Sound)
			{
				//サウンドだけタイムスタンプ設定が効かない
				return FilePath;
			}
			else
			{
				//キャッシュクリアのため、タイムスタンプを設定
				string tempurl = string.Format(
					"{0}?datetime={1}",
					FilePath,
					System.DateTime.Now.ToFileTime()
					);
				return tempurl;
			}
		}


		/// <summary>
		/// キャッシュデータテーブルをバイナリ書き込み
		/// </summary>
		/// <param name="writer">バイナリライター</param>
		public void Write(BinaryWriter writer)
		{
			writer.Write(Key);
			writer.Write((int)FileType);
			writer.Write(cacheVersion);
			writer.Write(cachePath);
			writer.Write((int)cryptType);
		}

		/// <summary>
		/// キャッシュデータテーブルをバイナリ読み込み
		/// </summary>
		/// <param name="reader"></param>
		void Read(BinaryReader reader)
		{
			InitKey(reader.ReadString());
			this.fileType = (AssetFileType)reader.ReadInt32();
			this.cacheVersion = reader.ReadInt32();
			this.cachePath = reader.ReadString();
			this.cryptType = (AssetFileCryptType)reader.ReadInt32();
			this.strageType = AssetFileStrageType.Web;
		}
	}


	/// <summary>
	/// アセットファイル情報のDictionary
	/// </summary>
	[System.Serializable]
	public class AssetFileInfoDictionary : SerializableDictionary<AssetFileInfo>
	{
		int cacheFileID;		//最新のキャッシュファイルのID

		/// <summary>
		/// キャッシュIDを加算
		/// </summary>
		/// <returns>加算後のキャッシュID</returns>
		public int IncCacheID()
		{
			return ++cacheFileID;
		}

		//キャッシュデータを全て削除
		public void DeleteCacheAll()
		{
			foreach (AssetFileInfo fileInfo in this.List)
			{
				//キャッシュファイル削除
				fileInfo.DeleteCache();
			}
			cacheFileID = 0;
		}


		static readonly int MagicID = FileIOManagerBase.ToMagicID('C', 'a', 'c', 'h');	//識別ID
		const int Version = 1;	//キャッシュ情報のファイルバージョン

		/// <summary>
		/// キャッシュデータテーブルをバイナリ読み込み
		/// </summary>
		/// <param name="reader">バイナリリーダー</param>
		public void Read(BinaryReader reader)
		{
			int magicID = reader.ReadInt32();
			if (magicID != MagicID)
			{
				throw new System.Exception("Read File Id Error");
			}

			int fileVersion = reader.ReadInt32();
			if (fileVersion == Version)
			{
				cacheFileID = reader.ReadInt32();
				int count = reader.ReadInt32();
				for (int i = 0; i < count; i++)
				{
					Add(new AssetFileInfo(reader));
				}
			}
			else
			{
				throw new System.Exception(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.UnknownVersion, fileVersion));
			}
		}

		/// <summary>
		/// キャッシュデータテーブルをバイナリ書き込み
		/// </summary>
		/// <param name="writer">バイナリライター</param>
		public void Write(BinaryWriter writer)
		{
			writer.Write(MagicID);
			writer.Write(Version);
			writer.Write(cacheFileID);
			int cacheCount = 0;
			foreach (AssetFileInfo info in List)
			{
				if (info.ExistCahce )
				{
					++cacheCount;
				}
			}
			writer.Write(cacheCount);
			foreach (AssetFileInfo info in List)
			{
				if (info.ExistCahce )
				{
					info.Write(writer);
				}
			}
		}
	};
}