
using UnityEngine;
using System;
using cPackage.Tools;

namespace cPackage.Package
{
	public partial class Configuration
	{
		[ConfigurationExtensionField]
		[ConfigurationField("Pandora Importer:", "nGUI or uGUI", "cPackage.Pipeline.CSharp.PandoraImporterSettingWindow")]
		public bool PandoraImporter;
	}
}

namespace cPackage.Pipeline.CSharp
{
	[ImportPrePipelineFeature("\\.cs$", "PandoraImporter")]
	public class PandoraImporter : BaseFeatureProcess
	{
		private bool _nGUISelected;
		private bool _uGUISelected;

		private void GetConfiguration()
		{
			_nGUISelected = Convert.ToBoolean(PlayerPrefs.GetString("cPackage.Pipeline.CSharp.nGUI", "true"));
			_uGUISelected = Convert.ToBoolean(PlayerPrefs.GetString("cPackage.Pipeline.CSharp.uGUI", "true"));
		}

		public override void ProcessFeature(string filePath)
		{
			GetConfiguration();

			if (_nGUISelected && _uGUISelected)
				return;

			string scriptContent = cPackageHelper.ReadStringFromFile(filePath);

			if (_uGUISelected == false)
			{
				scriptContent = scriptContent.Replace("#define USING_UGUI", "//#define USING_UGUI");
			}

			if (_nGUISelected == false)
			{
				scriptContent = scriptContent.Replace("#define USING_NGUI", "//#define USING_NGUI");
			}

			cPackageHelper.WriteStingToFile(filePath, scriptContent);
		}
	}
}
