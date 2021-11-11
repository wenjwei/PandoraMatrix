using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using cPackage.Tools;

namespace cPackage.Pipeline.Complex
{
    [ImportPrePipelineFeature("\\.*$", "PandoraImporter")]
    public class PandoraHelpToolsImporter : BaseFeatureProcess
    {
		public PandoraHelpToolsImporter()
		{
			ExecutePriority = 150;
		}

		private List<string> _checkList = new List<string>
        {
            "DraggableButton",
            "PandoraToolBox"
        };

        private  bool _nGUISelected;
        private  bool _uGUISelected;

        public override void ProcessFeature(string filePath)
        {
			string currentAssetDir = filePath.Substring(0, filePath.LastIndexOf("/"));
			string importPathnamePath = currentAssetDir + "/pathname";
			string importPath = cPackageHelper.ReadStringFromFile(importPathnamePath);

			foreach (var item in _checkList)
            {
                if (importPath.Contains(item))
                {
					//走删除流程
					Process(importPath, currentAssetDir);
				}
            }  
        }

        private  void Process(string importPath, string currentAssetDir)
        {
            GetConfiguration();
            if (_nGUISelected && _uGUISelected)
                return;

            if (_nGUISelected && (importPath.Contains("UGUI") || importPath.Contains("DraggableButton")) && Directory.Exists(currentAssetDir))
            {
				Directory.Delete(currentAssetDir, true);
				return;
            }
            if (_uGUISelected && importPath.Contains("NGUI") && Directory.Exists(currentAssetDir))
            {
				Directory.Delete(currentAssetDir, true);
				return;
            }
        }

        private  void GetConfiguration()
        {
            _nGUISelected = Convert.ToBoolean(PlayerPrefs.GetString("cPackage.Pipeline.CSharp.nGUI", "true"));
            _uGUISelected = Convert.ToBoolean(PlayerPrefs.GetString("cPackage.Pipeline.CSharp.uGUI", "true"));
        }
    }
}