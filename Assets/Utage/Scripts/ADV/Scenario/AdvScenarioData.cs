//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Utage
{

	/// <summary>
	/// シナリオのデータ
	/// </summary>
	public class AdvScenarioData
	{
		/// <summary>
		/// シナリオ名
		/// </summary>
		string Name { get { return this.name; } }
		string name;

		/// <summary>
		/// データグリッド名
		/// </summary>
		public string DataGridName { get { return dataGridName; } }
		string dataGridName;

		/// <summary>
		/// 初期化済みか
		/// </summary>
		public bool IsInit { get { return this.isInit; } }
		bool isInit = false;

		/// <summary>
		/// バックグランドでのロード処理を既にしているか
		/// </summary>
		public bool IsAlreadyBackGroundLoad { get { return this.isAlreadyBackGroundLoad; } }
		bool isAlreadyBackGroundLoad = false;

		/// <summary>
		/// このシナリオにあるシナリオラベル
		/// </summary>
		public List<string> ScenarioLabels { get { return this.scenarioLabels; } }
		List<string> scenarioLabels = new List<string>();


		/// <summary>
		/// このシナリオからリンクするジャンプ先のシナリオラベル
		/// </summary>
		public List<string> JumpScenarioLabels { get { return this.jumpScenarioLabels; } }
		List<string> jumpScenarioLabels = new List<string>();

		List<AdvCommand> commandList = new List<AdvCommand>();


		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="name">シナリオ名</param>
		/// <param name="grid">シナリオデータ</param>
		/// <param name="dataManager">各種設定データ</param>
		public void Init(string name, StringGrid grid, AdvSettingDataManager dataManager)
		{
			this.name = name;
			this.dataGridName = grid.Name;
			scenarioLabels.Add(name);
			ParseFromStringGrid(grid, dataManager);
		}

		/// <summary>
		/// 指定インデックスのコマンドを取得
		/// </summary>
		/// <param name="index">インデックス</param>
		/// <returns>コマンド</returns>
		public AdvCommand GetCommand(int index)
		{
			if (index < commandList.Count)
			{
				return commandList[index];
			}
			else
			{
				return null;
			}
		}


		/// <summary>
		/// バックグランドでダウンロードだけする
		/// </summary>
		/// <param name="dataManager">各種設定データ</param>
		public void Download(AdvDataManager dataManager)
		{
			foreach (AdvCommand command in commandList)
			{
				command.Download();
			}
			isAlreadyBackGroundLoad = true;
		}



		/// <summary>
		/// 指定のシナリオラベルがあるかチェック
		/// </summary>
		/// <param name="scenarioLabel">シナリオラベル</param>
		/// <returns>あったらtrue。なかったらfalse</returns>
		public bool IsExistScenarioLabel(string scenarioLabel)
		{
			foreach (string label in ScenarioLabels)
			{
				if (label == scenarioLabel)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 指定シナリオラベルの指定ページのコマンドインデックスを取得
		/// </summary>
		/// <param name="scenarioLabel">シナリオラベル</param>
		/// <param name="page">ページ</param>
		/// <returns>ページのコマンドインデックス</returns>
		public int SeekPageIndex(string scenarioLabel, int page)
		{
			int index = 0;

			if (Name == scenarioLabel)
			{
				//シナリオ名そのものだった場合は一番最初から
				index = 0;
			}
			else
			{
				//シナリオラベルをシーク
				while (true)
				{
					AdvCommand command = GetCommand(index);
					if (null == GetCommand(index))
					{
						Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.NotFoundScnarioLabel,scenarioLabel));
						return 0;
					}

					if ( command.GetScenarioLabel() == scenarioLabel)
					{
						break;
					}
					++index;
				}
			}
			if (page < 0)
			{	//シナリオラベル冒頭
				return index;
			}

			int pageCount = 0;
			//シナリオラベルからの指定のページまでシーク
			while (true)
			{
				AdvCommand command = GetCommand(index);
				if (null == command)
				{
					//指定のページ数がなかったので、ここまでで終了
					return index-1;
				}
				if (command.IsTypePageEnd())
				{
					if (pageCount >= page)
					{
						return index;
					}
					++pageCount;
				}
				++index;
			}
		}

		//コマンドデータの解析・初期化
		void ParseFromStringGrid(StringGrid grid, AdvSettingDataManager dataManager)
		{
			AddCommandBegin();
			AdvCommand continuousCommand = null;	//連続的なコマンド処理

			foreach (StringGridRow row in grid.Rows)
			{
				if (row.RowIndex < grid.DataTopRow ) continue;			//データの行じゃない
				if (row.IsEmpty) continue;								//データがない

				AdvCommand command = AdvCommandParser.CreateCommand( row, dataManager);
				if (null != command)
				{
					//連続するコマンドの場合は、連続が途切れたら終了コマンドを追加
					TryAddContinusCommand(continuousCommand, command);
					//コマンド追加
					AddCommand(command);
					//連続するコマンドの場合は、連続が途切れたら終了コマンドを追加
					continuousCommand = GetNextContinusCommand(continuousCommand, command);
				}
			}
			//連続するコマンドの場合は、連続が途切れたら終了コマンドを追加
			TryAddContinusCommand(continuousCommand, null);

			AddCommandEnd();
		}

		/// <summary>
		/// 選択肢など連続するタイプのコマンドの場合は、連続が途切れたら終了コマンドを追加
		/// </summary>
		/// <param name="continuousCommand">連続しているコマンド</param>
		/// <param name="currentCommand">現在のコマンド</param>
		void TryAddContinusCommand(AdvCommand continuousCommand, AdvCommand currentCommand )
		{
			if (continuousCommand != null)
			{
				if ( currentCommand == null || !continuousCommand.CheckContinues(currentCommand))
				{
					AddCommand(AdvCommandParser.CreateContiunesEndCommand(continuousCommand));
				}
			}
		}

		/// <summary>
		/// 次の連続するタイプのコマンドを現在のコマンドから取得
		/// </summary>
		/// <param name="continuousCommand">連続しているコマンド</param>
		/// <param name="currentCommand">現在のコマンド</param>
		/// <returns>次の連続するタイプのコマンド</returns>
		AdvCommand GetNextContinusCommand(AdvCommand continuousCommand, AdvCommand currentCommand )
		{
			if( currentCommand.IsIfCommand )
			{
				//IF文など、連続コマンドを途切れさせない場合は連続コマンドを変えない
				return continuousCommand;
			}
			else if (currentCommand.IsContinusCommand )
			{
				//現在のコマンドが連続コマンドなら更新
				return currentCommand;
			}
			else
			{
				return null;
			}
		}

		//コマンドの追加開始
		void AddCommandBegin()
		{
			isInit = false;
			commandList = new List<AdvCommand>();
		}

		//コマンドの追加
		void AddCommand(AdvCommand command)
		{
			if (null != command) commandList.Add(command);
		}

		//コマンドの追加終了
		void AddCommandEnd()
		{
			//シナリオラベルの解析
			foreach (AdvCommand command in commandList)
			{
				///シナリオラベルを取得
				string scenarioLabel = command.GetScenarioLabel();
				if (!string.IsNullOrEmpty(scenarioLabel) )
				{
					if (scenarioLabels.Contains(scenarioLabel))
					{
						Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.RedefinitionScenarioLabel, scenarioLabel,DataGridName));
					}
					else
					{
						scenarioLabels.Add(scenarioLabel);
					}
				}

				///このシナリオからリンクするジャンプ先のシナリオラベルを取得
				string jumpLabel = command.GetJumpLabel();
				if (!string.IsNullOrEmpty(jumpLabel))
				{
					jumpScenarioLabels.Add(jumpLabel);
				}
			}

			isInit = true;
		}
	}
}