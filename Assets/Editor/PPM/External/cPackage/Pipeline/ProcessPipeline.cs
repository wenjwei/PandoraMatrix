using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using cPackage.Tools;
using System.Text.RegularExpressions;

namespace cPackage.Pipeline
{
	#region Attribute

	[AttributeUsage(AttributeTargets.Class)]
	public abstract class PipelineFeatureAttribute : Attribute
	{
		public PipelineFeatureAttribute(string featurePattern, string configurationSwitch)
		{
			FeaturePattern = featurePattern;
			ConfigurationSwitch = configurationSwitch;
		}

		public string FeaturePattern;
		public string ConfigurationSwitch;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ExportPrePipelineFeatureAttribute : PipelineFeatureAttribute
	{
		public ExportPrePipelineFeatureAttribute(string featurePattern, string configurationSwitch) : base(featurePattern, configurationSwitch) { }

		public ExportPrePipelineFeatureAttribute(string featurePattern) : base(featurePattern, "") { }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ImportPrePipelineFeatureAttribute : PipelineFeatureAttribute
	{
		public ImportPrePipelineFeatureAttribute(string featurePattern, string configurationSwitch) : base(featurePattern, configurationSwitch) { }

		public ImportPrePipelineFeatureAttribute(string featurePattern) : base(featurePattern, "") { }
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ImportPostPipelineFeatureAttribute : PipelineFeatureAttribute
	{
		public ImportPostPipelineFeatureAttribute(string featurePattern, string configurationSwitch) : base(featurePattern, configurationSwitch) { }

		public ImportPostPipelineFeatureAttribute(string featurePattern) : base(featurePattern, "") { }
	}

	#endregion

	public abstract class BaseFeatureProcess
	{
		public int ExecutePriority = 100;

		public virtual string GetProgressBarTitle() { return ""; }

		public virtual void DisplayProgressBar(string info, float progress)
		{
			EditorUtility.DisplayProgressBar(GetProgressBarTitle(), info, progress);
		}

		public virtual bool BeforeProcessFeature(string toBeProcessedAssetPath, string assetPathInProject)
		{
			PipelineFeatureAttribute attr = cPackageHelper.GetClassAttribute<PipelineFeatureAttribute>(GetType());
			if (attr != null)
			{
				// Pattern Test
				Match match = Regex.Match(assetPathInProject, attr.FeaturePattern);
				if (!match.Success)
					return false;

				// ConfigurationSwitch Test
				if (string.IsNullOrEmpty(attr.ConfigurationSwitch) == true)
					return true;

				FieldInfo fi = typeof(cPackage.Package.Configuration).GetField(attr.ConfigurationSwitch);
				if (fi != null)
					return (bool)fi.GetValue(cPackageHelper.GetConfiguration());
				else
					cPackageHelper.LogError(string.Format("can not find {0} field in Configuration", attr.ConfigurationSwitch));
			}
			else
			{
				cPackageHelper.LogError(string.Format("can not find PipelineFeatureAttribute in {0}", GetType()));
			}

			return false;
		}

		public abstract void ProcessFeature(string toBeProcessedAssetPath);

		public virtual void AfterProcessFeature(string toBeProcessedAssetPath) { EditorUtility.ClearProgressBar(); }
	}

	public class ProcessPipeline<T> where T : PipelineFeatureAttribute
	{
		protected List<BaseFeatureProcess> _featureProcessList;

		public ProcessPipeline()
		{
			_featureProcessList = new List<BaseFeatureProcess>();
			RegisterFeatures();
		}

		protected virtual void RegisterFeatures()
		{
			List<Type> types = cPackageHelper.GetPipelineFeatures<T>();
			foreach (var type in types)
			{
				BaseFeatureProcess process = (BaseFeatureProcess)Activator.CreateInstance(type);
				if (!_featureProcessList.Contains(process))
				{
					_featureProcessList.Add(process);
					_featureProcessList.Sort((a, b) => { return a.ExecutePriority.CompareTo(b.ExecutePriority); });
				}
			}
		}

		/// <summary>
		/// Process asset
		/// </summary>
		/// <param name="toBeProcessedAssetPath">the asset path to be processed</param>
		/// <param name="assetPathInProject">the asset path in unity project</param>
		public virtual void Process(string toBeProcessedAssetPath, string assetPathInProject)
		{
			for (int i = 0; i < _featureProcessList.Count; i++)
			{
				var featureProcess = _featureProcessList[i];
				if (featureProcess.BeforeProcessFeature(toBeProcessedAssetPath, assetPathInProject))
					featureProcess.ProcessFeature(toBeProcessedAssetPath);
				featureProcess.AfterProcessFeature(toBeProcessedAssetPath);
			}
		}
	}
}
