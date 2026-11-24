using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace Outerward
{
    public static class CharacterVisualsExtensions
    {
        public static bool HasSkinMaterial(this CharacterVisuals __instance, EquipmentSlot.EquipmentSlotIDs slot)
        {
            bool materialExists = slot switch
            {
                EquipmentSlot.EquipmentSlotIDs.Helmet => __instance.EquippedBodyVisuals.Renderer.sharedMaterials.FirstOrDefault(x=>x.name.Contains("_skin")) != null,
                EquipmentSlot.EquipmentSlotIDs.Chest => __instance.EquippedBodyVisuals.Renderer.sharedMaterials.FirstOrDefault(x=>x.name.Contains("_skin")) != null,
                EquipmentSlot.EquipmentSlotIDs.Foot => __instance.EquippedFootVisuals.Renderer.sharedMaterials.FirstOrDefault(x=>x.name.Contains("_skin")) != null,
                _ => false
            };
            return materialExists;
        }
        internal static Material MakeTransparent(Material matSource)
        {
            Material mat = new Material(matSource);
            mat.SetInt("_Mode", 3);
            mat.SetInt("_SrcBlend", 5);
            mat.SetInt("_DstBlend", 10);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 0;
            mat.color = new Color(0, 0, 0, 0);
            //I have no idea what these keywords actually do, google told me to add & remove them but removing opaque was the one that made everything work
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_OPAQUE");
            //mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            return mat;
        }
        public static void LoadCharacterCreationHair(this CharacterVisuals __instance, string raceId, int _gender, int _hairStyleIndex, int _hairColorIndex)
        {
            string previewId = $"previewVisualHair_{_gender}_{raceId}_{_hairStyleIndex}";
            RaceData race = ModdedCharacterVisualData.GetRace(raceId);
            if (race == null)
            {
                __instance.LoadCharacterCreationHair(_hairStyleIndex, _hairColorIndex);
                return;
            }
            if (__instance.m_armorVisualPreview.ContainsKey(previewId))
            {
                if (!__instance.m_armorVisualPreview[previewId].gameObject.activeSelf)
                    __instance.m_armorVisualPreview[previewId].gameObject.SetActive(true);
                if (_hairStyleIndex != 0)//not bald
                {
                    Material material = race.GetPart<Material>(RaceData.BodyPart.HairColor, _gender, _hairColorIndex);
                    __instance.m_armorVisualPreview[previewId].Renderer.material = material;
                }
                return;
            }
            else
            {
                __instance.UseDefaultVisuals = true;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Hair, _gender, _hairStyleIndex).transform, null);
                Material material = race.GetPart<Material>(RaceData.BodyPart.HairColor, _gender, _hairColorIndex);
                __instance.DefaultHairVisuals = transform.GetComponent<ArmorVisuals>();
                __instance.DefaultHairVisuals.ApplyToCharacterVisuals(__instance);
                if (!__instance.DefaultHairVisuals.gameObject.activeSelf)
                    __instance.DefaultHairVisuals.gameObject.SetActive(true);
                if (_hairStyleIndex != 0)//not bald
                    __instance.DefaultHairVisuals.Renderer.material = material;
                __instance.FinalizeSkinnedRenderer(__instance.DefaultHairVisuals.Renderer);
                __instance.m_armorVisualPreview.Add(previewId, __instance.DefaultHairVisuals);
            }
        }
        public static void LoadCharacterCreationHead(this CharacterVisuals __instance, string raceId, int _gender, int _headVariationIndex)
        {
            string previewId = $"previewVisualHead_{_gender}_{raceId}_{_headVariationIndex}";
            RaceData race = ModdedCharacterVisualData.GetRace(raceId);
            if (race == null)
            {
                __instance.LoadCharacterCreationHead(0,_gender, _headVariationIndex);
                return;
            }
            if (__instance.m_armorVisualPreview.ContainsKey(previewId))
            {
                if (!__instance.m_armorVisualPreview[previewId].gameObject.activeSelf)
                    __instance.m_armorVisualPreview[previewId].gameObject.SetActive(true);
            }
            else
            {
                __instance.UseDefaultVisuals = true;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Head, _gender, _headVariationIndex).transform, null);
                transform.SetParent(__instance.transform, false);
                transform.ResetLocal(true);
                __instance.DefaultHeadVisuals = transform.GetComponent<ArmorVisuals>();
                __instance.DefaultHeadVisuals.ApplyToCharacterVisuals(__instance);
                __instance.DefaultHeadVisuals.gameObject.SetActive(true);
                __instance.FinalizeSkinnedRenderer(__instance.DefaultHeadVisuals.Renderer);
                __instance.m_armorVisualPreview.Add(previewId, __instance.DefaultHeadVisuals);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterVisuals))]
    public static class CharacterVisualsPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("VisualPresets",MethodType.Getter)]
        public static bool GetVisualPresets(CharacterVisuals __instance, ref CharacterVisualsPresets __result)
        {
            if (__instance.m_visualsPresets == null && __instance.UseDefaultVisuals)
            {
                __instance.m_visualsPresets = CharacterManager.CharacterVisualsPresets;
                if (__instance.VisualData is ModdedCharacterVisualData mcvd)
                {
                    RaceData race = ModdedCharacterVisualData.GetRace(mcvd.RaceGUID);
                    if(race != null)
                        __instance.m_skinMat = race.GetRaceMaterial(mcvd.Gender);
                }                
                else
                    __instance.m_skinMat = ((__instance.VisualData.Gender == Character.Gender.Male) ?
                        __instance.m_visualsPresets.MSkins[__instance.VisualData.SkinIndex] :
                        __instance.m_visualsPresets.FSkins[__instance.VisualData.SkinIndex]);
            }
            __result = __instance.m_visualsPresets;
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch("LoadDefaultHead")]
        public static bool LoadDefaultHead(CharacterVisuals __instance)
        {
            if(__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                bool loadAsDefault = race == null;
                if (loadAsDefault || !__instance.ShouldLoadDefaultHead())
                    return loadAsDefault;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Head, moddedData.Gender, moddedData.HeadVariationIndex).transform);
                if (transform)
                {
                    transform.SetParent(__instance.transform, false);
                    transform.ResetLocal(true);
                    __instance.DefaultHeadVisuals = transform.GetComponent<ArmorVisuals>();
                    __instance.DefaultHeadVisuals.ApplyToCharacterVisuals(__instance);
                    __instance.DefaultHeadVisuals.gameObject.SetActive(true);
                    __instance.FinalizeSkinnedRenderer(__instance.DefaultHeadVisuals.Renderer);
                    if (__instance.Head)
                        __instance.Head.gameObject.SetActive(false);
                    __instance.Head = __instance.m_defaultHeadVisuals.transform;
                    return false;
                }
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("LoadDefaultBody")]
        public static bool LoadDefaultBody(CharacterVisuals __instance)
        {
            if(__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                bool loadAsDefault = race == null;
                if (loadAsDefault || !__instance.ShouldLoadDefaultArmor())
                    return loadAsDefault;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Body, moddedData.Gender).transform, null);
                if (transform)
                {
                    transform.SetParent(__instance.transform, false);
                    transform.ResetLocal(true);
                    //nvm I'll just use sideloader some day
                    __instance.DefaultBodyVisuals = transform.GetComponent<ArmorVisuals>();
                    __instance.DefaultBodyVisuals.ApplyToCharacterVisuals(__instance);
                    __instance.DefaultBodyVisuals.gameObject.SetActive(true);
                    if (race.UseRaceMaterial)
                        __instance.ApplySkinMaterial(__instance.DefaultBodyVisuals.Renderer);
                    __instance.FinalizeSkinnedRenderer(__instance.DefaultBodyVisuals.Renderer);
                    return false;
                }
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("LoadDefaultBoots")]
        public static bool LoadDefaultBoots(CharacterVisuals __instance)
        {
            if (__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                bool loadAsDefault = race == null;
                if (loadAsDefault || !__instance.ShouldLoadDefaultBoots())
                    return loadAsDefault;
                Transform transform = __instance.InstantiateVisuals(race.GetPart(RaceData.BodyPart.Feet, moddedData.Gender).transform, null);
                if (transform)
                {
                    transform.SetParent(__instance.transform, false);
                    transform.ResetLocal(true);
                    __instance.DefaultFootVisuals = transform.GetComponent<ArmorVisuals>();
                    __instance.DefaultFootVisuals.ApplyToCharacterVisuals(__instance);
                    __instance.DefaultFootVisuals.gameObject.SetActive(true);
                    if (race.UseRaceMaterial)
                        __instance.ApplySkinMaterial(__instance.DefaultFootVisuals.Renderer);
                    __instance.FinalizeSkinnedRenderer(__instance.DefaultFootVisuals.Renderer);
                    return false;
                }
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("ApplySkinMaterial")]
        public static bool ApplySkinMaterial(CharacterVisuals __instance, SkinnedMeshRenderer _renderer)
        {
            if (__instance.VisualData is ModdedCharacterVisualData moddedData)
            {
                RaceData race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
                if (race == null)
                    return true;
                Material[] mats = _renderer.sharedMaterials;
                Material raceSkin = race.GetRaceMaterial(moddedData.Gender);
                if (raceSkin != null)
                    raceSkin = new(raceSkin);
                for (int i = 0; i < mats.Length; i++)
                    if (mats[i].name.Contains("_skin"))
                    {
                        raceSkin.name = mats[i].name;
                        mats[i] = raceSkin;
                    }
                _renderer.sharedMaterials = mats;
                return false;
            }
            else
                return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("ActivateDefaultVisuals")]
        public static bool ActivateDefaultVisuals(CharacterVisuals __instance, EquipmentSlot.EquipmentSlotIDs _slotID, bool _activate)
        {
            ArmorVisuals armorVisuals = null;
            RaceData race=null;
            if (__instance.VisualData is ModdedCharacterVisualData moddedData)
                race = ModdedCharacterVisualData.GetRace(moddedData.RaceGUID);
            if (race == null)
                return true;
            switch (_slotID)
            {
                case EquipmentSlot.EquipmentSlotIDs.Helmet:
                    if (_activate)
                    {
                        if (__instance.DefaultHelmVisuals == null)
                        {
                            __instance.LoadDefaultHelm();
                            if (__instance.DefaultHelmVisuals)
                                __instance.m_visualsSkinnedMeshCenterSize[1] = __instance.DefaultHelmVisuals.GetComponent<SkinnedMeshRenderer>().localBounds;
                        }
                        else
                            __instance.DefaultHelmVisuals.gameObject.SetActive(true);
                    }
                    else if (!_activate && __instance.DefaultHelmVisuals != null)
                        __instance.DefaultHelmVisuals.gameObject.SetActive(race.ForceDefaultHead && __instance.HasSkinMaterial(_slotID));
                    break;
                case EquipmentSlot.EquipmentSlotIDs.Chest:
                    if (_activate)
                    {
                        if (__instance.DefaultBodyVisuals == null)
                        {
                            __instance.LoadDefaultBody();
                            __instance.m_visualsSkinnedMeshCenterSize[2] = __instance.DefaultBodyVisuals.GetComponent<SkinnedMeshRenderer>().localBounds;
                        }
                        else
                            __instance.DefaultBodyVisuals.gameObject.SetActive(true);
                    }
                    else if (!_activate && __instance.DefaultBodyVisuals != null)
                        __instance.DefaultBodyVisuals.gameObject.SetActive(race.ForceDefaultBody && __instance.HasSkinMaterial(_slotID));
                    break;
                case EquipmentSlot.EquipmentSlotIDs.Foot:
                    if (_activate)
                    {
                        if (__instance.DefaultFootVisuals == null)
                        {
                            __instance.LoadDefaultBoots();
                            __instance.m_visualsSkinnedMeshCenterSize[3] = __instance.DefaultFootVisuals.GetComponent<SkinnedMeshRenderer>().localBounds;
                        }
                        else
                            __instance.DefaultFootVisuals.gameObject.SetActive(true);
                    }
                    else if (!_activate && __instance.DefaultFootVisuals != null)
                        __instance.DefaultFootVisuals.gameObject.SetActive(race.ForceDefaultFeet && __instance.HasSkinMaterial(_slotID));
                    break;
            }
            __instance.SetSkinnedMeshRendersLarge(__instance.m_largeBounds);
            if (armorVisuals!=null && !_activate)
            {
                armorVisuals.ReclaimVisuals();
                CharacterManager.PutBackDefaultVisuals(armorVisuals);
            }
            return false;
        }
        [HarmonyPatch("InitDefaultVisuals")]
        [HarmonyPrefix]
        public static void InitDefaultVisuals(CharacterVisuals __instance)
        {
            if (__instance.UseDefaultVisuals)
                __instance.m_character.VisualData = new ModdedCharacterVisualData(__instance.m_character.VisualData);
            else if (__instance.m_character.VisualData.GetType() == typeof(ModdedCharacterVisualData))
                __instance.m_character.VisualData = new CharacterVisualData(__instance.m_character.VisualData);
        }
    }
}
