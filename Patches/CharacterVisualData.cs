using HarmonyLib;
using UnityEngine;

namespace Outerward
{
    public struct RaceInfo
    {
        public int hairStyle, hairColor, skinIndex, faceIndex, raceGUID;
        public Character.Gender gender;
        public override string ToString()
        {
            string text = 
                ((int)gender).ToString() + ";" +
                hairStyle.ToString() + ";" +
                hairColor.ToString() + ";" +
                skinIndex.ToString() + ";" +
                faceIndex.ToString() + ";" +
                raceGUID.ToString() + ";";
            return text;
        }
    }
    public static class CharacterVisualDataExtensions
    {
        public static bool isModded(this CharacterVisualData __instance)
        {
            return __instance.SkinIndex == -1;
        }
        public static RaceInfo GetNormalized(this CharacterVisualData __instance)
        {
            RaceInfo raceInfo = new()
            {
                hairStyle = Mathf.Clamp(__instance.HairStyleIndex, 0, 14),
                hairColor = Mathf.Clamp(__instance.HairColorIndex, 0, 10),
                skinIndex = Mathf.Clamp(__instance.SkinIndex, -999, 2),
                faceIndex = Mathf.Clamp(__instance.HeadVariationIndex, 0, 1),
                gender = __instance.Gender
            };
            return raceInfo;
        }
    }
    [HarmonyPatch(typeof(CharacterVisualData))]
    public static class CharacterVisualDataPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("ToNetworkData")]
        public static bool ToNetworkData(CharacterVisualData __instance, ref string __result)
        {
            if (!__instance.isModded())//ADD SERVER CHECK NEXT
                return true;
            RaceInfo raceInfo = __instance.GetNormalized();
            __result = Encryptor.EncodeString(raceInfo.ToString());
            return false;
        }
    }
}