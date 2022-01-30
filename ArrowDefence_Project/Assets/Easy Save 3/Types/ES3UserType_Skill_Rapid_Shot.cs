using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("arrowCount", "shotDelay", "muzzleEffect", "id", "name", "desc", "iconSprite", "level", "skillType")]
	public class ES3UserType_Skill_Rapid_Shot : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Skill_Rapid_Shot() : base(typeof(ActionCat.Skill_Rapid_Shot)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (ActionCat.Skill_Rapid_Shot)obj;
			
			writer.WritePrivateField("arrowCount", instance);
			writer.WritePrivateField("shotDelay", instance);
			writer.WritePrivateFieldByRef("muzzleEffect", instance);
			writer.WritePrivateField("id", instance);
			writer.WritePrivateField("name", instance);
			writer.WritePrivateField("desc", instance);
			writer.WritePrivateFieldByRef("iconSprite", instance);
			writer.WritePrivateField("level", instance);
			writer.WritePrivateField("skillType", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (ActionCat.Skill_Rapid_Shot)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "arrowCount":
					reader.SetPrivateField("arrowCount", reader.Read<System.Byte>(), instance);
					break;
					case "shotDelay":
					reader.SetPrivateField("shotDelay", reader.Read<System.Single>(), instance);
					break;
					case "muzzleEffect":
					reader.SetPrivateField("muzzleEffect", reader.Read<ActionCat.ACEffector2D>(), instance);
					break;
					case "id":
					reader.SetPrivateField("id", reader.Read<System.String>(), instance);
					break;
					case "name":
					reader.SetPrivateField("name", reader.Read<System.String>(), instance);
					break;
					case "desc":
					reader.SetPrivateField("desc", reader.Read<System.String>(), instance);
					break;
					case "iconSprite":
					reader.SetPrivateField("iconSprite", reader.Read<UnityEngine.Sprite>(), instance);
					break;
					case "level":
					reader.SetPrivateField("level", reader.Read<ActionCat.SKILL_LEVEL>(), instance);
					break;
					case "skillType":
					reader.SetPrivateField("skillType", reader.Read<ActionCat.BOWSKILL_TYPE>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new ActionCat.Skill_Rapid_Shot();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_Skill_Rapid_ShotArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_Skill_Rapid_ShotArray() : base(typeof(ActionCat.Skill_Rapid_Shot[]), ES3UserType_Skill_Rapid_Shot.Instance)
		{
			Instance = this;
		}
	}
}