using System;
using System.Collections.Generic;
using UnityEngine;

namespace KA.Framework;

public class AvatarSettings : ScriptableObject
{
	[Serializable]
	public class BoneSettings
	{
		public string LEGS_PARENT_BONE = "";

		public string FOOTL_PARENT_BONE = "";

		public string FOOTR_PARENT_BONE = "";

		public string TOP_PARENT_BONE = "";

		public string HEAD_PARENT_BONE = "";

		public string HAIR_PARENT_BONE = "";

		public string HAT_PARENT_BONE = "";

		public string WRISTBANDL_PARENT_BONE = "";

		public string WRISTBANDR_PARENT_BONE = "";

		public string HANDLEFT_PARENT_BONE = "";

		public string HANDRIGHT_PARENT_BONE = "";

		public string SHOULDERPADLEFT_PARENT_BONE = "";

		public string SHOULDERPADRIGHT_PARENT_BONE = "";

		public string WING_PARENT_BONE = "";

		public string TAIL_PARENT_BONE = "";

		public string SWIMTAIL_PARENT_BONE = "Legs";

		public string HAND_PROP_RIGHT_BONE = "";

		public string BACK_PARENT_BONE = "";

		public string BUSY_PROP_BONE = "";
	}

	[Serializable]
	public class TextureSettings
	{
		public string TEXTURE_TYPE_EYES_OPEN = "EyesOpen";

		public string TEXTURE_TYPE_EYES_CLOSED = "EyesClosed";

		public string TEXTURE_TYPE_MOUTH_OPEN = "MouthOpen";

		public string TEXTURE_TYPE_MOUTH_CLOSED = "MouthClosed";

		public string TEXTURE_TYPE_SKIN = "Skin";

		public string TEXTURE_TYPE_STYLE = "Style";

		public string TEXTURE_TYPE_STYLE_M = "StyleM";

		public string TEXTURE_TYPE_STYLE_F = "StyleF";

		public string TEXTURE_TYPE_STYLER = "StyleR";

		public string TEXTURE_TYPE_MASK = "Mask";

		public string TEXTURE_TYPE_BUMP = "BumpMap";

		public string TEXTURE_TYPE_HIGHLIGHT = "Highlight";

		public string TEXTURE_TYPE_NUMBER = "Numbers";
	}

	[Serializable]
	public class ShaderSettings
	{
		public string SHADER_PROP_EYES = "_MainTex2";

		public string SHADER_PROP_MOUTH = "_MainTex3";

		public string SHADER_PROP_SKIN = "_MultiLayerGlobal";

		public string SHADER_PROP_PART = "_MainTex";

		public string SHADER_PROP_NON_GLOBAL_SKIN = "_Skin";

		public string SHADER_PROP_RANK_COLOR = "_Color";

		public string SHADER_PROP_COLOR = "_Color";

		public string SHADER_DIFFUSE = "Diffuse";

		public string SHADER_NON_PRIMARY_2_LAYER = "Player-2Layer-AlphaBlend";

		public string SHADER_NON_PRIMARY_4_LAYER = "Player-4Layer-AlphaBlend";

		public string SHADER_PROP_SHIELD_DECAL = "_DecalTex";
	}

	[Serializable]
	public class AvatarPartSettings
	{
		public string AVATAR_PART_LEGS = "Legs";

		public string AVATAR_PART_FEET = "Feet";

		public string AVATAR_PART_TOP = "Torso";

		public string AVATAR_PART_HEAD = "Head";

		public string AVATAR_PART_HAIR = "Hair";

		public string AVATAR_PART_HAT = "Hat";

		public string AVATAR_PART_EYES = "Eyes";

		public string AVATAR_PART_MOUTH = "Mouth";

		public string AVATAR_PART_SKIN = "Skin";

		public string AVATAR_PART_SCAR = "Scar";

		public string AVATAR_PART_FACE_DECAL = "Face_Decal";

		public string AVATAR_PART_FACEMASK = "FaceMask";

		public string AVATAR_PART_FOOT_LEFT = "FootL";

		public string AVATAR_PART_FOOT_RIGHT = "FootR";

		public string AVATAR_PART_WRISTBAND = "WristBand";

		public string AVATAR_PART_WRISTBAND_LEFT = "WristbandL";

		public string AVATAR_PART_WRISTBAND_RIGHT = "WristbandR";

		public string AVATAR_PART_HAND = "Hand";

		public string AVATAR_PART_HAND_LEFT = "HandL";

		public string AVATAR_PART_HAND_RIGHT = "HandR";

		public string AVATAR_PART_SHOULDERPAD = "ShoulderPad";

		public string AVATAR_PART_SHOULDERPAD_LEFT = "ShoulderPadL";

		public string AVATAR_PART_SHOULDERPAD_RIGHT = "ShoulderPadR";

		public string AVATAR_PART_TAIL = "Tail";

		public string AVATAR_PART_WING = "Wing";

		public string AVATAR_PART_GLOVES = "Gloves";

		public string AVATAR_PART_GLOVE_LEFT = "GloveL";

		public string AVATAR_PART_GLOVE_RIGHT = "GloveR";

		public string AVATAR_PART_HAND_PROP_RIGHT = "HandPropR";

		public string AVATAR_PART_BACK = "Back";
	}

	[Serializable]
	public class GeneralSettings
	{
		public int PART_LEFT_INDEX;

		public int PART_RIGHT_INDEX = 1;

		public int OPEN_INDEX;

		public int CLOSED_INDEX = 1;

		public int PRIMARY_INDEX;

		public int EYES_OFFSET_INDEX;

		public int MOUTH_OFFSET_INDEX = 1;

