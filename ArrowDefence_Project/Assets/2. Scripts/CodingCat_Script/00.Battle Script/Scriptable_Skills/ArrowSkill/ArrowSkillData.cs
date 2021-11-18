﻿namespace ActionCat
{
    using UnityEngine;

    public class ArrowSkillData : ScriptableObject
    {
        //Basic Arrow Skill Data
        //MEMBER
        public string SkillId;
        public string SkillName;
        public string SkillDesc;
        public SKILL_LEVEL SkillLevel;
        public ARROWSKILL SkillType;
        public Sprite IconSprite;
        protected ArrowSkill skillData = null;
        public ARROWSKILL_ACTIVETYPE ActiveType;

        //PROPERTIES
        public ArrowSkill ArrowSkill {
            get {
                if (skillData != null)
                    return skillData;
                else
                    return null;
            }
        }
    }
}
