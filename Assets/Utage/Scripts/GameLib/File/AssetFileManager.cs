//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// ファイル管理
	/// </summary>
	[AddComponentMenu("Utage/Lib/File/AssetFileManager")]
	public partial class AssetFileManager : MonoBehaviour
	{
		/// <summary>
		/// 割り当てる最大メモリサイズ
		/// </summary>
		static public int MaxMemSize { get { return (int)(GetInstance().maxMemSizeMB * 1024 * 1024); } }

		/// <summary>
		/// 最適化後のメモリサイズ
		/// </summary>
		static public int OptimizedMemSize { get { return (int)GetInstance().optimizedMemSizeMB * 1024 * 1024; } }

		/// <summary>
		/// ロード済みファイル（使用中とプール中の両方）の総メモリサイズ
		/// </summary>
		static public int TotalMemSize { get { return GetInstance().totalMemSize; } }

		/// <summary>
		/// 使用中ファイルの総メモリサイズ
		/// </summary>
		static public int TotalMemSizeUsing { get { return GetInstance().totalMemSizeUsing; } }

		/// <summary>
		/// アクティブの切り替え
		/// </summary>
		/// <param name="isActive">アクティブ状態</param>
		static public void SetActive(bool isActive)
		{
			GetInstance().gameObject.SetActive(isActive);
		}

		/// <summary>
		/// エラー処理の設定
		/// </summary>
		/// <param name="callbackError">エラー時に呼ばれるコールバック</param>
		static public void InitError(Action<AssetFile> callbackError)
		{
			GetInstance().CallbackError = callbackError;
		}

		/// <summary>
		/// ファイルをリロードする
		/// </summary>
		/// <param name="file">リロードするファイル</param>
		static public void ReloadFile(AssetFile file)
		{
			GetInstance().ReloadFileSub(file as AssetFileWork);
		}

		/// <summary>
		/// ファイル情報取得
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <returns>ファイル情報</returns>
		static public AssetFile GetFileCreateIfMissing(string path)
		{

			if (!IsEditorErrorCheck)
			{
				AssetFile file = GetInstance().AddSub(path);
				return file;
			}
			else
			{
//				AssetFileWork dummy = new AssetFileWork();
				return null;
			}
		}

		/// <summary>
		/// ファイルのロード
		/// すぐ使うファイルに使用すること
		/// ロードの優先順位は一番高い
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		/// <returns>ファイル情報</returns>	
		static public AssetFile Load(string path, System.Object referenceObj)
		{
			return Load(GetInstance().AddSub(path), referenceObj);
		}
		/// <summary>
		/// ファイルのロード
		/// すぐ使うファイルに使用すること
		/// ロードの優先順位は一番高い
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		/// <returns>ファイル情報</returns>	
		static public AssetFile Load(string path, int version, System.Object referenceObj)
		{
			AssetFile file = GetInstance().AddSub(path);
			file.Version = version;
			return Load(file, referenceObj);
		}
		/// <summary>
		/// ファイルのロード
		/// すぐ使うファイルに使用すること
		/// ロードの優先順位は一番高い
		/// </summary>
		/// <param name="file">ロードするファイル</param>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		/// <returns>ファイル情報</returns>	
		static public AssetFile Load(AssetFile file, System.Object referenceObj)
		{
			return GetInstance().LoadSub(file as AssetFileWork, referenceObj);
		}

		/// <summary>
		/// ファイルの先行ロード
		/// もうすぐ使うファイルに使用すること
		/// ロードの優先順位は二番目に高い
		/// 事前にロードをかけてロード時間を短縮しておくのが主な用途
		/// </summary>
		/// <param name="path">ファイルパス</param>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		static public void Preload(string path, System.Object referenceObj)
		{
			Preload(GetInstance().AddSub(path), referenceObj);
		}

		/// <summary>
		/// ファイルの先行ロード
		/// もうすぐ使うファイルに使用すること
		/// ロードの優先順位は二番目に高い
		/// 事前にロードをかけてロード時間を短縮しておくのが主な用途
		/// </summary>
		/// <param name="file">先行ロードするファイル</param>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		static public void Preload(AssetFile file, System.Object referenceObj)
		{
			GetInstance().PreloadSub(file as AssetFileWork, referenceObj);
		}

		/// <summary>
		/// ファイルのバックグラウンドロード
		/// すぐには使わないが、そのうち使うであろうファイルに使用すること
		/// ロードの優先順位は三番目に高い
		/// 事前にロードをかけてロード時間を短縮しておくのが主な用途。
		/// </summary>
		/// <param name="file">ファイルパス</param>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		static public AssetFile BackGroundLoad(string path, System.Object referenceObj)
		{
			return BackGroundLoad(GetInstance().AddSub(path), referenceObj);
		}
		/// <summary>
		/// ファイルのバックグラウンドロード
		/// すぐには使わないが、そのうち使うであろうファイルに使用すること
		/// ロードの優先順位は三番目に高い
		/// 事前にロードをかけてロード時間を短縮しておくのが主な用途。
		/// </summary>
		/// <param name="file">バックグラウンドロードするファイル</param>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		static public AssetFile BackGroundLoad(AssetFile file, System.Object referenceObj)
		{
			return GetInstance().BackGroundLoadSub(file as AssetFileWork, referenceObj);
		}


		/// <summary>
		/// ファイルのダウンロードだけする
		/// ロードの優先順位は一番低い
		/// バックグラウンドでファイルのダウンロードをする。
		/// デバイスストレージに保存可能ならファイルをキャッシュしておく
		/// ロードしたアセットはメモリにもキャッシュにもしておくが
		/// メモリキャッシュはメモリが枯渇すると揮発するので、その場合は再ロードに時間がかかる
		/// </summary>
		/// <param name="path">パス</param>	
		static public void Download(string path)
		{
			Download(GetInstance().AddSub(path));
		}

		/// <summary>
		/// ファイルのダウンロードだけする
		/// ロードの優先順位は一番低い
		/// バックグラウンドでファイルのダウンロードをする。
		/// デバイスストレージに保存可能ならファイルをキャッシュしておく
		/// ロードしたアセットはメモリにもキャッシュにもしておくが
		/// メモリキャッシュはメモリが枯渇すると揮発するので、その場合は再ロードに時間がかかる
		/// </summary>
		/// <param name="file">ダウンロードするファイル</param>
		static public void Download(AssetFile file)
		{
			GetInstance().DownloadSub(file as AssetFileWork);
		}

		/// <summary>
		/// キャッシュファイルを削除
		/// </summary>
		/// <param name="path">削除するキャッシュファイルのパス</param>	
		static public void DeleteCacheFile(string path)
		{
			GetInstance().DeleteCacheFileSub(path);
		}

		/// <summary>
		/// 指定タイプのキャッシュファイルを全て削除
		/// </summary>
		/// <param name="type">削除するキャッシュファイルのタイプ</param>
		static public void DeleteCacheFileAll(AssetFileType type)
		{
			GetInstance().DeleteCacheFileAllSub(type);
		}

		/// <summary>
		/// キャッシュファイルを全て削除
		/// </summary>
		static public void DeleteCacheFileAll()
		{
			GetInstance().DeleteCacheFileAllSub();
		}

		/// <summary>
		/// エディタ上のエラーチェックのために起動してるか
		/// </summary>
		static public bool IsEditorErrorCheck
		{
			get { return isEditorErrorCheck; }
			set { isEditorErrorCheck = value; }
		}
		static bool isEditorErrorCheck = false;

		static AssetFileManager instance;
		static AssetFileManager GetInstance()
		{
			if (instance==null)
			{
				instance = FindObjectOfType<AssetFileManager>();
				if(instance==null)
				{
					GameObject go = new GameObject("FileManager");
					instance = go.AddComponent<AssetFileManager>();
				}
			}
			return instance;
		}

		public FileIOManager FileIOManger
		{
			get { return fileIOManger ?? ( fileIOManger = this.GetComponent<FileIOManager>()); }
			set { fileIOManger = value; }
		}
		[SerializeField]
		FileIOManager fileIOManger;


		[SerializeField]
		float timeOutDownload = 10;					//タイムアウト時間
		[SerializeField]
		int autoRetryCountOnDonwloadError = 5;		//ダウンロードエラー時に、自動でリトライする回数

		[SerializeField]
		int loadFileMax = 5;					//同時にロードするファイルの最大数
		[SerializeField]
		float maxMemSizeMB = 64;				//最大メモリサイズ
		[SerializeField]
		float optimizedMemSizeMB = 32;			//最適化後メモリサイズ

		[SerializeField]
		AssetFileStrageType defaultStrageType = AssetFileStrageType.Resources;		//ストレージタイプ

		[SerializeField]
		string cacheDirectoryName = "Cache";	//DLファイルをキャッシュするディレクトリ名
		[SerializeField]
		string cacheTblFileName = "CacheTbl";	//キャッシュしたファイル名一覧のファイル名

		//暗号化するアセットのタイプ
		[Flags]
		enum CryptAssetType
		{
			Text = 0x1,					//テキスト
			Binary = 0x2,				//バイナリ
			Texture = 0x4,				//テクスチャ
		};
		//キャッシュするファイルの暗号化の仕方
		[SerializeField]
		[EnumFlagsAttribute]
		CryptAssetType cryptType = CryptAssetType.Text | CryptAssetType.Binary | CryptAssetType.Texture;

		[SerializeField]
		string[] textureExtArray = { ".png", ".jpg" };				//テクスチャー対応拡張子一覧	
		[SerializeField]
		string[] soundExtArray = { ".mp3", ".ogg", ".wav" };		//オーディオ対応拡張子一覧
		[SerializeField]
		string[] textExtArray = { ".txt" };							//テキスト対応拡張子一覧
		[SerializeField]
		string[] csvExtArray = { ".csv", ".tsv"};					//テクスチャー対応拡張子一覧

		[SerializeField]
		bool isOutPutDebugLog = false;								//ダウンロードのログをコンソールに出力する
		[SerializeField]
		bool isDebugCacheFileName = false;							//キャッシュファイルパスをデバッグモードにする（隠蔽せずに公開する）
		[SerializeField]
		bool isDebugBootDeleteChacheTextAndBinary = false;			//起動時に、テキストやバイナリのキャッシュを削除する
		[SerializeField]
		bool isDebugBootDeleteChacheAll = false;					//起動時に、キャッシュファイルを全て消す


		int totalMemSize = 0;													//ロード済みファイル（使用中とプール中両方）の総メモリサイズ
		int totalMemSizeUsing = 0;												//使用中ファイルの総メモリサイズ
		List<AssetFileWork> loadingFileList = new List<AssetFileWork>();		//ロード中ファイルリスト
		List<AssetFileWork> loadWaitFileList = new List<AssetFileWork>();		//ロード待ちファイルリスト
		List<AssetFileWork> usingFileList = new List<AssetFileWork>();			//使用中ファイルリスト
		List<AssetFileWork> unuesdFileList = new List<AssetFileWork>();			//未使用（ロード済みでオンメモリ）ファイルリスト
		AssetFileDictionary fileTbl = new AssetFileDictionary();				//管理中のファイルリスト
		AssetFileInfoDictionary fileInfoTbl = new AssetFileInfoDictionary();	//ファイル情報リスト

		Action<AssetFile> callbackError;

		public Action<AssetFile> CallbackError
		{
			get { return callbackError ??( callbackError = CallbackFileLoadError); }
			set { callbackError = value; }
		}

		AssetFile errorFile;		// ロードエラーしたファイル

		// ロードエラー時のデフォルトコールバック
		void CallbackFileLoadError(AssetFile file)
		{
			errorFile = file;
			string errorMsg = file.LoadErrorMsg + "\n" + file.FileName;
			DebugLog(errorMsg);

			if (SystemUi.GetInstance() != null)
			{
				//リロードを促すダイアログを表示
				SystemUi.GetInstance().OpenDialog1Button(
					errorMsg, LanguageSystemText.LocalizeText(SystemText.Retry),
					this.gameObject, "OnCloseFileLoadErrorDialog");
				AssetFileManager.SetActive(false);
			}
			else
			{
				AssetFileManager.ReloadFile(errorFile);
			}
		}

		// ロードエラーダイアログが閉じられたとき
		void OnCloseFileLoadErrorDialog()
		{
			AssetFileManager.SetActive(true);
			AssetFileManager.ReloadFile(errorFile);
		}



		void Awake()
		{
			if (null == instance)
			{
				instance = this;
			}
			else
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.SingletonError));
				Destroy(this);
			}

			// リソースをweb版にすると毎回キャッシュ読み込みのエラーが出るので基本毎回作り直しさせる
			DeleteCacheFileAllSub();

			// 消す設定とかも読み込んだあとにするとかもう意味わかんねぇな
			ReadCacheTbl();
			if (isDebugBootDeleteChacheAll)
			{
				DeleteCacheFileAllSub();
			}
			else if (isDebugBootDeleteChacheTextAndBinary)
			{
				DeleteCacheFileAllSub(AssetFileType.Text);
				DeleteCacheFileAllSub(AssetFileType.Bytes);
				DeleteCacheFileAllSub(AssetFileType.Csv);
			}
		}

		void Update()
		{
			MemoryOptimize();
		}

		void LastUpdate()
		{
			RefleshUnuseList();
		}

		//	最新バージョン番号でファイル情報を設定
		AssetFileInfo SetFileVersionSub(string path, int version)
		{
			AssetFileInfo fileInfo = GetFileInfoCreateIfMissing(path);
			fileInfo.Version = version;
			return fileInfo;
		}

		//	ファイル情報を追加
		AssetFileInfo GetFileInfoCreateIfMissing(string path)
		{
			AssetFileInfo fileInfo;
			if (fileInfoTbl.TryGetValue(path, out fileInfo))
			{
				//既にある
				return fileInfo;
			}
			else
			{
				if(string.IsNullOrEmpty(path))
				{
					Debug.LogError(path);
				}
				fileInfo = CreateFileInfo(path);
				fileInfoTbl.Add(fileInfo);
				return fileInfo;
			}
		}

		/// <summary>
		/// ファイル情報を作成
		/// </summary>
		AssetFileInfo CreateFileInfo( string path ){
			AssetFileType fileType = PraseFileType(path);
			AssetFileStrageType strageType = PraseStrageType(path);
			bool isCrypt = CheckCrypt(fileType, cryptType);
			return new AssetFileInfo(path, fileType, strageType, isCrypt);
		}

		//ファイルタイプを解析
		AssetFileType PraseFileType(string path)
		{
			string ext = System.IO.Path.GetExtension(path).ToLower();
			if (Array.Exists(textureExtArray, s => (s == ext)))
			{
				return AssetFileType.Texture;
			}
			else if (Array.Exists(soundExtArray, s => (s == ext)))
			{
				return AssetFileType.Sound;
			}
			else if (Array.Exists(textExtArray, s => (s == ext)))
			{
				return AssetFileType.Text;
			}
			else if (Array.Exists(csvExtArray, s => (s == ext)))
			{
				return AssetFileType.Csv;
			}
			else
			{
				return AssetFileType.Bytes;
			}
		}

		//ストレージタイプを解析
		AssetFileStrageType PraseStrageType(string path)
		{
			if (UtageToolKit.IsAbsoluteUri(path))
			{
#if UNITY_WEBPLAYER
				return AssetFileStrageType.WebNocache;
#else
				return AssetFileStrageType.Web;
#endif
			}
			else
			{
				return defaultStrageType;
			}
		}

		//暗号化するかのチェック
		bool CheckCrypt(AssetFileType type, CryptAssetType cryptType)
		{
			switch (type)
			{
				case AssetFileType.Bytes:
					return (cryptType & CryptAssetType.Binary) == CryptAssetType.Binary;
				case AssetFileType.Text:
				case AssetFileType.Csv:
					return (cryptType & CryptAssetType.Text) == CryptAssetType.Text;
				case AssetFileType.Texture:
					return (cryptType & CryptAssetType.Texture) == CryptAssetType.Texture;
				default:
					return false;
			}
		}

		// 管理ファイルを追加
		AssetFileWork AddSub(string path)
		{
			AssetFileWork file;
			if (fileTbl.TryGetValue(path, out file))
			{
				//既にマネージャーの管理下にある
				return file;
			}
			else
			{
				file = new AssetFileWork(GetFileInfoCreateIfMissing(path), FileIOManger);
				fileTbl.Add(file);
				return file;
			}
		}

		// ダウンロード
		void DownloadSub(AssetFileWork file)
		{
			if (file.FileInfo.IsWriteNewCache )
			{
				if (file.ReadyToLoad(AssetFileWork.LoadPriority.DownloadOnly, null))
				{
					RefleshMemSize();
					return;
				}
				AddLoadFile(file);
			}
		}
		// プリロード
		void PreloadSub(AssetFileWork file, System.Object referenceObj)
		{
			MoveToUseList(file);
			if (file.ReadyToLoad(AssetFileWork.LoadPriority.Preload, referenceObj))
			{
				RefleshMemSize();
				return;
			}
			AddLoadFile(file);
		}
		// ロード
		AssetFileWork LoadSub(AssetFileWork file, System.Object referenceObj)
		{
			MoveToUseList(file);
			if (file.ReadyToLoad(AssetFileWork.LoadPriority.Default, referenceObj))
			{
				RefleshMemSize();
				return file;
			}
			AddLoadFile(file);
			return file;
		}
		//	ファイルのバックグランドロード
		AssetFileWork BackGroundLoadSub(AssetFileWork file, System.Object referenceObj)
		{
			MoveToUseList(file);
			if (file.ReadyToLoad(AssetFileWork.LoadPriority.BackGround, referenceObj))
			{
				RefleshMemSize();
				return file;
			}
			AddLoadFile(file);
			return file;
		}

		//ロード待ちリストを追加
		void AddLoadFile(AssetFileWork file)
		{
			if (!LoadFile(file))
			{
				loadWaitFileList.Add(file);
			}
		}

		//ファイルロード開始（ファイルロード数が上限を超えていたら失敗）
		bool LoadFile(AssetFileWork file)
		{
			if (loadingFileList.Count < loadFileMax)
			{
				if (loadingFileList.Contains(file))
				{
					Debug.LogError(file.Key + " is already loaded");
				}
				loadingFileList.Add(file);
				DebugLog("Load Start :" + file.FileName + " ver:" + file.FileInfo.Version + " cache:" + file.FileInfo.CacheVersion);
				StartCoroutine(CoLoadFile(file));
				return true;
			}
			else
			{
				return false;
			}
		}
		IEnumerator CoLoadFile(AssetFileWork file)
		{
			yield return StartCoroutine(file.CoLoadAsync(timeOutDownload));

			if (!file.IsLoadError)
			{
				//新たにキャッシュファイル書きこむ必要がある場合
				if (file.FileInfo.IsWriteNewCache )
				{
					//ロード成功
					DebugLog("WriteCacheFile:" + file.FileName + " ver:" + file.FileInfo.Version + " cache:" + file.FileInfo.CacheVersion);
					WriteNewVersion(file);
				}
				//ロード終了処理
				file.LoadComplete();

				//再ロード必要
				if (file.IsLoadRetry)
				{
					DebugLog("IsLoadRetry");
					StartCoroutine(CoLoadFile(file));
				}
				else
				{
					//ロード成功
					DebugLog("Load End :" + file.FileName + " ver:" + file.FileInfo.Version);

					loadingFileList.Remove(file);
					LoadNextFile();
					MemoryOptimize();
				}
			}
			else
			{
				//ロード失敗
				DebugLogError("Load Failed :" + file.FileName + " ver:" + file.FileInfo.Version + "\n" + file.LoadErrorMsg);

				//リトライ
				if (file.IncLoadErrorRetryCount() < autoRetryCountOnDonwloadError )
				{
					DebugLog("Load Retry :" + file.FileName + " ver:" + file.FileInfo.Version);
					StartCoroutine(CoLoadFile(file));
				}
				else
				{
					if (file.FileInfo.IsCaching )
					{
						//キャシュ削除してもう一度DL
						file.ResetLoadErrorRetryCount();
						DeleteCacheFileSub(file.FileName);
						StartCoroutine(CoLoadFile(file));
					}
					else
					{
						if (null != CallbackError) CallbackError(file);
					}
				}
			}
		}

		//ファイルリロード
		void ReloadFileSub(AssetFileWork file)
		{
			StartCoroutine(CoLoadFile(file));
		}



		void LoadNextFile()
		{
			AssetFileWork next = null;
			foreach (AssetFileWork file in loadWaitFileList)
			{
				if (next == null)
				{
					next = file;
				}
				else
				{
					if (file.Priority < next.Priority)
					{
						next = file;
					}
				}
			}
			if (next != null)
			{
				loadWaitFileList.Remove(next);
				AddLoadFile(next);
			}
		}

		void DebugLog(string msg)
		{
			if (isOutPutDebugLog) Debug.Log(msg);
		}
		void DebugLogError(string msg)
		{
			if (isOutPutDebugLog) Debug.LogError(msg);
		}


		//キャッシュファイル書き込み
		void WriteNewVersion(AssetFileWork file)
		{
			//キャッシュファイル書き込み準備
			file.FileInfo.ReadyToWriteCache(fileInfoTbl.IncCacheID(), GetCacheRootDir(), isDebugCacheFileName);
			string cachePath = file.FileInfo.CachePath;

			//キャッシュ用のディレクトリがなければ作成
			FileIOManger.CreateDirectory(cachePath);

			//ファイル書き込み
			bool ret = false;
			if (file.FileInfo.IsCrypt)
			{
				switch (file.FileInfo.FileType)
				{
					case AssetFileType.Sound:
						ret = FileIOManger.WriteSound(cachePath, file.WriteCacheFileSound);
						break;
					case AssetFileType.Texture:
						ret = FileIOManger.WriteEncodeNoCompress(cachePath, file.CacheWriteBytes);
						break;
					default:
						ret = FileIOManger.WriteEncode(cachePath, file.CacheWriteBytes);
						break;
				}
			}
			else
			{
				ret = FileIOManger.Write(cachePath, file.CacheWriteBytes);
			}

#if UNITY_IPHONE
			iPhone.SetNoBackupFlag(cachePath);
#endif

			//キャッシュファイルテーブルを更新して上書き
			if (!ret)
			{
				DebugLogError("Write Failed :" + file.FileInfo.CachePath);
			}
			else
			{
				WriteCacheTbl();
				file.FileInfo.DeleteOldCacheFile();
			}
		}

		/// キャッシュファイルのルートディレクトリ
		string GetCacheRootDir()
		{
			return FileIOManager.SdkTemporaryCachePath + cacheDirectoryName + "/";
		}
		/// キャッシュファイル管理テーブルのファイルパス
		string GetCacheTblPath()
		{
			return FileIOManager.SdkTemporaryCachePath + cacheTblFileName;
		}


		//ファイルを使用中リストに
		void MoveToUseList(AssetFileWork work)
		{
			if (!usingFileList.Contains(work))
			{
				usingFileList.Add(work);
			}
			if (unuesdFileList.Contains(work))
			{
				unuesdFileList.Remove(work);
			}
			RefleshMemSize();
		}

		/**
			移動っぽいけど全消し
		*/
		void moveToUnuseListAll(){
			if( usingFileList != null ){
				foreach( AssetFileWork data in usingFileList ){
					unuesdFileList.Add( data );
					usingFileList.Remove(data);
				}
			}
			RefleshMemSize();
		}


		//ファイルの使用・未使用リストを更新
		List<AssetFileWork> tmpList = new List<AssetFileWork>();
		void RefleshUnuseList()
		{
			tmpList.Clear();
			foreach (AssetFileWork file in usingFileList)
			{
				if (file.CheckUnuse())
				{
					tmpList.Add(file);
				}
			}
			if (tmpList.Count > 0)
			{
				foreach (AssetFileWork file in tmpList)
				{
					usingFileList.Remove(file);
					unuesdFileList.Add(file);
				}
				tmpList.Clear();
				RefleshMemSize();
			}
		}

		//確保メモリ数を再計算
		void RefleshMemSize()
		{
			totalMemSize = 0;
			totalMemSizeUsing = 0;
			foreach (AssetFileWork file in usingFileList)
			{
				totalMemSizeUsing += file.MemSize;
				totalMemSize += file.MemSize;
			}
			foreach (AssetFileWork file in unuesdFileList)
			{
				totalMemSize += file.MemSize;
			}
		}

		//メモリの最適化
		void MemoryOptimize()
		{
			RefleshUnuseList();
			//確保メモリが上限を超えていたら、キャッシュメモリを消去
			if (totalMemSize > MaxMemSize)
			{
				UnloadChaceFile(totalMemSize - OptimizedMemSize);
				RefleshMemSize();
			}
		}

		//システムメモリにキャッシュされてるファイルをいったんアンロードして、メモリを解放
		void UnloadChaceFile(int unloadSize)
		{
			//未使用ファイルの消去優先順でソート
			unuesdFileList.Sort((a, b) => a.UnusedSortID - b.UnusedSortID);

			//指定サイズだけアンロード
			int count = 0;
			int size = 0;
			foreach (AssetFileWork file in unuesdFileList)
			{
				DebugLog("Unload " + file.FileName + " ver:" + file.FileInfo.Version);
				file.Unload();
				++count;
				size += file.MemSize;
				if (size >= unloadSize)
				{
					break;
				}
			}
			unuesdFileList.RemoveRange(0, count);
		}

		//キャッシュファイルテーブルを読み込み
		void ReadCacheTbl()
		{
#if !UNITY_WEBPLAYER
			fileInfoTbl = new AssetFileInfoDictionary();
			if (!FileIOManger.ReadBinaryDecode(GetCacheTblPath(), fileInfoTbl.Read))
			{
				fileInfoTbl.Clear();
			}
#endif
		}

		//キャッシュファイルテーブルを保存
		void WriteCacheTbl()
		{
#if !UNITY_WEBPLAYER
			//キャッシュ用のディレクトリがなければ作成
			string path = GetCacheTblPath();

			FileIOManger.CreateDirectory(path);

			//Debug.Log( path );

			FileIOManger.WriteBinaryEncode(path, fileInfoTbl.Write);

#if UNITY_IPHONE
			iPhone.SetNoBackupFlag(path);
#endif

#endif
		}

		//	キャッシュファイルを削除
		void DeleteCacheFileSub(string path)
		{
			AssetFileInfo fileInfo;
			if (fileInfoTbl.TryGetValue(path, out fileInfo))
			{
				//キャッシュファイル削除
				fileInfo.DeleteCache();
				fileInfoTbl.Remove(path);
			}
			WriteCacheTbl();
		}
		//	指定タイプのキャッシュファイルを全て削除
		void DeleteCacheFileAllSub(AssetFileType type)
		{
			List<string> removeFile = new List<string>();
			foreach ( string key in fileInfoTbl.Keys)
			{
				AssetFileInfo fileInfo = fileInfoTbl.GetValue(key);
				if (fileInfo.FileType == type)
				{
					removeFile.Add(key);
				}
			}
			foreach ( string key in removeFile)
			{
				AssetFileInfo fileInfo = fileInfoTbl.GetValue(key);
				//キャッシュファイル削除
				fileInfo.DeleteCache();
				fileInfoTbl.Remove(key);
			}
			WriteCacheTbl();
		}
		//	キャッシュファイルを全て削除
		void DeleteCacheFileAllSub()
		{
			DeleteCacheFileAllSub(AssetFileType.Text);
			DeleteCacheFileAllSub(AssetFileType.Bytes);
			DeleteCacheFileAllSub(AssetFileType.Csv);

			FileIOManger.Delete( GetCacheTblPath());

			fileInfoTbl.DeleteCacheAll();
			fileInfoTbl.Clear();
			fileTbl.Clear();
			WriteCacheTbl();

			//Debug.Log( "here001");

		}
	}
}