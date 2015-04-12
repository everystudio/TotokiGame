//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System;
using System.Collections;
using UnityEngine;

namespace Utage
{


	/// <summary>
	/// メインエンジン
	/// </summary>/
	[AddComponentMenu("Utage/ADV/MainEngine")]
	[RequireComponent(typeof(DontDestoryOnLoad))]
	[RequireComponent(typeof(AdvDataManager))]
	[RequireComponent(typeof(AdvScenarioPlayer))]
	[RequireComponent(typeof(AdvPage))]
	[RequireComponent(typeof(AdvSelectionManager))]
	[RequireComponent(typeof(AdvBacklogManager))]
	[RequireComponent(typeof(AdvConfig))]	
	[RequireComponent(typeof(AdvSystemSaveData))]
	[RequireComponent(typeof(AdvSaveManager))]	
	public partial class AdvEngine : MonoBehaviour
	{
		/// <summary>
		/// 最初からはじめる場合のシナリオ名
		/// </summary>
		const string StartScenarioLabel = "Start";
		
		/// <summary>
		/// シナリオや設定等のデータ
		/// </summary>
		public AdvDataManager DataManager { get { return this.dataManager ?? (this.dataManager = GetComponent<AdvDataManager>()); } }
		AdvDataManager dataManager;

		/// <summary>
		/// シナリオの実行部分
		/// </summary>
		public AdvScenarioPlayer ScenarioPlayer { get { return this.scenarioPlayer ?? (this.scenarioPlayer = GetComponent<AdvScenarioPlayer>()); } }
		AdvScenarioPlayer scenarioPlayer;

		/// <summary>
		/// ページ情報
		/// </summary>
		public AdvPage Page { get { return this.page ?? (this.page = GetComponent<AdvPage>()); } }
		AdvPage page;


		/// <summary>
		/// 選択肢
		/// </summary>
		public AdvSelectionManager SelectionManager { get { return this.selectionManager ?? (this.selectionManager = GetComponent<AdvSelectionManager>()); } }
		AdvSelectionManager selectionManager;

		/// <summary>
		/// バックログ
		/// </summary>
		public AdvBacklogManager BacklogManager { get { return this.backlogManager ?? (this.backlogManager = GetComponent<AdvBacklogManager>()); } }
		AdvBacklogManager backlogManager;

		/// <summary>
		/// コンフィグデータ
		/// </summary>
		public AdvConfig Config { get { return this.config ?? (this.config = GetComponent<AdvConfig>()); } }
		AdvConfig config;

		/// <summary>
		/// システムセーブデータ
		/// </summary>
		public AdvSystemSaveData SystemSaveData { get { return this.systemSaveData ?? (this.systemSaveData = GetComponent<AdvSystemSaveData>()); } }
		AdvSystemSaveData systemSaveData;

		/// <summary>
		/// 通常のセーブデータ
		/// </summary>
		public AdvSaveManager SaveManager { get { return this.saveManager ?? (this.saveManager = GetComponent<AdvSaveManager>()); } }
		AdvSaveManager saveManager;

		/// <summary>
		/// 表示レイヤー管理
		/// </summary>
		public AdvLayerManager LayerManager { get { return this.layerManager ?? (this.layerManager = FindObjectOfType<AdvLayerManager>() as AdvLayerManager); } }
		[SerializeField]
		AdvLayerManager layerManager;

		/// <summary>
		/// トランジション管理
		/// </summary>
		public AdvTransitionManager TransitionManager { get { return this.transitionManager ?? (this.transitionManager = FindObjectOfType<AdvTransitionManager>()); } }
		[SerializeField]
		AdvTransitionManager transitionManager;

		/// <summary>
		/// UI管理
		/// </summary>
		public AdvUiManager UiManager { get { return this.uiManager ?? (this.uiManager = FindObjectOfType<AdvUiManager>()); } }
		[SerializeField]
		AdvUiManager uiManager;

		/// <summary>
		/// サウンドマネージャー
		/// </summary>
		public SoundManager SoundManager { get { return this.soundManger ?? (this.soundManger = FindObjectOfType<SoundManager>()); } }
		[SerializeField]
		SoundManager soundManger;

