//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using Utage;

namespace Utage
{
	/// <summary>
	/// 選択肢用UIのサンプル
	/// </summary>
	[RequireComponent(typeof(ListViewItem))]
	[AddComponentMenu("Utage/ADV/UiSelection")]
	public class AdvUiSelection : MonoBehaviour
	{
		/// <summary>本文テキスト</summary>
		public TextArea2D text;

		/// <summary>
		/// 初期化
		/// </summary>
		/// <param name="data">選択肢データ</param>
		public void Init(AdvSelection data)
		{
			text.text = data.Text;
		}
	}
}
