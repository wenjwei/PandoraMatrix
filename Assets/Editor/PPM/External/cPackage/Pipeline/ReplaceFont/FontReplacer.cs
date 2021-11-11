using System;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace cPackage.Pipeline.Prefab
{
    public class FontReplacer
    {
        public static int ReplaceFont(ref GameObject gameObject, Font font)
        {
            int counter = 0;

            Type textType = GetTypeByFullName("UnityEngine.UI.Text");
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
			if (textType != null && gameObject.GetComponentInChildren(textType, true) != null)
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			if (textType != null && gameObject.GetComponentInChildren(textType) != null)
#endif
			{
				counter += ReplaceUGUIFont(ref gameObject, font);
            }

            Type uiLabelType = GetTypeByFullName("UILabel");
#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4_OR_NEWER
            if (uiLabelType != null && gameObject.GetComponentInChildren(uiLabelType, true) != null)
#elif UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
			if (uiLabelType != null && gameObject.GetComponentInChildren(uiLabelType) != null)
#endif
			{
				counter += ReplaceNGUIFont(ref gameObject, font);
            }

            EditorUtility.SetDirty(gameObject);
            return counter;
        }

        private static Type GetTypeByFullName(string typeFullName)
        {
            var assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblyArray)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.FullName.Equals(typeFullName))
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        private static int ReplaceUGUIFont(ref GameObject gameObject, Font font)
        {
            int counter = 0;
            counter += ReplaceComponentFont(ref gameObject, font, "UnityEngine.UI.Text", "font");
            return counter;
        }

        private static int ReplaceNGUIFont(ref GameObject gameObject, Font font)
        {
            int counter = 0;
            counter += ReplaceComponentFont(ref gameObject, font, "UILabel", "trueTypeFont");
            counter += ReplaceComponentFont(ref gameObject, font, "UIPopupList", "trueTypeFont");
            return counter;
        }

        private static int ReplaceComponentFont(ref GameObject gameObject, Font font, String componentFullName, String fontPropertyName)
        {
            int counter = 0;

            Type componentType = GetTypeByFullName(componentFullName);
            if (componentType == null)
            {
                return counter;
            }
            Component[] componentArray = gameObject.GetComponentsInChildren(componentType, true);
            foreach (var component in componentArray)
            {
                PropertyInfo componentFontProperty = component.GetType().GetProperty(fontPropertyName);
                if (componentFontProperty != null)
                {
                    componentFontProperty.SetValue(component, font, null);
                }
                counter++;
            }

            return counter;
        }
    }
}
