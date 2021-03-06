//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;

namespace Utage
{

	/// <summary>
	/// IF分岐のマネージャー
	/// </summary>
	internal class AdvIfManager
	{
		//処理中のif文
		AdvIfData current;

		/// <summary>
		/// クリア
		/// </summary>
		public void Clear()
		{
			current = null;
		}

		/// <summary>
		/// if文の開始
		/// </summary>
		/// <param name="param">判定に使う数値パラメーター</param>
		/// <param name="exp">判定式</param>
		public void BeginIf(AdvParamSetting param, ExpressionParser exp)
		{
			AdvIfData new_if = new AdvIfData();
			if (null != current)
			{
				new_if.Parent = current;
			}
			current = new_if;
			current.BeginIf(param, exp);
		}

		/// <summary>
		/// else if文の開始
		/// </summary>
		/// <param name="param">判定に使う数値パラメーター</param>
		/// <param name="exp">判定式</param>
		public void ElseIf(AdvParamSetting param, ExpressionParser exp)
		{
			if (current == null)
			{
//				Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.ElseIf, exp));
				current = new AdvIfData();
				current.IsSkpping = true;
			}
			else
			{
				current.ElseIf(param, exp);
			}
		}

		/// <summary>
		/// else文の開始
		/// </summary>
		public void Else()
		{
			if (current == null)
			{
//				Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.Else));
				current = new AdvIfData();
				current.IsSkpping = true;
			}
			else
			{
				current.Else();
			}
		}

		/// <summary>
		/// if文の終了
		/// </summary>
		public void EndIf()
		{
			if (current == null)
			{
//				Debug.LogError(LanguageAdvErrorMsg.LocalizeTextFormat(AdvErrorMsg.EndIf));
				current = new AdvIfData();
				current.EndIf();
			}
			else
			{
				current.EndIf();
				current = current.Parent;
			}
		}

		/// <summary>
		/// 分岐によるスキップをする（条件判定がfalseなため処理をしない）か
		/// </summary>
		/// <param name="command">コマンドデータ</param>
		/// <returns>スキップする場合はtrue。しない場合はfalse</returns>
		public bool CheckSkip(AdvCommand command)
		{
			if (command == null) return false;

			if (null == current)
			{
				return false;
			}
			else
			{
				if (current.IsSkpping)
				{
					return !command.IsIfCommand;
				}
				else
				{
					return false;
				}
			}
		}
	}
}