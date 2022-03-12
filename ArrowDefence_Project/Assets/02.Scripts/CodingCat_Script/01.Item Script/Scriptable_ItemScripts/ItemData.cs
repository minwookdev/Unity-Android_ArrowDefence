﻿namespace ActionCat
{
    using UnityEngine;

    [System.Serializable]
    public abstract class ItemData : ScriptableObject {
        [ReadOnly]
        public ITEMTYPE Item_Type;
        public ITEMGRADE Item_Grade;
        public string Item_Id;
        public int Item_Amount;
        public string Item_Name;
        public string Item_Desc;
        public Sprite Item_Sprite;
    }
}
