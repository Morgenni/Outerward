using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MapMagic;
using ParadoxNotion.Serialization.FullSerializer;
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Outerward
{
    [BepInPlugin(OuterwardPlugin.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class OuterwardPlugin : BaseUnityPlugin
    {
        internal const string PLUGIN_GUID = "com.Morgenni.Outerward";
        internal const string PLUGIN_NAME = "Outerward";
        internal const string PLUGIN_VERSION = "0.0.3";
        internal static new ManualLogSource Logger;
        static readonly Type[] serializeMCVDTypes = { typeof(CharacterSave),typeof(EnvironmentSave) };
        private void Awake()
        {
            Logger = base.Logger;
            Harmony harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
            for(int i=0;i<serializeMCVDTypes.Length;i++)
            {
                Type type = serializeMCVDTypes[i];
                if (!MajorElementSave.m_serializers.TryGetValue(type, out XmlSerializer v))
                {
                    XmlSerializer serializer = new XmlSerializer(type, new[] { typeof(ModdedCharacterVisualData) });
                    MajorElementSave.m_serializers[type] = serializer;
                }
            }
        }
        internal static bool ResourceLoaded = false;
        public static void InitializeDefaultRaces()
        {
            CharacterVisualsPresets og = CharacterManager.m_characterVisualsPresets;
            RaceData whiteRace = ModdedCharacterVisualData.GetRace("Outward.Auraian");
            whiteRace.SetRaceMaterial(new[] { og.MSkins[0], og.FSkins[0] });
            foreach (GameObject headObj in og.MHeadsWhite)
                whiteRace.AddBodyPart(RaceData.BodyPart.Head, headObj, Character.Gender.Male);
            foreach (GameObject headObj in og.FHeadsWhite)
                whiteRace.AddBodyPart(RaceData.BodyPart.Head, headObj, Character.Gender.Female);
            LocalizationManager.m_instance.m_generalLocalization["Outward.Auraian"] = LocalizationManager.m_instance.m_generalLocalization["CharacterCreation_White"];

            RaceData blackRace = ModdedCharacterVisualData.GetRace("Outward.Tramon");
            blackRace.SetRaceMaterial(new[] { og.MSkins[1], og.FSkins[1] });
            foreach (GameObject headObj in og.MHeadsBlack)
                blackRace.AddBodyPart(RaceData.BodyPart.Head, headObj, Character.Gender.Male);
            foreach (GameObject headObj in og.FHeadsBlack)
                blackRace.AddBodyPart(RaceData.BodyPart.Head, headObj, Character.Gender.Female);
            LocalizationManager.m_instance.m_generalLocalization["Outward.Tramon"]= LocalizationManager.m_instance.m_generalLocalization["CharacterCreation_Black"];

            RaceData asianRace = ModdedCharacterVisualData.GetRace("Outward.Kazite");
            asianRace.SetRaceMaterial(new[] { og.MSkins[2], og.FSkins[2] });
            foreach (GameObject headObj in og.MHeadsAsian)
                asianRace.AddBodyPart(RaceData.BodyPart.Head, headObj, Character.Gender.Male);
            foreach (GameObject headObj in og.FHeadsAsian)
                asianRace.AddBodyPart(RaceData.BodyPart.Head, headObj, Character.Gender.Female);
            LocalizationManager.m_instance.m_generalLocalization["Outward.Kazite"] = LocalizationManager.m_instance.m_generalLocalization["CharacterCreation_Asian"];

            foreach(string mod in Directory.GetDirectories(Paths.PluginPath,"*",SearchOption.TopDirectoryOnly))
            {
                string outerwardFolder = mod + @"\Outerward";
                if (Directory.Exists(outerwardFolder))
                    foreach (string file in Directory.GetFiles(outerwardFolder, "*.xml"))
                    {
                        string[] raceFilename = file.Split('\\').Last().Split(['.']);
                        string modName = mod.Split('\\').Last().Split('-').Last();
                        string raceGUID = $"{modName}.{raceFilename[raceFilename.Length - 2]}";
                        RaceData race = new(raceGUID);
                        race.FromXml(File.ReadAllText(file));
                        ModdedCharacterVisualData.AddRace(race);
                    }
            }
            ResourceLoaded = true;
        }
    }
}
