using HarmonyLib;
using Outerward.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Outerward
{
    public static class CharacterCreationPanelExtensions
    {
        public static void RefreshHairIndex(this CharacterCreationPanel __instance)
        {
            string skinColorIndex = __instance.m_skinSelector.m_stringSelector.Value;
            RaceData race = ModdedCharacterVisualData.GetRace(skinColorIndex);
            List<GameObject> hairList = race.GetParts(RaceData.BodyPart.Hair, __instance.m_gender) ?? CharacterManager.m_characterVisualsPresets.Hairs.ToList();
            __instance.m_hairStyleSelector.MaxLimit = hairList.Count;
            if (__instance.m_hairStyleIndex >= __instance.m_hairStyleSelector.MaxLimit)
            {
                __instance.m_hairStyleIndex = __instance.m_hairStyleSelector.MaxLimit - 1;
                __instance.m_hairStyleSelector.SetValue(__instance.m_hairStyleIndex + 1);
            }
        }
        public static void RefreshHairColorIndex(this CharacterCreationPanel __instance)
        {
            string skinColorIndex = __instance.m_skinSelector.m_stringSelector.Value;
            RaceData race = ModdedCharacterVisualData.GetRace(skinColorIndex);
            Material[] hairColorList = race.GetParts<Material>(RaceData.BodyPart.HairColor, __instance.m_gender)?.ToArray() ?? CharacterManager.m_characterVisualsPresets.HairMaterials;
            __instance.m_hairColorSelector.MaxLimit = hairColorList.Length;
            if (__instance.m_hairColorIndex >= __instance.m_hairColorSelector.MaxLimit)
            {
                __instance.m_hairColorIndex = __instance.m_hairColorSelector.MaxLimit - 1;
                __instance.m_hairColorSelector.SetValue(__instance.m_hairColorIndex + 1);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterCreationPanel))]
    public static class CharacterCreationPanelPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("RefreshCharPreview")]
        public static bool RefreshCharPreview(CharacterCreationPanel __instance)
        {
            __instance.m_refreshPreviewWanted = false;
            __instance.m_characterVisuals.ResetDataCharacterCreation();

            string race = __instance.m_skinSelector.m_stringSelector.Value;
            __instance.m_characterVisuals.LoadCharacterCreationHead(race, (int)__instance.m_gender, __instance.m_headVariationIndex);
            __instance.m_characterVisuals.LoadCharacterCreationHair(race, (int)__instance.m_gender, __instance.m_hairStyleIndex, __instance.m_hairColorIndex);

            bool isMale = __instance.m_gender == Character.Gender.Male;
            if (__instance.m_gearBodyMale.gameObject.activeSelf != isMale)
                __instance.m_gearBodyMale.gameObject.SetActive(isMale);
            if (__instance.m_gearBodyFemale.gameObject.activeSelf != !isMale)
                __instance.m_gearBodyFemale.gameObject.SetActive(!isMale);
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch("RefreshHeadIndex")]
        public static bool RefreshHeadIndex(CharacterCreationPanel __instance)
        {
            string skinColorIndex = __instance.m_skinSelector.m_stringSelector.Value;
            RaceData race = ModdedCharacterVisualData.GetRace(skinColorIndex);
            List<GameObject> faceList = race.GetParts(RaceData.BodyPart.Head, __instance.m_gender) ?? CharacterManager.m_characterVisualsPresets.MHeadsBlack.ToList();
            __instance.m_faceSelector.MaxLimit = faceList.Count;
            if (__instance.m_headVariationIndex >= __instance.m_faceSelector.MaxLimit)
            {
                __instance.m_headVariationIndex = __instance.m_faceSelector.MaxLimit - 1;
                __instance.m_faceSelector.SetValue(__instance.m_headVariationIndex + 1);
            }
            return false;
        }
        [HarmonyPostfix]
        [HarmonyPatch("StartInit")]
        public static void StartInit(CharacterCreationPanel __instance)
        {
            int totalRaces = CharacterManager.CharacterVisualsPresets.GetTotalRaces();
            __instance.m_skinSelector.m_stringSelector.m_content = ModdedCharacterVisualData.modRaces.Keys.ToArray();
            __instance.m_skinSelector.MaxLimit = totalRaces;
        }
        [HarmonyPostfix]
        [HarmonyPatch("OnHairStyleChanged")]
        public static void OnHairStyleChanged(CharacterCreationPanel __instance, int _index)=> __instance.RefreshHairIndex();
        [HarmonyPostfix]
        [HarmonyPatch("OnHairColorChanged")]
        public static void OnHairColorChanged(CharacterCreationPanel __instance, int _index)=> __instance.RefreshHairColorIndex();
        [HarmonyPostfix]
        [HarmonyPatch("OnSkinChanged")]
        [HarmonyPatch("OnSexeChanged")]
        public static void OnCharChanged(CharacterCreationPanel __instance, int _index)
        {
            __instance.RefreshHairIndex();
            __instance.RefreshHairColorIndex();
        }
        [HarmonyPrefix]
        [HarmonyPatch("OnConfirmSave")]
        public static bool OnConfirmSave(CharacterCreationPanel __instance)
        {
            __instance.m_creationConfirmed = true;
            CharacterSave characterSave = SaveManager.Instance.CreateNewCharacterSave(true);
            characterSave.PSave.Name = __instance.m_charName;
            ModdedCharacterVisualData visualData = new ModdedCharacterVisualData()
            {
                Gender = __instance.m_gender,
                RaceGUID = __instance.m_skinSelector.m_stringSelector.Value,
                SkinIndex = __instance.m_skinColorIndex,
                HeadVariationIndex = __instance.m_headVariationIndex,
                HairColorIndex = __instance.m_hairColorIndex,
                HairStyleIndex = __instance.m_hairStyleIndex
            };
            characterSave.PSave.VisualData = visualData;
            characterSave.PSave.Food = 800f;
            characterSave.PSave.Drink = 800f;
            characterSave.PSave.HardcoreMode = CharacterManager.Instance.HardcoreMode;
            if (__instance.m_legacy != null)
            {
                List<LegacyChestData> list = new List<LegacyChestData>();
                for (int i = 0; i < __instance.m_legacy.LegacyChestSave.LegacyChestList.Count; i++)
                {
                    list.Add(LegacyChestData.InheritLegacy(__instance.m_legacy.LegacyChestSave.LegacyChestList[i]));
                    if (list[list.Count - 1].StoredItemID == 4000220)
                        characterSave.PSave.VisualData.SkinIndex = -999;
                }
                characterSave.PSave.PendingLegacyData = ((list.Count > 0) ? list : null);
            }
            __instance.onSaveSelected.Invoke(characterSave);
            return false;
        }
    }
}
