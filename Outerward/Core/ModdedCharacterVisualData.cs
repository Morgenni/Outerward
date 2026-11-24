using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Outerward
{
    [Serializable]
    [XmlType(TypeName = "ModdedCharacterVisualData",Namespace = "")]
    public class ModdedCharacterVisualData : CharacterVisualData
    {
        [XmlElement("Race")]
        public string RaceGUID = "";
        public static string FromSkinIndex(int skinIndex)=> skinIndex switch
        {
            0 => "Outward.Auraian",
            1 => "Outward.Tramon",
            2 => "Outward.Kazite",
            -999 => "Outward.Troglodyte",
            _ => "Outward.Auraian"
        };
        internal static ModdedCharacterVisualData FromVanillaData(CharacterVisualData oldData)
        {
            ModdedCharacterVisualData data = new()
            {
                RaceGUID = FromSkinIndex(oldData.SkinIndex),
                SkinIndex = oldData.SkinIndex,
                HeadVariationIndex = oldData.HeadVariationIndex,
                HairStyleIndex = oldData.HairStyleIndex,
                HairColorIndex = oldData.HairColorIndex,
                Gender = oldData.Gender
            };
            return data;
        }
        internal static List<string> RacesByIndex = [];
        internal static Dictionary<string, RaceData> modRaces = [];
        public static RaceData GetRace(string GUID)
        {
            if (GUID == "")
                return null;
            if (!modRaces.ContainsKey(GUID))
            {
                OuterwardPlugin.Logger.LogMessage($"Race registered: {GUID}");
                modRaces.Add(GUID, new RaceData(GUID));
                RacesByIndex.Add(GUID);
            }
            return modRaces[GUID];
        }
        public void CopyFromClass(object copy)
        {
            CopyFrom(copy as CharacterVisualData);
            if (copy.GetType() == typeof(ModdedCharacterVisualData))
                RaceGUID = (copy as ModdedCharacterVisualData).RaceGUID;
            else
                RaceGUID = FromSkinIndex(SkinIndex);
            if (!RaceGUID.StartsWith("Outward."))
                SkinIndex = -1;
        }
        public ModdedCharacterVisualData(){}
        public ModdedCharacterVisualData(ModdedCharacterVisualData _copyFrom) => CopyFromClass(_copyFrom);
        public ModdedCharacterVisualData(CharacterVisualData _copyFrom) => CopyFromClass(_copyFrom);
    }
}
