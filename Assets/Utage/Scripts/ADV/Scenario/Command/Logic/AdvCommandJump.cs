//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------
using UnityEngine;

namespace Utage
{

	/// <summary>
	/// コマンド：他他シナリオにジャンプ
	/// </summary>
	internal class AdvCommandJump : AdvCommand
	{
		public AdvCommandJump(StringGridRow row, AdvSettingDataManager dataManager)
		{
			this.jumpLabel = AdvCommandParser.ParseScenarioLabel(row, AdvColumnName.Arg1);
			string expStr = AdvParser.ParseCellOptional<string>(row, AdvColumnName.Arg2, "");
			if (string.IsNullOrEmpty(expStr))
			{
				this.exp = null;
			}
			else
			{
				this.exp = dataManager.DefaultParam.CreateExpressionBoolean(expStr);
				if (this.exp.ErrorMsg != null)
				{
					Debug.LogError(row.ToErrorString(this.exp.ErrorMsg));
				}
			}
		}

		public override string GetJumpLabel() { return jumpLabel; }

		public override void DoCommand(AdvEngine engine)
		{
			if (IsEnable(engine.Param))
			{
				engine.ScenarioPlayer.JumpReady(engine);
				engine.JumpScenario(jumpLabel);
			}
		}

		//コマンド終了待ち
		//JumpScenarioした場合は常にtrueを返すのが必須
		public override bool Wait(AdvEngine engine)
		{
			if (IsEnable(engine.Param))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		bool IsEnable( AdvParamSetting param )
		{
			return (exp == null || param.CalcExpressionBoolean( exp ) );
		}

		string jumpLabel;
		ExpressionParser exp;	//ジャンプ条件式
	}
}