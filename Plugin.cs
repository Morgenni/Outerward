using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MapMagic;
using ParadoxNotion.Serialization.FullSerializer;
using System;
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

            RaceData skeletonRace = ModdedCharacterVisualData.GetRace($"{PLUGIN_NAME}.Skeleton");
            GameObject
                skeletonHead = ResourcesPrefabManager.Instance.GetItemVisualPrefab("Assets/_Prefabs/_ItemsAssets/3000000_Armors/3200031_SkeletonHelm_e_v").gameObject,
                skeletonBody = ResourcesPrefabManager.Instance.GetItemVisualPrefab("Assets/_Prefabs/_ItemsAssets/3000000_Armors/3200030_Skeleton_e_v").gameObject,
                skeletonFeet = ResourcesPrefabManager.Instance.GetItemVisualPrefab("Assets/_Prefabs/_ItemsAssets/3000000_Armors/3200032_SkeletonBoots_e_v").gameObject;
            skeletonRace.AddBodyPart(RaceData.BodyPart.Head,  skeletonHead);
            skeletonRace.AddBodyPart(RaceData.BodyPart.Body,  skeletonBody);
            skeletonRace.AddBodyPart(RaceData.BodyPart.Feet,  skeletonFeet);
            skeletonRace.ForceDefaultBody = true;
            skeletonRace.ForceDefaultFeet = true;
            skeletonRace.UseRaceMaterial = false;
            Material transparentMaterial = skeletonBody.GetComponent<SkinnedMeshRenderer>().material;
            skeletonRace.SetRaceMaterial(CharacterVisualsExtensions.MakeTransparent(transparentMaterial));
            skeletonRace.Name="Mertonfolk";
            ResourceLoaded = true;
        }
    }
}
