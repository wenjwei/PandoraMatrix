
using UnityEngine;
using cPackage.Tools;

namespace cPackage.Package
{
	public partial class Configuration
	{
		[ConfigurationExtensionField]
		[ConfigurationField("UnityObjectToClipPos Replace:", "fix UnityObjectToClipPos Undeclared Compile Error")]
		public bool FixUnityObjectToClipPosUndeclared;
	}
}

namespace cPackage.Pipeline.Shader
{
	[ImportPrePipelineFeature("\\.shader$", "FixUnityObjectToClipPosUndeclared")]
	public class FixUnityObjectToClipPosUndeclaredCompileError : BaseFeatureProcess
	{
		public override void ProcessFeature(string filePath)
		{
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			string shaderContent = cPackageHelper.ReadStringFromFile(filePath);
			shaderContent = shaderContent.Replace("UnityObjectToClipPos(", "mul(UNITY_MATRIX_MVP, ");
			cPackageHelper.WriteStingToFile(filePath, shaderContent);
#endif
		}
	}
}
