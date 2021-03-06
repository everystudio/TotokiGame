//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using System.Collections;

namespace Utage
{

	/// <summary>
	/// ボタン一つのダイアログ
	/// </summary>
	[AddComponentMenu("Utage/Lib/UI/Dialog1Button")]
	public class Dialog1Button : MonoBehaviour
	{

		/// <summary>
		/// 本文表示用のテキストエリア
		/// </summary>
		[SerializeField]
		protected TextArea2D windowLabel;

		/// <summary>
		/// ボタン1用のテキストエリア
		/// </summary>
		[SerializeField]
		protected TextArea2D button1Label;

		/// <summary>
		/// ボタンを押したときのメッセージの送り先
		/// </summary>
		[SerializeField]
		protected GameObject target;

		/// <summary>
		/// ボタン1を押したときに送られるメッセージ
		/// </summary>
		[SerializeField]
		protected string func1;

		/// <summary>
		/// 表示テキスト。配列が複数の場合は複数ページ。ボタン1を押すごとにページ送り
		/// </summary>
		[SerializeField]
		protected string[] textArray;

		int indexText = 0;

		/// <summary>
		/// ダイアログを開く
		/// </summary>
		/// <param name="text">表示テキスト</param>
		/// <param name="buttonText1">ボタン1のテキスト</param>
		/// <param name="target">ボタンを押したときのメッセージの送り先</param>
		/// <param name="func1">ボタン1を押したときに送られるメッセージ</param>
		public void Open(string text, string buttonText1, GameObject target, string func1 )
		{
			string[] array = { text };
			Open(array, buttonText1, target, func1 );
		}

		/// <summary>
		/// ダイアログを開く（テキストを複数ページで表示）
		/// </summary>
		/// <param name="textArray">表示テキスト。配列要素ごとに複数ページに対応</param>
		/// <param name="buttonText1">ボタン1のテキスト</param>
		/// <param name="target">最終ページでボタン1を押したときのメッセージの送り先</param>
		/// <param name="func1">最終ページでボタン1を押したときに送られるメッセージ</param>
		public void Open(string[] textArray, string buttonText1, GameObject target, string func1)
		{
			indexText = 0;
			this.textArray = textArray;
			windowLabel.text = textArray[0];
			button1Label.text = buttonText1;
			this.target = target;
			this.func1 = func1;
			Open();
		}

		/// <summary>
		/// ボタン1が押された時の処理
		/// </summary>
		protected void OnTapButton1()
		{
			++indexText;
			if (textArray != null && indexText < textArray.Length)
			{
				windowLabel.text = textArray[indexText];
			}
			else
			{
				UtageToolKit.SafeSendMessage(target, func1, true);
				Close();
			}
		}

		/// <summary>
		/// オープン
		/// </summary>
		public void Open()
		{
			this.gameObject.SetActive(true);
		}

		/// <summary>
		/// クローズ
		/// </summary>
		public void Close()
		{
			this.gameObject.SetActive(false);
		}
	}
}