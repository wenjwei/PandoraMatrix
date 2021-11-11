// The MIT License (MIT)

// Copyright 2015 Siney/Pangweiwei siney@yeah.net
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

//#define USING_NGUI
#define USING_UGUI
namespace com.tencent.pandora
{
    using System.Collections.Generic;
    using System;

    public class CustomExport
    {
        public static void OnGetAssemblyToGenerateExtensionMethod(out List<string> list)
        {
            list = new List<string> {
                "Assembly-CSharp",
            };
        }

        //自定义类型列表
        public static void OnAddCustomClass(LuaCodeGen.ExportGenericDelegate add)
        {
            add(typeof(Dictionary<string, string>), "DictStrStr");
            add(typeof(List<string>), "ListStr");
            add(typeof(CSharpInterface), null);
            add(typeof(Logger), null);
#if USING_NGUI
            //NGUI类型列表
            add(typeof(UIButton), null);
            add(typeof(UIButtonColor), null);
            //add(typeof(UIButtonActivate), null);
            //add(typeof(UIButtonColor), null);
            //add(typeof(UIButtonKeys), null);
            //add(typeof(UIButtonMessage), null);
            //add(typeof(UIButtonOffset), null);
            //add(typeof(UIButtonRotation), null);
            //add(typeof(UIButtonScale), null);
            //add(typeof(UICenterOnChild), null);
            //add(typeof(UICenterOnClick), null);
            //add(typeof(UIDragCamera), null);
            //add(typeof(UIDragDropContainer), null);
            //add(typeof(UIDragDropItem), null);
            //add(typeof(UIDragDropRoot), null);
            //add(typeof(UIDraggableCamera), null);
            //add(typeof(UIDragObject), null);
            //add(typeof(UIDragResize), null);
            //add(typeof(UIDragScrollView), null);
            //add(typeof(UIEventTrigger), null);
            add(typeof(UIGrid), null);
            //add(typeof(UIImageButton), null);
            //add(typeof(UIKeyBinding), null);
            //add(typeof(UIKeyNavigation), null);
            //add(typeof(UIPlayAnimation), null);
            //add(typeof(UIPlaySound), null);
            //add(typeof(UIPlayTween), null);
            add(typeof(UIPopupList), null);
            //add(typeof(UIProgressBar), null);
            //add(typeof(UISavedOption), null);
            //add(typeof(UIScrollBar), null);
            add(typeof(UIScrollView), null);
            //add(typeof(UIShowControlScheme), null);
            add(typeof(UISlider), null);
            //add(typeof(UISoundVolume), null);
            add(typeof(UITable), null);
            add(typeof(UIToggle), null);
            //add(typeof(UIToggledComponents), null);
            //add(typeof(UIToggledObjects), null);
            //add(typeof(UIWidgetContainer), null);
            //add(typeof(UIWrapContent), null);
            //add(typeof(AnimatedAlpha), null);
            //add(typeof(AnimatedColor), null);
            //add(typeof(SpringPosition), null);
            //add(typeof(TweenAlpha), null);
            //add(typeof(TweenColor), null);
            //add(typeof(TweenFOV), null);
            //add(typeof(TweenHeight), null);
            //add(typeof(TweenOrthoSize), null);
            //add(typeof(TweenPosition), null);
            //add(typeof(TweenRotation), null);
            //add(typeof(TweenScale), null);
            //add(typeof(TweenTransform), null);
            //add(typeof(TweenVolume), null);
            //add(typeof(TweenWidth), null);
            add(typeof(UITweener), null);
            //add(typeof(UI2DSprite), null);
            //add(typeof(UI2DSpriteAnimation), null);
            add(typeof(UIAnchor), null);
            add(typeof(UIAtlas), null);
            //add(typeof(UICamera), null);
            //add(typeof(UIColorPicker), null);
            //add(typeof(UIFont), null);
            add(typeof(UIInput), null);
            //add(typeof(UIInputOnGUI), null);
            add(typeof(UILabel), null);
            add(typeof(UIWidget), null);
            add(typeof(UIWidgetContainer), null);
            //add(typeof(UILocalize), null);
            //add(typeof(UIOrthoCamera), null);
            //add(typeof(UIPanel), null);
            //add(typeof(UIRoot), null);
            add(typeof(UISprite), null);
            add(typeof(UIBasicSprite), null);
            //add(typeof(UISpriteAnimation), null);
            //add(typeof(UISpriteData), null);
            //add(typeof(UIStretch), null);
            //add(typeof(UITextList), null);
            add(typeof(UITexture), null);
            //add(typeof(UITooltip), null);
            //add(typeof(UIViewport), null);
            add(typeof(EventDelegate), null);
            add(typeof(UIEventListener), null);
            add(typeof(UIRect), null);
#endif
            // add your custom class here
            // add( type, typename)
            // type is what you want to export
            // typename used for simplify generic type name or rename, like List<int> named to "ListInt", if not a generic type keep typename as null or rename as new type name
        }

