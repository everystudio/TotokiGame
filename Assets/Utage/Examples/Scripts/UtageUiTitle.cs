//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using Utage;


/// <summary>
/// タイトル表示のサンプル
/// </summary>
[AddComponentMenu("Utage/Examples/Title")]
public class UtageUiTitle : UtageUiView
{
	/// <summary>メインゲーム画面</summary>
	public UtageUiMainGame mainGame;

	/// <summary>コンフィグ画面</summary>
	public UtageUiConfig config;

	/// <summary>セーブデターのロード画面</summary>
	public UtageUiSaveLoad load;

	///<summary>ギャラリー画面</summary>
	public UtageUiGallery gallery;

	///「はじめから」ボタンが押された
	void OnTapStart(Button button)
	{
		Close();
		mainGame.OpenStartGame();
	}

	///「つづきから」ボタンが押された
	void OnTapLoad(Button button)
	{
		Close();
		load.OpenLoad(this);
	}

	///「コンフィグ」ボタンが押された
	void OnTapConfig(Button button)
	{
		Close();
		config.Open(this);
	}
	
	//「ギャラリー」ボタンが押された
	void OnTapGallery(Button button)
	{
		Close();
		gallery.Open(this);
	}
}
