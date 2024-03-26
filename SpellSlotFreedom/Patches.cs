using HarmonyLib;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Spellbook.KnownSpells;
using Kingmaker.UI.MVVM._PCView.ServiceWindows.Spellbook.Switchers;
using Kingmaker.UnitLogic.Abilities;
using UnityEngine.EventSystems;

namespace SpellSlotFreedom;

/// <summary>
/// Extends spellbook drag and drop functionality
/// </summary>
[HarmonyPatch(typeof(SpellbookKnownSpellPCView), nameof(SpellbookKnownSpellPCView.EndDrag))]
internal static class SpellbookKnownSpellPCView_EndDrag_Patch
{
    [HarmonyPostfix]
    static void Postfix(SpellbookKnownSpellPCView __instance, PointerEventData eventData)
    {
        if (DragNDropManager.DropTarget != null)
        {
            return;
        }
        var levelLabel = eventData.pointerEnter;
        var component = levelLabel.GetComponentInParent<SpellbookLevelSwitcherEntityPCView>();
        if (component != null)
        {
            var spellbook = levelLabel.GetComponentInParent<SpellbookLevelSwitcherPCView>().ViewModel.CurrentSpellbook.Value;
            var spellData = __instance.ViewModel.SpellData;
            var lvl = component.ViewModel.SpellbookLevel.Level;
            var shift = spellData.SpellLevel - lvl;

            if (spellData.MetamagicData != null)
            {
                return;
            }

            var spellKey = $"{spellbook.Owner.Unit.UniqueId}:{spellbook.Blueprint.AssetGuid.m_Guid}:{spellData.Blueprint.AssetGuid.m_Guid}";
            if (shift > 0)
            {
                if (spellData.IsTemporary)
                {
                    spellbook.RemoveTemporarySpell(spellData);
                    Storage.PerSave.SpellsOnHigherLevels.Remove(spellKey);
                    Storage.SavePerSaveSettings();
                    EventBus.RaiseEvent(delegate (ISlotWasAddedHandler h)
                    {
                        h.SlotWasAdded(spellbook.Owner.Unit);
                    });
                }
            }
            else if (shift < 0)
            {
                spellbook.AddKnownTemporary(lvl, spellData.Blueprint);
                Storage.PerSave.SpellsOnHigherLevels[spellKey] = shift;
                Storage.SavePerSaveSettings();
                EventBus.RaiseEvent(delegate (ISlotWasAddedHandler h)
                {
                    h.SlotWasAdded(spellbook.Owner.Unit);
                });
                Main.LogTrace($"Saved spell with key {spellKey} at shift {shift}");
            }
        }
    }
}

/// <summary>
/// Adds spell level penalty to spells that were put to higher levels
/// </summary>
[HarmonyPatch(typeof(RuleCalculateAbilityParams))]
[HarmonyPatch(MethodType.Constructor, [typeof(UnitEntityData), typeof(AbilityData)])]
public static class RuleCalculateAbilityParams_Constructor_Patch
{
    [HarmonyPostfix]
    public static void Post(RuleCalculateAbilityParams __instance, UnitEntityData initiator, AbilityData spell)
    {
        if (!spell.IsTemporary)
        {
            return;
        }
        var str = $"{initiator.UniqueId}:{spell.Spellbook.Blueprint.AssetGuid.m_Guid}:{spell.Blueprint.AssetGuid.m_Guid}";
        if (Storage.PerSave.SpellsOnHigherLevels.TryGetValue(str, out var shift))
        {
            __instance.AddBonusSpellLevel(shift);
        }
    }
}
