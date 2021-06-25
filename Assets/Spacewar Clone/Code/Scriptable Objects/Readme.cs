using UnityEngine;
using System;

namespace HeathenEngineering
{
	[CreateAssetMenu(fileName = "Readme", menuName = "ScriptableObjects/Readme", order = 5)]
	public class Readme : ScriptableObject
	{
		public Texture2D icon;
		public float iconMaxWidth = 128f;
		public string title;
		public Section[] sections;

		[Serializable]
		public class Section
		{
			public string heading, text, linkText, url;
		}
	}
}