        public static void OnAddCustomAssembly(ref List<string> list)
        {
            // add your custom assembly here
            // you can build a dll for 3rd library like ngui titled assembly name "NGUI", put it in Assets folder
            // add its name into list, slua will generate all exported interface automatically for you

            //list.Add("NGUI");
        }

        //自定义的名称空间
        public static HashSet<string> OnAddCustomNamespace()
        {
            return new HashSet<string>
            {
            };
        }

        /// <summary>
        /// UnityEngine.UI.dll中类型白名单列表，定义该列表后只有该列表中的类型会被导出，注意需要用到类型的父类型方法时，父类型也需要添加到该列表
        /// </summary>
        /// <param name="list"></param>
        public static void OnGetUIUseList(out List<string> list)
        {
            list = new List<string>
            {
#if USING_UGUI
                "UnityEngine.EventSystems.UIBehaviour",
                "UnityEngine.UI.Button",
                "UnityEngine.UI.Graphic",
                "UnityEngine.UI.MaskableGraphic",
                "UnityEngine.UI.GridLayoutGroup",
                "UnityEngine.UI.HorizontalLayoutGroup",
                "UnityEngine.UI.VerticalLayoutGroup",
                "UnityEngine.UI.LayoutGroup",
                "UnityEngine.UI.LayoutRebuilder",
                "UnityEngine.UI.LayoutUtility",
                "UnityEngine.UI.Image",
                "UnityEngine.UI.InputField",
                "UnityEngine.UI.Mask",
                "UnityEngine.UI.Outline",
                "UnityEngine.UI.RawImage",
                "UnityEngine.UI.Scrollbar",
                "UnityEngine.UI.ScrollRect",
                "UnityEngine.UI.Selectable",
                "UnityEngine.UI.Shadow",
                "UnityEngine.UI.Slider",
                "UnityEngine.UI.Text",
                "UnityEngine.UI.Toggle",
                "UnityEngine.UI.ToggleGroup",
                "UnityEngine.UI.Dropdown"
#endif
            };
        }

        /// <summary>
        /// UnityEngine.UI.dll中类型黑名单列表，此列表中的类不会被导出
        /// </summary>
        /// <param name="list"></param>
        public static void OnGetUINoUseList(out List<string> list)
        {
            list = new List<string>
            {
                "GraphicRegistry",
                "CoroutineTween",
                "FontUpdateTracker",
                "GraphicRebuildTracker",
                "PositionAsUV1"
            };
        }

