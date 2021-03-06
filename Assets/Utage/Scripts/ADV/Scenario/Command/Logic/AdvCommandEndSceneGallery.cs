//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------


namespace Utage
{

	/// <summary>
	/// コマンド：シーン回想終了
	/// </summary>
	internal class AdvCommandEndSceneGallery : AdvCommand
	{
		public AdvCommandEndSceneGallery(StringGridRow row)
		{
		}

		public override void DoCommand(AdvEngine engine)
		{
			engine.ScenarioPlayer.EndSceneGallery(engine);
			if (engine.IsSceneGallery)
			{
				engine.EndSceneGallery();
			}
		}
		//コマンド終了待ち
		public override bool Wait(AdvEngine engine) { return engine.IsSceneGallery; }
	}
}