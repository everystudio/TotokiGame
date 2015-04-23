using UnityEngine;
using System.Collections;

public class Define : MonoBehaviour {

	public static string UtageScenarioRoot{
		get{
			string scenario_path = "http://every-studio.com";
			scenario_path = "http://192.168.33.10";
			scenario_path = Application.streamingAssetsPath;
			//scenario_path = "http://every-studio.com/resources";
			return scenario_path;
		}
	}
	public static string UtageResourcesRoot{
		get{
			string resources_path = "http://every-studio.com/resources";
			//resources_path = "http://192.168.33.10";
			//resources_path = Application.streamingAssetsPath;

			return resources_path;
		}
	}

}