        // if uselist return a white list, don't check noUseList(black list) again
        /// <summary>
        /// UnityEngine.dll中类型白名单，定义该列表后只有该列表中的类型会被导出，注意需要用到类型的父类型方法时，父类型也需要添加到该列表
        /// </summary>
        /// <param name="list"></param>
        public static void OnGetEngineUseList(out List<string> list)
        {
            list = new List<string>
            {
                "UnityEngine.Application",
                "UnityEngine.SystemInfo",
                "UnityEngine.Object",
                "UnityEngine.Component",
                "UnityEngine.Behaviour",
                "UnityEngine.MonoBehaviour",
                "UnityEngine.Vector2",
                "UnityEngine.Vector3",
                "UnityEngine.Vector4",
                "UnityEngine.Quaternion",
                "UnityEngine.Color",
                "UnityEngine.GameObject",
                //"UnityEngine.Texture",
                "UnityEngine.Transform",
                "UnityEngine.AssetBundle",
                //"UnityEngine.ParticleSystem",
                "UnityEngine.Renderer",
                //"UnityEngine.Material",
                //"UnityEngine.Font",
                "UnityEngine.Resources",
#if USING_UGUI
                "UnityEngine.Sprite",
                "UnityEngine.RectTransform",
                "UnityEngine.Canvas",
                "UnityEngine.CanvasGroup",
                "UnityEngine.Event",
                "UnityEngine.Events.UnityEvent",
                "UnityEngine.Events.UnityEventBase"
#endif
            };
        }

        // black list if white list not given
        /// <summary>
        /// UnityEngine.dll中类型黑名单列表，此列表中的类不会被导出
        /// </summary>
        /// <param name="list"></param>
        public static void OnGetEngineNoUseList(out List<string> list)
        {
            list = new List<string>
            {
                "HideInInspector",
                "ExecuteInEditMode",
                "AddComponentMenu",
                "ContextMenu",
                "RequireComponent",
                "DisallowMultipleComponent",
                "SerializeField",
                "AssemblyIsEditorAssembly",
                "Attribute",
                "Types",
                "UnitySurrogateSelector",
                "TrackedReference",
                "TypeInferenceRules",
                "FFTWindow",
                "RPC",
                "Network",
                "MasterServer",
                "BitStream",
                "HostData",
                "ConnectionTesterStatus",
                "GUI",
                "EventType",
                "EventModifiers",
                "FontStyle",
                "TextAlignment",
                "TextEditor",
                "TextEditorDblClickSnapping",
                "TextGenerator",
                "TextClipping",
                "Gizmos",
                "ADBannerView",
                "ADInterstitialAd",
                "Android",
                "Tizen",
                "jvalue",
                "iPhone",
                "iOS",
                "Windows",
                "CalendarIdentifier",
                "CalendarUnit",
                "CalendarUnit",
                "ClusterInput",
                "FullScreenMovieControlMode",
                "FullScreenMovieScalingMode",
                "Handheld",
                "LocalNotification",
                "NotificationServices",
                "RemoteNotificationType",
                "RemoteNotification",
                "SamsungTV",
                "TextureCompressionQuality",
                "TouchScreenKeyboardType",
                "TouchScreenKeyboard",
                "MovieTexture",
                "UnityEngineInternal",
                "Terrain",
                "Tree",
                "SplatPrototype",
                "DetailPrototype",
                "DetailRenderMode",
                "MeshSubsetCombineUtility",
                "AOT",
                "Social",
                "Enumerator",
                "SendMouseEvents",
                "Cursor",
                "Flash",
                "ActionScript",
                "OnRequestRebuild",
                "Ping",
                "ShaderVariantCollection",
                "SimpleJson.Reflection",
                "CoroutineTween",
                "GraphicRebuildTracker",
                "Advertisements",
                "UnityEditor",
                "WSA",
                "EventProvider",
                "Apple",
                "ClusterInput",
            };
        }