		/// <summary>
		/// パラメータ管理
		/// </summary>
		public AdvParamSetting Param { get { return this.param; } }
		[SerializeField]
		AdvParamSetting param;

		/// <summary>
		/// 起動時ロード待ちか判定
		/// </summary>
		public bool IsWaitBootLoading { get { return isWaitBootLoading; } }
		bool isWaitBootLoading = false;


		/// <summary>
		/// シーン回想を再生中か
		/// </summary>
		public bool IsSceneGallery { get { return isSceneGallery; } }
		bool isSceneGallery = false;

		/// <summary>
		/// ロード待ちか判定
		/// </summary>
		public bool IsLoading
		{
			get
			{
				if (IsWaitBootLoading) return true;

				return ScenarioPlayer.IsWaitLoading;
			}
		}

		/// <summary>
		/// シナリオが終了したか判定
		/// </summary>
		public bool IsEndScenario
		{
			get
			{
				if (ScenarioPlayer == null ) return false;
				if (IsLoading) return false;

				return ScenarioPlayer.IsStopScenario;
			}
		}

		/// <summary>
		/// シナリオが停止したか判定
		/// </summary>
		public bool IsStopScenario
		{
			get
			{
				if (ScenarioPlayer == null ) return false;
				if (IsLoading) return false;

				return ScenarioPlayer.IsEndScenario;
			}
		}

		/// <summary>
		/// 初期化。スクリプトから動的に生成する場合に
		/// </summary>
		public void InitOnCreate( AdvLayerManager layerManager, AdvTransitionManager transitionManager, AdvUiManager uiManager )
		{
			this.layerManager = layerManager;
			this.transitionManager = transitionManager;
			this.uiManager = uiManager;
		}

		/// <summary>
		/// 設定されたエクスポートデータからゲームを開始
		/// </summary>
		/// <param name="rootDirResource">リソースディレクトリ</param>
		public void BootFromExportData(AdvSettingDataManager settingDataManager, AdvScenarioDataExported[] exportedScenarioDataTbl, string resourceDir )
		{
			Clear();
			DataManager.InitData( settingDataManager, exportedScenarioDataTbl);
			BootInit(resourceDir);
		}

		/// <summary>
		/// 指定のパスのゲームを開始
		/// </summary>
		/// <param name="url">ファイルパス</param>
		/// <param name="resourceDir">リソースディレクトリ</param>
		/// <param name="version">シナリオバージョン（-1以下で必ずサーバーからデータを読み直す）</param>
		public void BootFromCsv(string url, string resourceDir, int version )
		{
			Debug.Log("Start BootFromCsv:" + url );
			AssetFileManager.DeleteCacheFileAll();
			Clear();
			StartCoroutine(LoadSettingDataCsvAsync(url, resourceDir, version));
		}
		IEnumerator LoadSettingDataCsvAsync(string url, string resourceDir, int version)
		{
			isWaitBootLoading = true;
			yield return StartCoroutine(DataManager.LoadCsvAsync(url, version));
			BootInit(resourceDir);
			isWaitBootLoading = false;
			Debug.Log("End   BootFromCsv");
		}

		void Clear()
		{
			Page.Clear();
			SelectionManager.Clear();
			BacklogManager.Clear();
			LayerManager.Clear();
			TransitionManager.Clear();
			if (UiManager!=null) UiManager.Close();
		}

		/// <summary>
		/// シナリオ終了
		/// </summary>
		public void EndScenario()
		{
			Clear();
			ScenarioPlayer.EndScenario();
		}

		/// <summary>
		/// EndScenarioだと画面消えちゃうので残したままシナリオを始めれる処理を追加
		/// </summary>
		public void StopScenario(){
			ScenarioPlayer.EndScenario();
		}

		/// <summary>
		/// 消す用のコマンド（作成時はEndScenarioと変わらず）
		/// </summary>
		public void EraseScenario(){
			EndScenario();
		}