		public int COLOR_INDEX = 1;

		public string BLINK_PLANE_MALE = "BlinkPlaneM";

		public string BLINK_PLANE_FEMALE = "BlinkPlaneF";

		public string ICON_MEMBER = "IcoProfileMember";

		public string ICON_NONMEMBER = "IcoProfileNonMember";
	}

	[Serializable]
	public class DefaultSettings
	{
		public string DEFAULT_LEGS_GEOM_RES = "";

		public string DEFAULT_LEGS_TEX_RES = "";

		public string DEFAULT_FOOTL_GEOM_RES = "";

		public string DEFAULT_FOOTR_GEOM_RES = "";

		public string DEFAULT_FOOTL_TEX_RES = "";

		public string DEFAULT_FOOTR_TEX_RES = "";

		public string DEFAULT_WRISTBANDL_GEOM_RES = "";

		public string DEFAULT_WRISTBANDR_GEOM_RES = "";

		public string DEFAULT_WRISTBAND_TEX_RES = "";

		public string DEFAULT_HANDL_GEOM_RES = "";

		public string DEFAULT_HANDR_GEOM_RES = "";

		public string DEFAULT_HANDL_TEX_RES = "";

		public string DEFAULT_HANDR_TEX_RES = "";

		public string DEFAULT_TOP_GEOM_RES = "";

		public string DEFAULT_TOP_TEX_RES = "";

		public string DEFAULT_HAT_GEOM_RES = "";

		public string DEFAULT_HAT_TEX_RES = "";

		public string DEFAULT_FACEMASK_GEOM_RES = "";

		public string DEFAULT_FACEMASK_TEX_RES = "";

		public string DEFAULT_HEAD_GEOM_RES = "";

		public string DEFAULT_HEAD_TEX_RES = "";

		public string DEFAULT_HEAD_TEX_MASK_RES = "";

		public string DEFAULT_HEAD_TEX_DECAL1 = "";

		public string DEFAULT_HEAD_TEX_DECAL2 = "";

		public Color DEFAULT_SKIN_COLOR = new Color(0f, 0f, 0f, 0f);

		public Color DEFAULT_HAIR_COLOR = new Color(0f, 0f, 0f, 0f);

		public Color DEFAULT_EYE_COLOR = new Color(0f, 0f, 0f, 0f);

		public Color DEFAULT_WARPAINT_COLOR = new Color(0f, 0f, 0f, 0f);

		public string DEFAULT_HAIR_GEOM_RES = "";

		public string DEFAULT_HAIR_TEX_RES = "";

		public string DEFAULT_HAIR_TEX_MASK_RES = "";

		public string DEFAULT_HAIR_TEX_HIGH_RES = "";

		public string DEFAULT_EYE_TEX_RES = "";

		public string DEFAULT_EYE_TEX_MASK_RES = "";

		public string DEFAULT_MOUTH_OPEN_TEX_RES = "";

		public string DEFAULT_MOUTH_CLOSE_TEX_RES = "";

		public string DEFAULT_SKIN_TEX_RES = "";

		public string DEFAULT_HAND_PROP_RIGHT_RES = "";

		public string DEFAULT_BACK_RES = "";
	}

	[Serializable]
	public class CustomAvatarSettings
	{
		[Serializable]
		public class ShaderPropertyOverride
		{
			public string _Shader;

			public string _Property;

			public string _Override;
		}

		[Serializable]
		public class PartShaderMap
		{
			public string _TextureType;

			public Shader _MobileShader;

			public Shader _Shader;
		}

		public string DETAIL = "_DetailTex";

		public string DETAILEYES = "_DetailEyes";

		public string MASK = "_DetailMask";

		public string COLOR_MASK = "_ColorMask";

		public string BUMP = "_BumpMap";

		public string HIGHLIGHT = "_Highlight";

		public string EYEMASK = "_EyeColorMask";

		public string SKIN_COLOR = "_SkinColor";

		public string HAIR_COLOR = "_SkinColor";

		public string EYE_COLOR = "_EyeColor";

		public string WAR_COLOR = "_WarColor";

		public string DECAL1 = "_Decal1";

		public string DECAL2 = "_Decal2";

		public int SKINCOLOR_INDEX;

		public int HAIRCOLOR_INDEX = 1;

		public int EYECOLOR_INDEX = 2;

		public int WARPAINTCOLOR_INDEX = 3;

		public List<ShaderPropertyOverride> _ShaderPropertyOverride = new List<ShaderPropertyOverride>();

		public List<PartShaderMap> _PartsShaderMap = new List<PartShaderMap>();

		public List<string> _CustomizeTextureTypes = new List<string>();

		public List<string> _ValidEyeTextures = new List<string>();
	}

	public string _AvatarName = "Avatar";

	public string _SpriteName = "Sprite";

	public string _MainRootName = "Main_Root";

	public BoneSettings _BoneSettings;

	public TextureSettings _TextureSettings;

	public ShaderSettings _ShaderSettings;

	public AvatarPartSettings _AvatarPartSettings;

	public GeneralSettings _GeneralSettings;

	public DefaultSettings _MaleSettings;

	public DefaultSettings _FemaleSettings;

	public CustomAvatarSettings _CustomAvatarSettings;

	public List<string> _PartVisibilityAvatarExcludeList = new List<string>();

	public List<string> _ItemCustomizationAvatarExcludeList = new List<string>();

	private static AvatarSettings mInstance;

	public static AvatarSettings pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (AvatarSettings)RsResourceManager.LoadAssetFromResources("AvatarSettings.asset", isPrefab: false);
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<AvatarSettings>();
				}
			}
			return mInstance;
		}
	}
}
