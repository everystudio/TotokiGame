//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;

namespace Utage
{

	/// <summary>
	/// テキストメッセージ制御
	/// </summary>
	[AddComponentMenu("Utage/ADV/Internal/MessageWindow")]
	public class AdvPage : MonoBehaviour
	{
		/// <summary>
		/// シナリオラベル
		/// </summary>
		public string ScenarioLabel
		{
			get { return scenarioLabel; }
			set { scenarioLabel = value; }
		}
		string scenarioLabel;

		/// <summary>
		/// ページ番号
		/// </summary>
		public int PageNo
		{
			get { return pageNo; }
			private set { pageNo = value; }
		}
		int pageNo;

		/// <summary>
		/// 表示する名前テキスト
		/// </summary>
		public string NameText
		{
			get { return nameText; }
			private set { nameText = value; }
		}
		string nameText = "";

		/// <summary>
		/// 表示テキストデータ
		/// </summary>
		public TextData TextData
		{
			get { return textData; }
		}
		TextData textData;

		/// <summary>
		/// 現在の、文字送りインデックス
		/// </summary>
		public int CurrentTextLen { get { return currentTextLen; } }
		int currentTextLen;

		float deltaTimeSendMessage;			//テキスト送りに使う時間経過

		enum Status
		{
			SendChar,		//文字送り中
			WaitBrPage,		//改行待ち
			WaitSelection,	//選択肢待ち
			BrPage,			//改ページされた
			EndPage,		//ページ終了
		};
		Status status = Status.BrPage;

		//テキスト表示中か
		public bool IsShowingText
		{
			get { return status == Status.SendChar || status == Status.WaitBrPage; }
		}
		//改ページ待ち中か
		public bool IsWaitPage
		{
			get { return IsShowingText || status == Status.WaitSelection; }
		}

		AdvEngine Engine { get { return this.engine ?? (this.engine = GetComponent<AdvEngine>()); } }
		[SerializeField]
		AdvEngine engine;


		/// <summary>
		/// クリア
		/// </summary>
		public void Clear()
		{
			this.status = Status.BrPage;
			this.ScenarioLabel = "";
			this.PageNo = 0;
			this.NameText = "";
			this.textData = new TextData("");
			this.currentTextLen = 0;
			this.deltaTimeSendMessage = 0;
		}

		/// <summary>
		/// ページ冒頭の初期化
		/// </summary>
		/// <param name="scenarioName">シナリオラベル</param>
		/// <param name="pageNo">ページ名</param>
		public void BeginPage(string scenarioLabel, int pageNo)
		{
			this.ScenarioLabel = scenarioLabel;
			this.PageNo = pageNo;
		}

		/// <summary>
		/// ページ終了
		/// </summary>
		/// <param name="scenarioName">シナリオラベル</param>
		/// <param name="pageNo">ページ名</param>
		public void EndPage()
		{
			this.NameText = "";
			this.textData = new TextData("");
			this.currentTextLen = 0;
			this.deltaTimeSendMessage = 0;
			this.status = Status.EndPage;
		}


		/// <summary>
		/// ページ末までノーウェイトでテキスト送り
		/// </summary>
		public void SendTextNoWait()
		{
			currentTextLen = TextData.Length;
		}

		/// <summary>
		/// 文字送り
		/// </summary>
		/// <param name="timeCharSend">文字送りにかかる時間</param>
		public void SendChar(float timeCharSend)
		{
			//1秒間の文字送り数(0以下の場合、ページ末までノーウェイト)
			if (currentTextLen >= TextData.Length)
			{
				SendTextNoWait();
			}
			else
			{
				if (timeCharSend <= 0)
				{
					currentTextLen = TextData.Length;
				}
				else
				{
					deltaTimeSendMessage += Time.deltaTime;
					while (deltaTimeSendMessage >= 0)
					{
						++currentTextLen;
						deltaTimeSendMessage -= timeCharSend;
						if (currentTextLen > TextData.Length)
						{
							currentTextLen = TextData.Length;
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// キャラクタのセリフを設定
		/// </summary>
		/// <param name="text">テキスト(セリフ)</param>
		/// <param name="name">キャラクター名</param>
		public void SetCharacterText(string text, string name)
		{
			this.NameText = name;
			this.textData = new TextData(text);
			if (text.Length == 0)
			{
				Debug.LogError("text is empty");
			}
			this.status = ( text.Length > 0 ) ? Status.SendChar : Status.BrPage;
			this.isInputSendMessage = false;
		}

		/// <summary>
		/// テキスト（地の文）を設定
		/// </summary>
		/// <param name="text">テキスト</param>
		public void SetText(string text)
		{
			SetCharacterText(text, "");
		}

		/// <summary>
		/// 選択肢待ち
		/// </summary>
		public void SetSelectionWait()
		{
			this.status = Status.WaitSelection;
		}

		/// <summary>
		/// 文字送りの入力
		/// 外部から呼ぶこと
		/// </summary>
		public void InputSendMessage() { isInputSendMessage = true; }
		bool IsInputSendMessage() { return isInputSendMessage; }
		bool isInputSendMessage;

		//改ページ待ち時間
		float waitTimeBrPage;


		/// <summary>
		/// スキップのチェック
		/// </summary>
		/// <returns></returns>
		public bool CheckSkip()
		{
			return Engine.Config.CheckSkip(Engine.SystemSaveData.ReadData.CheckReadPage(scenarioLabel, pageNo));
		}

		/// <summary>
		/// テキストの更新。外部から呼ぶこと
		/// スキップやページ送りの入力の結果処理・文字送りなどの処理をする
		/// 更新の順番がシビアなので、内部でUpdateをしない。
		/// </summary>
		public void UpdateText()
		{
			if (CheckSkip())
			{
				EndBrPageWait();
			}
			else
			{
				//文字送り
				switch (status)
				{
					case Status.SendChar:
						UpdateSendChar();
						break;
					case Status.WaitBrPage:
						UpdateWaitBrPage();
						break;
					default:
						break;
				}
			}
			isInputSendMessage = false;
		}

		//文字送り
		void UpdateSendChar()
		{
			bool isSend = IsInputSendMessage();
			if (isSend)
			{
				SendTextNoWait();
			}
			else
			{
				SendChar(Engine.Config.TimeSendChar);
			}

			if ((currentTextLen >= TextData.Length))
			{
				status = Status.WaitBrPage;
				waitTimeBrPage = Engine.Config.AutoPageWaitTime;
			}
		}

		//改ページ待ち
		void UpdateWaitBrPage()
		{
			if (Engine.Config.IsAutoBrPage)
			{
				waitTimeBrPage -= Time.deltaTime;
				if (waitTimeBrPage < 0 && Engine.SoundManager.IsStop(SoundManager.StreamType.Voice))
				{
					EndBrPageWait();
					return;
				}
			}
			if (IsInputSendMessage())
			{
				EndBrPageWait();
				return;
			}
		}
		//改ページ待ちの終了
		void EndBrPageWait()
		{
			SendTextNoWait();
			status = Status.BrPage;
		}
	}
}