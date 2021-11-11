using System;
using System.Collections.Generic;
namespace com.tencent.pandora {
	[LuaBinder(1)]
	public class BindUnityUI {
		public static Action<IntPtr>[] GetBindList() {
			Action<IntPtr>[] list= {
				Lua_UnityEngine_EventSystems_UIBehaviour.reg,
				Lua_UnityEngine_UI_Selectable.reg,
				Lua_UnityEngine_UI_Button.reg,
				Lua_UnityEngine_UI_Button_ButtonClickedEvent.reg,
				Lua_UnityEngine_UI_Dropdown.reg,
				Lua_UnityEngine_UI_Dropdown_DropdownEvent.reg,
				Lua_UnityEngine_UI_Graphic.reg,
				Lua_UnityEngine_UI_MaskableGraphic.reg,
				Lua_UnityEngine_UI_Image.reg,
				Lua_UnityEngine_UI_Image_FillMethod.reg,
				Lua_UnityEngine_UI_Image_Type.reg,
				Lua_UnityEngine_UI_InputField.reg,
				Lua_UnityEngine_UI_InputField_OnChangeEvent.reg,
				Lua_UnityEngine_UI_InputField_SubmitEvent.reg,
				Lua_UnityEngine_UI_InputField_LineType.reg,
				Lua_UnityEngine_UI_InputField_CharacterValidation.reg,
				Lua_UnityEngine_UI_InputField_InputType.reg,
				Lua_UnityEngine_UI_InputField_ContentType.reg,
				Lua_UnityEngine_UI_Mask.reg,
				Lua_UnityEngine_UI_MaskableGraphic_CullStateChangedEvent.reg,
				Lua_UnityEngine_UI_RawImage.reg,
				Lua_UnityEngine_UI_Scrollbar.reg,
				Lua_UnityEngine_UI_Scrollbar_ScrollEvent.reg,
				Lua_UnityEngine_UI_Scrollbar_Direction.reg,
				Lua_UnityEngine_UI_ScrollRect.reg,
				Lua_UnityEngine_UI_ScrollRect_ScrollRectEvent.reg,
				Lua_UnityEngine_UI_ScrollRect_ScrollbarVisibility.reg,
				Lua_UnityEngine_UI_ScrollRect_MovementType.reg,
				Lua_UnityEngine_UI_Selectable_Transition.reg,
				Lua_UnityEngine_UI_Slider.reg,
				Lua_UnityEngine_UI_Slider_SliderEvent.reg,
				Lua_UnityEngine_UI_Slider_Direction.reg,
				Lua_UnityEngine_UI_Text.reg,
				Lua_UnityEngine_UI_Toggle.reg,
				Lua_UnityEngine_UI_Toggle_ToggleEvent.reg,
				Lua_UnityEngine_UI_Toggle_ToggleTransition.reg,
				Lua_UnityEngine_UI_ToggleGroup.reg,
				Lua_UnityEngine_UI_LayoutGroup.reg,
				Lua_UnityEngine_UI_GridLayoutGroup.reg,
				Lua_UnityEngine_UI_GridLayoutGroup_Constraint.reg,
				Lua_UnityEngine_UI_GridLayoutGroup_Axis.reg,
				Lua_UnityEngine_UI_GridLayoutGroup_Corner.reg,
				Lua_UnityEngine_UI_HorizontalOrVerticalLayoutGroup.reg,
				Lua_UnityEngine_UI_HorizontalLayoutGroup.reg,
				Lua_UnityEngine_UI_LayoutRebuilder.reg,
				Lua_UnityEngine_UI_LayoutUtility.reg,
				Lua_UnityEngine_UI_VerticalLayoutGroup.reg,
				Lua_UnityEngine_UI_BaseMeshEffect.reg,
				Lua_UnityEngine_UI_Shadow.reg,
				Lua_UnityEngine_UI_Outline.reg,
			};
			return list;
		}
	}
}
