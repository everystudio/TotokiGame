//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utage
{


	/// <summary>
	/// ファイルの実態。システムから使う想定なので、外からは使用しないこと
	/// </summary>
	[System.Serializable]
	public class AssetFileWork : SerializableDictionaryKeyValue, AssetFile
	{
		/// <summary>
		/// ファイルパス
		/// </summary>
		public string FileName { get { return this.Key; } }


		/// <summary>
		/// ファイルの情報
		/// </summary>
		public AssetFileInfo FileInfo { get { return this.fileInfo; } }
		AssetFileInfo fileInfo;

		/// <summary>
		/// ファイルタイプ
		/// </summary>
		public AssetFileType FileType { get { return FileInfo.FileType; } }

		/// <summary>
		/// ロードの優先順
		/// </summary>
		public enum LoadPriority
		{
			Default,				//通常
			Preload,				//先読み
			BackGround,				//バックグラウンドでのロード
			DownloadOnly,			//ダウンロードのみ
		};

		/// <summary>
		/// ロードの優先順
		/// </summary>
		public LoadPriority Priority { get { return this.priority; } }
		LoadPriority priority;


		//状態
		enum STAUS
		{
			LOAD_WAIT,	//待機中
			LOADING,	//ロード中
			LOAD_END,	//ロード終了
			USING,		//アセットは使用中
			UNUSED,		//アセットは未使用
		};
		STAUS status;

		//ストリーミングの状態
		enum LOAD_STREAMING_STAUS
		{
			NONE,		//ストリーミングしない
			LOADING,	//ロード中
			READY,		//ストリーミング再生可能状態
			LOADEND,	//ストリーミングロード終了
			DONE,		//wwwのロードが完全に終わった（ストリーミングできない小さい素材なので通常ロードする）
		};
		LOAD_STREAMING_STAUS streamingStatus;

		/// <summary>
		/// メモリサイズ（正確な値ではなく目安）
		/// </summary>
		public int MemSize { get { return this.memSize; } }
		int memSize = 0;

		/// <summary>
		/// ロード終了したか
		/// </summary>
		public bool IsLoadEnd
		{
			get
			{
				bool ret = (status != STAUS.LOAD_WAIT) && (status != STAUS.LOADING);
				return ret;
			}
		}

		/// <summary>
		/// 再度ロード（エラー時のリトライとは別）
		/// </summary>
		public bool IsLoadRetry { get { return this.isLoadRetry; } }
		bool isLoadRetry = false;

		/// <summary>
		/// ロードエラーしたか
		/// </summary>
		public bool IsLoadError { get { return this.isLoadError; } }
		bool isLoadError = false;

		/// <summary>
		/// ロードエラー時のエラーメッセージ
		/// </summary>
		public string LoadErrorMsg { get { return this.loadErrorMsg; } }
		string loadErrorMsg = "";

		/// <summary>
		/// ロードエラー時のリトライ回数を加算
		/// </summary>
		/// <returns>加算後のリトライ回数</returns>
		public int IncLoadErrorRetryCount()
		{
			return ++loadErrorRetryCount;
		}

		/// <summary>
		/// ロードエラー時のリトライ回数をリセット
		/// </summary>
		public void ResetLoadErrorRetryCount()
		{
			loadErrorRetryCount = 0; ;
		}
		int loadErrorRetryCount = 0;

		/// <summary>
		/// ストリーム再生ができるか
		/// </summary>
		public bool IsReadyStreaming { get { return (streamingStatus == LOAD_STREAMING_STAUS.READY) || IsLoadEnd; } }

		/// <summary>
		/// 新たなキャッシュファイルを書き込むときのバイナリ
		/// </summary>
		public byte[] CacheWriteBytes { get { return this.cacheWriteBytes; } }
		byte[] cacheWriteBytes;

		///ダウンロードのみか
		bool IsDownloadOnly { get { return priority == LoadPriority.DownloadOnly; } }

		//ファイルIOシステム
		FileIOManagerBase fileIO;

		//参照オブジェクト
		HashSet<System.Object> referenceSet = new HashSet<object>();

		//未使用リソースのソートID
		static int sCommonUnusedSortID = 0;
		int unusedSortID;
		public int UnusedSortID { get { return unusedSortID; } }

		float elapsedTime = 0.0f;	//DL経過時間
		float lastProgress = 0;		//前回のDL進行度

		/// <summary>
		/// 実際にロード中のパス
		/// </summary>
		public string LoadingPath { get { return this.loadingPath; } }
		string loadingPath;

		/// <summary>
		/// ロードしたテキスト
		/// </summary>
		public string Text { get { return IsReadyToUse() ? this.text : null; } }
		string text;

		/// <summary>
		/// ロードしたバイナリ
		/// </summary>
		public byte[] Bytes { get { return IsReadyToUse() ? this.bytes : null; } }
		byte[] bytes;

		/// <summary>
		/// ロードしたテクスチャ
		/// </summary>
		public Texture2D Texture { get { return IsReadyToUse() ? this.texture : null; } }
		Texture2D texture;

		/// <summary>
		/// ロードしたサウンド
		/// </summary>
		public AudioClip Sound { get { return IsReadyToUse() ? this.sound : null; } }
		AudioClip sound;
		public AudioClip WriteCacheFileSound { get { return this.sound; } }

		/// <summary>
		/// ロードしたCSV
		/// </summary>
		public StringGrid Csv { get { return IsReadyToUse() ? this.csv : null; } }
		StringGrid csv;

		/// <summary>
		/// スプライトの作成に必要な情報
		/// </summary>
		public AssetFileSpriteInfo SpriteInfo
		{
			get { return spriteInfo; }
			set { spriteInfo = value;}
		}
		AssetFileSpriteInfo spriteInfo = new AssetFileSpriteInfo();


		/// <summary>
		/// ロードしたテクスチャから作ったスプライト
		/// </summary>
		/// <param name="pixelsToUnits">スプライトを作成する際の、座標1.0単位辺りのピクセル数</param>
		/// <returns>作成したスプライト</returns>
		public Sprite GetSprite(float pixelsToUnits)
		{
			if (sprite == null)
			{
				if(spriteInfo.scale!=0)
				{
					pixelsToUnits/=spriteInfo.scale;
				}
				sprite = UtageToolKit.CreateSprite(this.Texture, pixelsToUnits, spriteInfo.pivot);
			}
			return sprite;
		}
		Sprite sprite;

		/// <summary>
		/// バージョン
		/// </summary>
		public int Version
		{
			get { return FileInfo.Version; }
			set
			{
				if (FileInfo.Version != value)
				{
					if (status != STAUS.LOAD_WAIT)
					{
						Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.DisableChangeFileVersion));
					}
					else
					{
						FileInfo.Version = value;
					}
				}
			}
		}

		/// <summary>
		/// キャッシュファイルのバージョン
		/// </summary>
		public int CacheVersion
		{
			get { return FileInfo.CacheVersion; }
		}

		/// <summary>
		/// ロードフラグ
		/// </summary>
		public AssetFileLoadFlags LoadFlags
		{
			get { return FileInfo.LoadFlags; }
		}
		/// <summary>
		/// ロードフラグを追加
		/// </summary>
		public void AddLoadFlag(AssetFileLoadFlags flags)
		{
			if( (FileInfo.LoadFlags & flags) == AssetFileLoadFlags.None ) return;

			//ロードフラグの反映
			if (status != STAUS.LOAD_WAIT)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.DisableChangeFileLoadFlag));
			}
			else
			{
				FileInfo.AddLoadFlag( flags );
			}
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="info">ファイル情報</param>
		/// <param name="fileIO">ファイルのIO管理クラス</param>
		public AssetFileWork(AssetFileInfo info, FileIOManagerBase fileIO)
		{
			this.InitKey(info.Key);
			this.fileInfo = info;
			this.fileIO = fileIO;
			this.status = STAUS.LOAD_WAIT;
			this.priority = LoadPriority.DownloadOnly;
		}

		/// <summary>
		/// ロードの準備開始
		/// </summary>
		/// <param name="loadPriority">ロードの優先順</param>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		/// <returns></returns>
		public bool ReadyToLoad(LoadPriority loadPriority, System.Object referenceObj)
		{

			//ロードプライオリティの反映
			if (loadPriority < this.priority)
			{
				this.priority = loadPriority;
			}

			Use(referenceObj);
			unusedSortID = Int32.MaxValue;

			//通常ロード
			switch (status)
			{
				case STAUS.LOAD_WAIT:
					status = STAUS.LOADING;
					return false;
				case STAUS.LOADING:
				case STAUS.LOAD_END:
					return true;
				case STAUS.USING:
				case STAUS.UNUSED:
				default:
					status = STAUS.USING;
					return true;
			}
		}

		/// <summary>
		/// ロード処理
		/// </summary>
		/// <param name="timeOutDownload">ダウンロードのタイムアウトエラー時間</param>
		/// <returns></returns>
		public IEnumerator CoLoadAsync(float timeOutDownload)
		{
			this.status = STAUS.LOADING;
			this.isLoadRetry = false;
			this.isLoadError = false;
			this.streamingStatus = LOAD_STREAMING_STAUS.NONE;
			this.loadingPath = FileInfo.LoadWWWPath;

			if (FileInfo.StrageType == AssetFileStrageType.Resources)
			{
				LoadResource(loadingPath);
			}
			else
			{
				elapsedTime = 0.0f;
				lastProgress = 0;

				WWW www = new WWW(loadingPath);
				if (www == null)
				{
					SetLoadError("Not Found");
					yield break;
				}

				//WWWでダウンロード
				using (www)
				{
					//ストリーミング再生でキャッシュへの書き込みが必要ない場合は、先にストリーミング用のサウンドを作成
					if (FileInfo.IsStreamingType && !FileInfo.IsWriteNewCache )
					{
						streamingStatus = LOAD_STREAMING_STAUS.LOADING;
						sound = www.GetAudioClip(FileInfo.IsAudio3D, true, FileInfo.AudioType );
						sound.name = FileInfo.FilePath;
					}

					//ロード待ち
					while (!www.isDone && string.IsNullOrEmpty(www.error)
						&& !CheckSoundStreamReady(www)
						&& !CheckDownloadTimeout(www, timeOutDownload))
					{
						UpdateLoadPirority(www);
						yield return 0;
					}

					if (!string.IsNullOrEmpty(www.error))
					{
						//ロードエラー
						SetLoadError(www.error);
					}
					else if (CheckSoundStreamReady(www))
					{
						//ストリーミングのみ独自処理
						streamingStatus = LOAD_STREAMING_STAUS.READY;

						//その後のロード待ち
						while (!www.isDone && string.IsNullOrEmpty(www.error)
							&& !CheckDownloadTimeout(www, timeOutDownload))
						{
							UpdateLoadPirority(www);
							yield return 0;
						}
						if (!string.IsNullOrEmpty(www.error))
						{
							//ロードエラー
							SetLoadError(www.error);
						}
						else if (!www.isDone)
						{
							//ロードエラー（タイムアウト）
							SetLoadError("DownLoad TimeOut " + elapsedTime + "sec");
						}
						else
						{

						}
					}
					else if (!www.isDone)
					{
						//ロードエラー（タイムアウト）
						SetLoadError("DownLoad TimeOut " + elapsedTime + "sec");
					}
					else
					{
						//ロード終了

						//ストリーミングでも、完全にロードした場合はこっち来る
						if (streamingStatus != LOAD_STREAMING_STAUS.NONE) streamingStatus = LOAD_STREAMING_STAUS.DONE;

						if (IsDownloadOnly)
						{
							//ダウンロードするだけでリソースをめもりには読み込まない
							//ダウンロード = キャッシュファイルに書き込むなので,ここでバイナリを記録しておく
							cacheWriteBytes = www.bytes;
						}
						else if (FileInfo.IsCaching && FileInfo.IsCrypt)
						{
							//暗号化されているファイルをキャッシュファイルからロード
							if (null != www.bytes)
							{
								LoadWWWCriptCacheFile(www);
							}
							else
							{
								SetLoadError("CaecheFile Read Error " + FileInfo.CachePath);
							}
						}
						else
						{
							//通常のファイルロード
							LoadWWWNormal(www);
						}
					}
				}
			}
		}

		//ロードエラーを設定
		void SetLoadError(string errorMsg)
		{
			loadErrorMsg = errorMsg;
			isLoadError = true;
		}

		//ロードのタイムアウトをチェック
		bool CheckDownloadTimeout(WWW www, float timeOutDownload)
		{
			if (lastProgress == www.progress)
			{
				elapsedTime += Time.deltaTime;
				if (elapsedTime >= timeOutDownload)
				{
					return true;
				}
			}
			else
			{
				elapsedTime = 0;
				lastProgress = www.progress;
			}
			return false;
		}

		void UpdateLoadPirority(WWW www)
		{
			switch (Priority)
			{
				case LoadPriority.Default:
					www.threadPriority = ThreadPriority.High;
					break;
				case LoadPriority.Preload:
					www.threadPriority = ThreadPriority.Normal;
					break;
				case LoadPriority.BackGround:
					www.threadPriority = ThreadPriority.BelowNormal;
					break;
				case LoadPriority.DownloadOnly:
				default:
					www.threadPriority = ThreadPriority.Low;
					break;
			}
		}

		//ストリーミングタイプのサウンドだった場合、利用可能になったか
		bool CheckSoundStreamReady(WWW www)
		{
			if (null == sound)
			{
				return false;
			}
			else
			{
				return sound.isReadyToPlay;
			}
		}

		//ロード処理（通常）
		void LoadWWWNormal(WWW www)
		{
			if (FileInfo.IsStreamingType && FileInfo.IsWriteNewCache )
			{
				//ストリーミング再生かつ、キャッシュに書き込む必要がある場合は、まだリソースを読まない
				//書き込み後の再読み込みを発行する
				isLoadRetry = true;
			}
			else
			{
				switch (FileType)
				{
					case AssetFileType.Text:		//テキスト
						text = www.text;
						break;
					case AssetFileType.Bytes:		//バイナリ
						bytes = www.bytes;
						break;
					case AssetFileType.Texture:		//テクスチャ
						if (FileInfo.IsTextureMipmap)
						{
							//サイズとTextureFormatはLoadImage後無視される。ミップマップの反映のみ
							texture = new Texture2D(1, 1, TextureFormat.ARGB32, FileInfo.IsTextureMipmap);
							texture.LoadImage(www.bytes);
							texture.Apply(false,true);
						}
						else
						{
							texture = www.textureNonReadable;
						}
						texture.name = FileInfo.FilePath;
						texture.wrapMode = TextureWrapMode.Clamp;
						break;
					case AssetFileType.Sound:				//サウンド
						if (sound != null)
						{
							//ロードできていないストリーミング用のサウンドを消す
							UnityEngine.Object.Destroy(sound);
						}
						//非ストリーミングでオンメモリでロード
						sound = www.GetAudioClip(FileInfo.IsAudio3D, false, FileInfo.AudioType);
						sound.name = FileInfo.FilePath;
						break;
					case AssetFileType.Csv:			//CSV
						csv = new StringGrid(this.LoadingPath, FileInfo.IsCsv ? CsvType.Csv : CsvType.Tsv, www.text);
						break;
					default:
						break;
				}
			}

			//新たにキャッシュファイルとして書き込む必要がある場合は、バイナリを取得しておく
			if (FileInfo.IsWriteNewCache )
			{
				if ((FileInfo.FileType != AssetFileType.Sound) || !FileInfo.IsCrypt)
				{
					cacheWriteBytes = www.bytes;
				}
			}
		}
		//ロード処理（暗号化済みキャッシュファイルから）
		void LoadWWWCriptCacheFile(WWW www)
		{
#if !UNITY_WEBPLAYER
			byte[] readBytes = www.bytes;
			switch (FileType)
			{
				case AssetFileType.Text:		//テキスト
					text = System.Text.Encoding.Unicode.GetString(fileIO.Decode(readBytes));
					break;
				case AssetFileType.Bytes:		//バイナリ
					bytes = fileIO.Decode(readBytes);
					break;
				case AssetFileType.Texture:	//テクスチャ
					fileIO.DecodeNoCompress(readBytes);			//圧縮なしでデコード
					//サイズとTextureFormatはLoadImage後無視される。ミップマップの反映のみ
					texture = new Texture2D(1, 1, TextureFormat.ARGB32, FileInfo.IsTextureMipmap);
					texture.LoadImage(readBytes);
					texture.name = FileInfo.FilePath;
					texture.wrapMode = TextureWrapMode.Clamp;
					texture.Apply(false, true);
					break;
				case AssetFileType.Sound:		//サウンド
					fileIO.DecodeNoCompress(readBytes);			//圧縮なしでデコード
					sound = FileIOManagerBase.ReadAudioFromMem(this.Key, readBytes);
					sound.name = FileInfo.FilePath;
					break;
				case AssetFileType.Csv:			//CSV
					csv = new StringGrid(this.LoadingPath, FileInfo.IsCsv ? CsvType.Csv : CsvType.Tsv, System.Text.Encoding.UTF8.GetString(fileIO.Decode(readBytes)) );
					break;
				default:
					break;
			}
#endif
		}

		//ロード処理（Resourcesから）
		void LoadResource(string loadPath)
		{
			loadPath = Path.GetDirectoryName(loadPath) + "/" + Path.GetFileNameWithoutExtension(loadPath);
			if (loadPath[0] == '/')
			{
				loadPath = loadPath.Substring(1);
			}
			TextAsset textAsset;
			switch (FileType)
			{
				case AssetFileType.Text:		//テキスト
					textAsset = Resources.Load(loadPath, typeof(TextAsset)) as TextAsset;
					if (null != textAsset)
					{
						text = textAsset.text;
						Resources.UnloadAsset(textAsset);
					}
					else
					{
						SetLoadError("LoadResource Error");
					}
					break;
				case AssetFileType.Bytes:		//バイナリ
					textAsset = Resources.Load(loadPath, typeof(TextAsset)) as TextAsset;
					if (null != textAsset)
					{
						bytes = textAsset.bytes;
						Resources.UnloadAsset(textAsset);
					}
					else
					{
						SetLoadError("LoadResource Error");
					}
					break;
				case AssetFileType.Texture:		//テクスチャ
					texture = Resources.Load(loadPath, typeof(Texture2D)) as Texture2D;
					if (null == texture)
					{
						SetLoadError("LoadResource Error");
					}
					break;
				case AssetFileType.Sound:		//サウンド
					sound = Resources.Load(loadPath, typeof(AudioClip)) as AudioClip;
					if (null == sound)
					{
						SetLoadError("LoadResource Error");
					}
					break;
				case AssetFileType.Csv:			//CSV
					textAsset = Resources.Load(loadPath, typeof(TextAsset)) as TextAsset;
					if (null != textAsset)
					{
						csv = new StringGrid(loadPath, FileInfo.IsCsv ? CsvType.Csv : CsvType.Tsv, textAsset.text);
						Resources.UnloadAsset(textAsset);
					}
					else
					{
						SetLoadError("LoadResource Error");
					}
					break;
				default:
					break;
			}
		}

		//メモリサイズを計算
		void InitMemsSize()
		{
			switch (FileType)
			{
				case AssetFileType.Text:		//テキスト
					memSize = text.Length * 2;
					break;
				case AssetFileType.Bytes:		//バイナリ
					memSize = bytes.Length;
					break;
				case AssetFileType.Texture:		//テクスチャ
					memSize = Mathf.NextPowerOfTwo(texture.width) * Mathf.NextPowerOfTwo(texture.height)*4;
					break;
				case AssetFileType.Sound:		//サウンド
					if (streamingStatus == LOAD_STREAMING_STAUS.READY)
					{
						memSize = 1024 * 128;	//適当なある程度の値
					}
					else
					{
						memSize = sound.samples * sound.channels * 4;
					}
					break;
				case AssetFileType.Csv:			//CSV
					memSize = csv.TextLength * 2;
					break;
				default:
					break;
			}
		}



		/// <summary>
		/// ロードの完了処理
		/// </summary>
		public void LoadComplete()
		{
			cacheWriteBytes = null;
			if (IsDownloadOnly)
			{
				status = STAUS.LOAD_WAIT;
			}
			else
			{
				//			status = STAUS.LOAD_END;
				status = STAUS.USING;
				//メモリサイズを計算
				if (!IsLoadRetry) InitMemsSize();
				streamingStatus = LOAD_STREAMING_STAUS.LOADEND;
			}
		}

		/// <summary>
		/// そのオブジェクトで使用する（参照を設定する）
		/// </summary>
		/// <param name="referenceObj">ファイルを参照するオブジェクト</param>
		public void Use(System.Object referenceObj)
		{
			if (null != referenceObj)
			{
				referenceSet.Add(referenceObj);
			}
		}

		/// <summary>
		/// そのオブジェクトから未使用にする（参照を解放する）
		/// </summary>
		/// <param name="referenceObj">ファイルの参照を解除するオブジェクト</param>
		public void Unuse(System.Object referenceObj)
		{
			if (null != referenceObj)
			{
				referenceSet.Remove(referenceObj);
			}
		}

		/// <summary>
		/// 参照用コンポーネントの追加
		/// </summary>
		/// <param name="go">参照コンポーネントを追加するGameObject</param>
		public void AddReferenceComponet(GameObject go)
		{
			AssetFileReference fileReference = go.AddComponent<AssetFileReference>();
			fileReference.Init(this);
		}


		bool IsReadyToUse()
		{
			return (status == STAUS.USING);
		}

		void ErrorCheckReadyToUse()
		{
			if (!IsReadyToUse())
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.FileIsNotReady));
			}
		}

		/// <summary>
		/// 使っていないか（アンロード可能か）チェック
		/// </summary>
		/// <returns>使っていないならtrue。まだ使っているならfalse</returns>
		public bool CheckUnuse()
		{

			if (referenceSet.RemoveWhere(s => s == null) > 0)
			{
				Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.FileReferecedIsNull));
			}
			if (status == STAUS.USING)
			{
				if (referenceSet.Count <= 0)
				{
					status = STAUS.UNUSED;
					unusedSortID = sCommonUnusedSortID;
					++sCommonUnusedSortID;
				}
			}
			else if (status == STAUS.UNUSED)
			{
				if (referenceSet.Count > 0)
				{
					status = STAUS.USING;
					unusedSortID = 0;
					Debug.LogError(LanguageErrorMsg.LocalizeTextFormat(ErrorMsg.MemoryLeak));
				}
			}
			return (status == STAUS.UNUSED);
		}

		/// <summary>
		/// リソースをアンロードして、メモリを解放
		/// </summary>
		public void Unload()
		{
			if (FileInfo.StrageType == AssetFileStrageType.Resources)
			{
				UnloadResources();
			}
			else
			{
				UnloadWWW();
			}
			memSize = 0;
			priority = LoadPriority.DownloadOnly;
			status = STAUS.LOAD_WAIT;
		}

		/// <summary>
		/// リソースをアンロードして、メモリを解放(Resourcesでロードしたもの)
		/// </summary>
		void UnloadResources()
		{
			switch (FileType)
			{
				case AssetFileType.Text:		//テキスト
					text = null;
					break;
				case AssetFileType.Bytes:		//バイナリ
					bytes = null;
					break;
				case AssetFileType.Texture:		//テクスチャ
					Resources.UnloadAsset( texture );
					texture = null;
					break;
				case AssetFileType.Sound:		//サウンド
					Resources.UnloadAsset( sound );
					sound = null;
					break;
				case AssetFileType.Csv:		//CSV
					csv = null;
					break;
				default:
					break;
			}
		}


		/// <summary>
		/// リソースをアンロードして、メモリを解放(WWWでロードしたもの)
		/// </summary>
		void UnloadWWW()
		{
			switch (FileType)
			{
				case AssetFileType.Text:		//テキスト
					text = null;
					break;
				case AssetFileType.Bytes:		//バイナリ
					bytes = null;
					break;
				case AssetFileType.Texture:		//テクスチャ
					if (texture != null)
					{
						UnityEngine.Object.Destroy(texture);
						texture = null;
					}
					if (sprite != null)
					{
						UnityEngine.Object.Destroy(sprite);
						sprite = null;
					}
					break;
				case AssetFileType.Sound:		//サウンド
					if (sound != null)
					{
						UnityEngine.Object.Destroy(sound);
						sound = null;
					}
					break;
				case AssetFileType.Csv:		//CSV
					csv = null;
					break;
				default:
					break;
			}
		}
	}

	[System.Serializable]
	public class AssetFileDictionary : SerializableDictionary<AssetFileWork> { };
}
