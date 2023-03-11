using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using ValheimPlus.Configurations;

namespace ValheimPlus.GameClasses
{
    /// <summary>
    /// Alters teleportation prevention
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "IsTeleportable")]
    public static class noItemTeleportPrevention
    {
        public static bool Prefix(ref Inventory __instance, ref bool __result)
        {
            if (!Configuration.Current.Items.IsEnabled) return true;

            if (!Configuration.Current.Items.noTeleportPrevention) return true;

            if (!Configuration.Current.Items.noTeleportPreventionOnlyAllowsIngots)
            {
                __result = true;
                return false;
            }

            foreach(var item in __instance.m_inventory)
            {
                if (TeleportableItemDefinitions.IngotNames.Contains(item.m_shared.m_name))
                {
                    continue;
                }
                if (!item.m_shared.m_teleportable)
                {
                    __result = false;
                    return false;
                }
            }
            
            __result = true;
            return false;
        }
    }

    /// <summary>
    /// Makes all items fill inventories top to bottom instead of just tools and weapons
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "TopFirst")]
    public static class Inventory_TopFirst_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            if (Configurations.Configuration.Current.Inventory.IsEnabled &&
                Configurations.Configuration.Current.Inventory.inventoryFillTopToBottom)
            {
                __result = true;
                return false;
            }
            else return true;
        }
    }

    /// <summary>
    /// Configure player inventory size
    /// </summary>
    [HarmonyPatch(typeof(Inventory), MethodType.Constructor, new Type[] { typeof(string), typeof(Sprite), typeof(int), typeof(int) })]
    public static class Inventory_Constructor_Patch
    {
        private const int playerInventoryMaxRows = 20;
        private const int playerInventoryMinRows = 4;

        public static void Prefix(string name, ref int w, ref int h)
        {
            if (Configuration.Current.Inventory.IsEnabled)
            {
                // Player inventory
                if (name == "Grave" || name == "Inventory")
                {
                    h = Helper.Clamp(Configuration.Current.Inventory.playerInventoryRows, playerInventoryMinRows, playerInventoryMaxRows);
                }
            }
        }
    }


    public static class Inventory_NearbyChests_Cache
    {
        public static List<Container> chests = new List<Container>();
        public static readonly Stopwatch delta = new Stopwatch();
    }

    /// <summary>
    /// When merging another inventory, try to merge items with existing stacks.
    /// </summary>
    [HarmonyPatch(typeof(Inventory), "MoveAll")]
    public static class Inventory_MoveAll_Patch
    {
        private static void Prefix(ref Inventory __instance, ref Inventory fromInventory)
        {
            if (Configuration.Current.Inventory.IsEnabled && Configuration.Current.Inventory.mergeWithExistingStacks)
            {
                List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
                foreach (ItemDrop.ItemData otherItem in list)
                {
                    if (otherItem.m_shared.m_maxStackSize > 1)
                    {
                        foreach (ItemDrop.ItemData myItem in __instance.m_inventory)
                        {
                            if (myItem.m_shared.m_name == otherItem.m_shared.m_name && myItem.m_quality == otherItem.m_quality)
                            {
                                int itemsToMove = Math.Min(myItem.m_shared.m_maxStackSize - myItem.m_stack, otherItem.m_stack);
                                myItem.m_stack += itemsToMove;
                                if (otherItem.m_stack == itemsToMove)
                                {
                                    fromInventory.RemoveItem(otherItem);
                                    break;
                                }
                                else
                                {
                                    otherItem.m_stack -= itemsToMove;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static class TeleportableItemDefinitions {
        public static readonly List<string> IngotNames = new List<string>
        {
            "$item_tin",
            "$item_copper",
            "$item_bronze",
            "$item_iron",
            "$item_silver",
            "$item_blackmetal",
            "$item_flametal"
        };
    }

}
