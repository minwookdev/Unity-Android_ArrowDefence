﻿namespace ActionCat.Data {
    public static class CCPlayerData {
        public static AD_Inventory      inventory  = new AD_Inventory();
        public static Player_Equipments equipments = new Player_Equipments();
        public static PlayerAbility     ability    = new PlayerAbility();
        public static PlayerInfo        infos      = new PlayerInfo();
        public static GameSettings      settings   = new GameSettings();

        static readonly string KEY_INVENTORY = "KEY_INVENTORY";
        static readonly string KEY_EQUIPMENT = "KEY_EQUIPMENT";
        static readonly string KEY_INFOS     = "KEY_INFOS";
        static readonly string KEY_SETTINGS  = "KEY_SETTINGS";

        public static bool IsSaveExistence = false;

        public static void CheckSaveData() {
            if (IsSaveExistence) return;

            //세이브 데이터를 불러오지 못한 경우 EX)세이브 데이터 없음. 
            //게임진행에 필요한 최소의 조건 충족 EX)기초 아이템 지급.
            infos.AddCraftSlot(3); //초기에 3개 오픈
            infos.OpenSlot(0, 1, 2);

            IsSaveExistence = true;
        }

        public static void SaveUserData() {
            //ES3.Save<AD_Inventory>(inventoryKey, inventory);

            try {
                ES3.Save(KEY_INVENTORY, inventory);
                ES3.Save(KEY_EQUIPMENT, equipments);
                ES3.Save(KEY_SETTINGS, settings);
                ES3.Save(KEY_INFOS, infos);

                CatLog.Log(StringColor.GREEN, "Success Save All User Data.");
            }
            catch (System.Exception exception) {
                CatLog.ELog("UserData 저장 실패.");
                throw exception;
            }
        }

        public static void LoadUserData() {
            //Load Inventory
            if (ES3.KeyExists(KEY_INVENTORY))
            {
                inventory = ES3.Load<AD_Inventory>(KEY_INVENTORY);
                //inventory = ES3.Load(KEY_INVENTORY, inventory.invenList);
                //ES3.LoadInto<AD_item>(KEY_INVENTORY, inventory.invenList);
                //ES3.Load
                //inventory.AddRangeItem(ES3.Load<List<AD_item>>(KEY_INVENTORY));
                CatLog.Log("성공적으로 Inventory를 불러왔습니다.");
            }
            else CatLog.WLog("ES3 inventory KEY값이 없습니다.");

            //Load Equipments
            if (ES3.KeyExists(KEY_EQUIPMENT))
            {
                equipments = ES3.Load<Player_Equipments>(KEY_EQUIPMENT);
                CatLog.Log("성공적으로 Equipments를 불러왔습니다.");
            }
            else CatLog.WLog("ES3 equipment KEY값이 없습니다.");

            //Load Player information
            if (ES3.KeyExists(KEY_INFOS)) {
                infos = ES3.Load<PlayerInfo>(KEY_INFOS);
                CatLog.Log(StringColor.GREEN, "성공적으로 Infos Data를 불러왔습니다.");
            }
            else CatLog.WLog("ES3 Player Information Key값이 존재하지 않습니다.");

            //Load Settings -> 세이브 데이터에 넣는 방식이 아닌 기기내부에만 저장되도록 변경.
            if (ES3.KeyExists(KEY_SETTINGS))
            {
                settings = ES3.Load<GameSettings>(KEY_SETTINGS);
                CatLog.Log("성공적으로 Settings를 불러왔습니다.");
            }
            else CatLog.WLog("ES3 Game Settings KEY값이 없습니다.");

            //데이터 로드하는거 하나로 묶어주고, 데이터 없음 판정에서는 false처리 해주기
            IsSaveExistence = true;
        }

        public static void TEST_CREATE_TEMP_CRAFTING_SLOT() {
            infos.AddCraftSlot(3);
            infos.OpenSlot(0);
            infos.OpenSlot(1);
            infos.OpenSlot(2);
        }
    }
}