		/// <summary>
		/// 起動時の初期化
		/// </summary>
		/// <param name="rootDirResource">ルートディレクトリのリソース</param>
		void BootInit(string rootDirResource )
		{
			DataManager.BootInit(rootDirResource);
			//設定データを反映
			LayerManager.InitLayerSetting(DataManager.SettingDataManager.LayerSetting);

			//システムセーブデータの初期化
			SystemSaveData.Init(Config);
			//通常セーブデータの初期化
			SaveManager.Init();

			//パラメーターを反映
			TextData.CallbackCalcExpression += Param.CalcExpressionNotSetParam;
			iTweenData.CallbackGetValue += Param.GetParameter;

			//パラメーターをデフォルト値でリセット
			param.Copy(DataManager.SettingDataManager.DefaultParam);

			//シナリオデータのロードと初期化を開始
			DataManager.StartLoadAndInitScenariodData();

			//リソースファイル(画像やサウンド)のダウンロードをバックグラウンドで進めておく

			// スクリプトから読み出すときに最初のシナリオを指定できないので削除
			//DataManager.StartBackGroundDownloadResource(StartScenarioLabel);
		}

		/// <summary>
		/// システムセーブデータを書き込み
		/// </summary>
		public void WriteSystemData()
		{
			systemSaveData.Write();
		}

		/// <summary>
		/// セーブデータを書き込み
		/// </summary>
		/// <param name="saveData">書き込むセーブデータ</param>
		public void WriteSaveData(AdvSaveData saveData)
		{
			SaveManager.WriteSaveData(this, saveData);
		}

		/// <summary>
		/// セーブデータのロード
		/// </summary>
		/// <param name="saveData">ロードするセーブデータ</param>
		void LoadSaveData(AdvSaveData saveData)
		{
			Clear();
			saveData.LoadGameData(this);
			StartScenario(saveData.CurrentSenarioLabel, saveData.CurrentPage, saveData.CurrentGallerySceneLabel );
		}

		/// <summary>
		/// クイックセーブ
		/// </summary>
		public void QuickSave()
		{
			WriteSaveData(SaveManager.QuickSaveData);
		}

		/// <summary>
		/// クイックロード
		/// </summary>
		/// <returns>成否</returns>
		public bool QuickLoad()
		{
			if (SaveManager.ReadQuickSaveData())
			{
				LoadSaveData(SaveManager.QuickSaveData);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// シナリオを一番最初から開始
		/// </summary>
		public void StartGame()
		{
			StartGame(StartScenarioLabel);
		}

		/// <summary>
		/// シナリオを指定のシーンから開始
		/// </summary>
		public void StartGame(string scenarioLabel)
		{
			isSceneGallery = false;
			StartGameSub(scenarioLabel);
		}

		void StartGameSub(string scenarioLabel)
		{
			//パラメーターをデフォルト値でリセット
			param.Copy(DataManager.SettingDataManager.DefaultParam);
			Clear();
			StartScenario(scenarioLabel, -1, "");
		}

		/// <summary>
		/// セーブデータをロードして開始
		/// </summary>
		/// <param name="saveData">ロードするセーブデータ</param>
		public void OpenLoadGame(AdvSaveData saveData)
		{
			isSceneGallery = false;
			LoadSaveData(saveData);
		}

		/// <summary>
		/// シーン回想を開始
		/// </summary>
		/// <param name="label">シーンラベル</param>
		public void StartSceneGallery(string label)
		{
			isSceneGallery = true;
			StartGameSub(label);
		}

		/// <summary>
		/// シーン回想を終了
		/// </summary>
		public void EndSceneGallery()
		{
			if (IsSceneGallery)
			{
				EndScenario();
			}
		}

		/*
			データマネージャーにセットデータが入ってるかの確認
		*/
		public bool IsReadySetingData(){
			return this.DataManager.IsReadySettingData;
		}

		/// <summary>
		/// 指定のラベルにシナリオジャンプ
		/// </summary>
		/// <param name="label">ジャンプ先のラベル</param>
		public void JumpScenario(string label)
		{
			StartScenario(label, -1, ScenarioPlayer.CurrentGallerySceneLabel );
		}

		void StartScenario(string label, int page, string gallerySceneLabel)
		{
			if (UiManager != null) UiManager.Open();
			ScenarioPlayer.StartScenario(this, label, page, gallerySceneLabel);
		}

	}
}







