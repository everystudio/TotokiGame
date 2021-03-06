//----------------------------------------------
// UTAGE: Unity Text Adventure Game Engine
// Copyright 2014 Ryohei Tokimura
//----------------------------------------------

using UnityEngine;
using Utage;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// シーン回想用のUIのサンプル
/// </summary>
[AddComponentMenu("Utage/Examples/SceneGalleryItem")]
[RequireComponent(typeof(ListViewItem))]
public class UtageUiSceneGalleryItem : MonoBehaviour
{
	public Sprite2D texture;
	public TextArea2D title;
	public float pixelsToUnits = 100;

	public ListViewItem ListViewItem { get { return this.listViewItem ?? (this.listViewItem = GetComponent<ListViewItem>()); } }
	ListViewItem listViewItem;

	/// <summary>
	/// 初期化
	/// </summary>
	/// <param name="data">セーブデータ</param>
	/// <param name="index">インデックス</param>
	public void Init(AdvSceneGallerySettingData data, int index, AdvSystemSaveData saveData )
	{
		bool isOpend = saveData.GalleryData.CheckSceneLabels(data.ScenarioLabel);
		ListViewItem.IsEnableButton = isOpend;
		if (!isOpend)
		{
			texture.LocalAlpha = 0;
			title.text = "";
		}
		else{
			texture.SetTextureFile(data.ThumbnailPath, pixelsToUnits);
			title.text = data.Title;
		}
	}
}
