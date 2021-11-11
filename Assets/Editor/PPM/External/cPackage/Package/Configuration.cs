
using System;
using UnityEngine;

namespace cPackage.Package
{
	[AttributeUsage(AttributeTargets.Field)]
	public class ConfigurationFieldAttribute : Attribute
	{
		public ConfigurationFieldAttribute(string editorLabel, string tips = "", string settingCls = "")
		{
			EditorLabel = editorLabel;
			Tips = tips;
			SettingCls = settingCls;
		}

		public string EditorLabel { get; set; }

		public string Tips { get; set; }

		public string SettingCls { get; set; }
	}

	[AttributeUsage(AttributeTargets.Field)]
	public class ConfigurationExtensionFieldAttribute : Attribute { }

	public partial class Configuration
	{
		[ConfigurationField("Enable Debug Mode:")]
		public bool DebugMode;

		[ConfigurationField("Export Include CSharp:")]
		public bool IncludeCSharp;

		[System.NonSerialized]
		[ConfigurationField("Serialized Version:", "GameObject Serialized Version")]
		public string GameObjectSerializedVersion;

		[ConfigurationField("Advanced Mode:")]
		public bool AdvancedMode;
	}
}