        /// <summary>
        /// 类型中不导出的方法名列表
        /// </summary>
        public static List<string> FunctionFilterList = new List<string>()
        {
            "UIWidget.showHandlesWithMoveTool",
            "UIWidget.showHandles",
            "UIInput.ProcessEvent",
            "UIPanel.GetMainGameViewSize",
            "UIPanel.OnDrawGizmos",
            "UIWidget.CreatePanel",
            "UIWidget.SetPanel",
            "UIWidget.FullCompareFunc",
            "UIWidget.PanelCompareFunc",
            "UIWidget.uid",
            "UIWidget.sid",
            "UnityEngine.Transform.RotateAround",
            "UnityEngine.Transform.LookAt",
            "UnityEngine.Transform.DetachChildren",
            "UnityEngine.Transform.eulerAngles",
            "UnityEngine.Transform.localEulerAngles",
            "UnityEngine.Transform.right",
            "UnityEngine.Transform.up",
            "UnityEngine.Transform.forward",
            "UnityEngine.Transform.lossyScale",
            "UnityEngine.GameObject.SampleAnimation",
            "UnityEngine.GameObject.CreatePrimitive",
            "UnityEngine.GameObject.FindGameObjectWithTag",
            "UnityEngine.GameObject.FindWithTag",
            "UnityEngine.GameObject.FindGameObjectsWithTag",
            "UnityEngine.GameObject.isStatic",
            "UnityEngine.GameObject.camera",
            "UnityEngine.GameObject.light",
            "UnityEngine.GameObject.animation",
            "UnityEngine.GameObject.constantForce",
            "UnityEngine.GameObject.audio",
            "UnityEngine.GameObject.guiText",
            "UnityEngine.GameObject.guiTexture",
            "UnityEngine.GameObject.hingeJoint",
            "UnityEngine.GameObject.particleEmitter",
            "UnityEngine.GameObject.particleSystem",
            "UnityEngine.Application.Quit",
            "UnityEngine.Application.CancelQuit",
            "UnityEngine.Application.LoadLevel",
            "UnityEngine.Application.LoadLevelAsync",
            "UnityEngine.Application.LoadLevelAdditiveAsync",
            "UnityEngine.Application.LoadLevelAdditive",
            "UnityEngine.Application.GetStreamProgressForLevel",
            "UnityEngine.Application.CanStreamedLevelBeLoaded",
            "UnityEngine.Application.HasProLicense",
            "UnityEngine.Application.RegisterLogCallback",
            "UnityEngine.Application.RegisterLogCallbackThreaded",
            "UnityEngine.Application.RequestUserAuthorization",
            "UnityEngine.Application.HasUserAuthorization",
            "UnityEngine.Application.loadedLevel",
            "UnityEngine.Application.loadedLevelName",
            "UnityEngine.Application.isLoadingLevel",
            "UnityEngine.Application.levelCount",
            "UnityEngine.Application.streamedBytes",
            "UnityEngine.Application.isPlaying",
            "UnityEngine.Application.isEditor",
            "UnityEngine.Application.isWebPlayer",
            "UnityEngine.Application.runInBackground",
            "UnityEngine.Application.unityVersion",
            "UnityEngine.Application.targetFrameRate",
            "UnityEngine.Application.backgroundLoadingPriority",
            "UnityEngine.Application.genuine",
            "UnityEngine.Application.genuineCheckAvailable",
            "UnityEngine.Component.rigidbody",
            "UnityEngine.Component.rigidbody2D",
            "UnityEngine.Component.camera",
            "UnityEngine.Component.light",
            "UnityEngine.Component.animation",
            "UnityEngine.Component.constantForce",
            "UnityEngine.Component.renderer",
            "UnityEngine.Component.audio",
            "UnityEngine.Component.guiText",
            "UnityEngine.Component.guiTexture",
            "UnityEngine.Component.collider",
            "UnityEngine.Component.collider2D",
            "UnityEngine.Component.hingeJoint",
            "UnityEngine.Component.particleEmitter",
            "UnityEngine.Component.particleSystem",
            "UnityEngine.Vector3.Set",
            "UnityEngine.Vector3.Scale",
            "UnityEngine.Vector3.Normalize",
            "UnityEngine.Vector3.Lerp",
            "UnityEngine.Vector3.Slerp",
            "UnityEngine.Vector3.OrthoNormalize",
            "UnityEngine.Vector3.MoveTowards",
            "UnityEngine.Vector3.RotateTowards",
            "UnityEngine.Vector3.SmoothDamp",
            "UnityEngine.Vector3.Scale",
            "UnityEngine.Vector3.Cross",
            "UnityEngine.Vector3.Reflect",
            "UnityEngine.Vector3.Normalize",
            "UnityEngine.Vector3.Dot",
            "UnityEngine.Vector3.Project",
            "UnityEngine.Vector3.ProjectOnPlane",
            "UnityEngine.Vector3.op_UnaryNegation",
            "UnityEngine.Vector3.op_Multiply",
            "UnityEngine.Vector3.op_Division",
            "UnityEngine.Vector3.op_Inequality",

            "UnityEngine.Vector2.Set",
            "UnityEngine.Vector2.Scale",
            "UnityEngine.Vector2.Normalize",
            "UnityEngine.Vector2.Lerp",
            "UnityEngine.Vector2.Slerp",
            "UnityEngine.Vector2.OrthoNormalize",
            "UnityEngine.Vector2.MoveTowards",
            "UnityEngine.Vector2.RotateTowards",
            "UnityEngine.Vector2.SmoothDamp",
            "UnityEngine.Vector2.Scale",
            "UnityEngine.Vector2.Cross",
            "UnityEngine.Vector2.Reflect",
            "UnityEngine.Vector2.Normalize",
            "UnityEngine.Vector2.Dot",
            "UnityEngine.Vector2.Project",
            "UnityEngine.Vector2.ProjectOnPlane",
            "UnityEngine.Vector2.op_UnaryNegation",
            "UnityEngine.Vector2.op_Multiply",
            "UnityEngine.Vector2.op_Division",
            "UnityEngine.Vector2.op_Inequality",

            "UnityEngine.Vector4.Set",
            "UnityEngine.Vector4.Scale",
            "UnityEngine.Vector4.Normalize",
            "UnityEngine.Vector4.Lerp",
            "UnityEngine.Vector4.Slerp",
            "UnityEngine.Vector4.OrthoNormalize",
            "UnityEngine.Vector4.MoveTowards",
            "UnityEngine.Vector4.RotateTowards",
            "UnityEngine.Vector4.SmoothDamp",
            "UnityEngine.Vector4.Scale",
            "UnityEngine.Vector4.Cross",
            "UnityEngine.Vector4.Reflect",
            "UnityEngine.Vector4.Normalize",
            "UnityEngine.Vector4.Dot",
            "UnityEngine.Vector4.Project",
            "UnityEngine.Vector4.ProjectOnPlane",
            "UnityEngine.Vector4.op_UnaryNegation",
            "UnityEngine.Vector4.op_Multiply",
            "UnityEngine.Vector4.op_Division",
            "UnityEngine.Vector4.op_Inequality",

            "UnityEngine.Color.Set",
            "UnityEngine.Color.Scale",
            "UnityEngine.Color.Normalize",
            "UnityEngine.Color.Lerp",
            "UnityEngine.Color.Slerp",
            "UnityEngine.Color.OrthoNormalize",
            "UnityEngine.Color.MoveTowards",
            "UnityEngine.Color.RotateTowards",
            "UnityEngine.Color.SmoothDamp",
            "UnityEngine.Color.Scale",
            "UnityEngine.Color.Cross",
            "UnityEngine.Color.Reflect",
            "UnityEngine.Color.Normalize",
            "UnityEngine.Color.Dot",
            "UnityEngine.Color.Project",
            "UnityEngine.Color.ProjectOnPlane",
            "UnityEngine.Color.op_UnaryNegation",
            "UnityEngine.Color.op_Multiply",
            "UnityEngine.Color.op_Division",
            "UnityEngine.Color.op_Inequality",
            "UnityEngine.MonoBehaviour.runInEditMode",
        };


    }
}