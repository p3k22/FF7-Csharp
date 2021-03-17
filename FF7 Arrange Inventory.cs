/**************************************
 *
 *      FF7 Item Menu Sorting Algorithm
 *
 **************************************/

// Created by P3k 2021

// Code has been extracted from FF7.exe, converted into C# and manually fine tuned.

// player.inventory.items = Your inventory list with at minimum the following fields:
// SortTag, Quantity, DatabaseID, Name

// StaticExtensions.ReturnAllItemsAndEquipmentNames should be an array of every item name  in order It/Wp/Ar/Ac

using System.Collections.Generic;
using System.Linq;

public class FF7Sorting
{
    public enum MethodID
    {
        Custom,

        Field,

        Battle,

        Throw,

        Type,

        Name,

        Most,

        Least
    };

    public enum SortingTag
    {
        None,

        Field,

        Battle,

        Throwable
    }

    private List<string> nameOrderList;

    private MethodID methodID;

    public void Sort(int maxSlotCount, MethodID methodID)
    {
        this.methodID = methodID;

        nameOrderList = StaticExtensions.ReturnAllItemsAndEquipmentNames.ToList();
        nameOrderList.Sort();

        var lastTopSlot = new int[64];
        var lastBottomSlot = new int[64];
        lastTopSlot[0] = 0;
        lastBottomSlot[0] = maxSlotCount - 1;

        var iterator = 1;

        // Loop 1
        while (1 != 0)
        {
            if (iterator-- <= 0)
            {
                break;
            }

            var topSlot = lastTopSlot[iterator];
            var topSlotPlusOne = topSlot + 1;
            var bottomSlot = lastBottomSlot[iterator];
            maxSlotCount = bottomSlot;

            // Loop 2
            while (1 != 0)
            {
                if (topSlotPlusOne >= bottomSlot)
                {
                    break;
                }

                // Loop 3
                while (topSlotPlusOne < bottomSlot)
                {
                    if (RequiresSorting(topSlot, topSlotPlusOne))
                    {
                        break;
                    }

                    topSlotPlusOne++;
                }

                // Loop 4
                while (1 != 0)
                {
                    if (topSlotPlusOne > bottomSlot)
                    {
                        break;
                    }

                    if (RequiresSorting(bottomSlot, topSlot))
                    {
                        break;
                    }

                    bottomSlot--;
                }

                if (topSlotPlusOne < bottomSlot)
                {
                    SwapSlots(topSlotPlusOne, bottomSlot);

                    topSlotPlusOne++;
                    bottomSlot--;
                }
            }

            // Restart Loop
            if (RequiresSorting(bottomSlot, topSlot))
            {
                SwapSlots(bottomSlot, topSlot);
            }

            if (bottomSlot > topSlot)
            {
                bottomSlot--;
            }

            if (bottomSlot > topSlot && maxSlotCount > topSlotPlusOne
                && bottomSlot - topSlot < maxSlotCount - topSlotPlusOne)
            {
                SwapIndex(ref bottomSlot, ref maxSlotCount);
                SwapIndex(ref topSlot, ref topSlotPlusOne);
            }

            if (bottomSlot > topSlot)
            {
                lastTopSlot[iterator] = topSlot;
                lastBottomSlot[iterator++] = bottomSlot;
            }

            if (maxSlotCount > topSlotPlusOne)
            {
                lastTopSlot[iterator] = topSlotPlusOne;
                lastBottomSlot[iterator++] = maxSlotCount;
            }

            if (iterator >= 64)
            {
                break;
            }
        }
    }

    private bool RequiresSorting(int slot1, int slot2)
    {
        switch (methodID)
        {
            case MethodID.Field:
            case MethodID.Battle:
            case MethodID.Throw:
                return SortByTag(slot1, slot2, (int)methodID) > 0;
            case MethodID.Type:
                return SortByType(slot1, slot2) > 0;
            case MethodID.Name:
                return SortByName(slot1, slot2) > 0;
            case MethodID.Most:
                return SortByMost(slot1, slot2) > 0;
            case MethodID.Least:
                return SortByLeast(slot1, slot2) > 0;
        }

        return false;
    }

    private int SortByTag(int slot1, int slot2, int tagID)
    {
        var tagToMatch = (SortingTag)tagID;

        var inventory = player.inventory.items;

        var s1Count = inventory[slot1].sortTag.Contains(tagToMatch) ? 2 - inventory[slot1].quantity : 2000;
        var s2Count = inventory[slot2].sortTag.Contains(tagToMatch) ? 2 - inventory[slot2].quantity : 2000;

        var sign = s2Count - s1Count;

        return sign < 0 ? -1 : sign > 0 ? 1 : 0;
    }

    private int SortByType(int slot1, int slot2)
    {
        var inventory = player.inventory.items;

        var s1Count = string.IsNullOrEmpty(inventory[slot1].name) ? 2000 : inventory[slot1].databaseID;

        var s2Count = string.IsNullOrEmpty(inventory[slot2].name) ? 2000 : inventory[slot2].databaseID;

        var sign = s2Count - s1Count;

        return sign < 0 ? -1 : sign > 0 ? 1 : 0;
    }

    private int SortByName(int slot1, int slot2)
    {
        var inventory = player.inventory.items;

        var s1Count = string.IsNullOrEmpty(inventory[slot1].name) ? 2000 : nameOrderList.IndexOf(inventory[slot1].name);

        var s2Count = string.IsNullOrEmpty(inventory[slot2].name) ? 2000 : nameOrderList.IndexOf(inventory[slot2].name);

        var sign = s2Count - s1Count;

        return sign < 0 ? -1 : sign > 0 ? 1 : 0;
    }

    private int SortByMost(int slot1, int slot2)
    {
        var inventory = player.inventory.items;

        var s1Count = string.IsNullOrEmpty(inventory[slot1].name) ? 0 : inventory[slot1].quantity;

        var s2Count = string.IsNullOrEmpty(inventory[slot2].name) ? 0 : inventory[slot2].quantity;

        var sign = s1Count - s2Count;

        return sign < 0 ? -1 : sign > 0 ? 1 : 0;
    }

    private int SortByLeast(int slot1, int slot2)
    {
        var inventory = player.inventory.items;

        var s1Count = string.IsNullOrEmpty(inventory[slot1].name) ? 2000 : inventory[slot1].quantity;

        var s2Count = string.IsNullOrEmpty(inventory[slot2].name) ? 2000 : inventory[slot2].quantity;

        var sign = s2Count - s1Count;

        return sign < 0 ? -1 : sign > 0 ? 1 : 0;
    }

    private void SwapSlots(int slot1, int slot2)
    {
        var item1 = player.inventory.items[slot1];
        var item2 = player.inventory.items[slot2];

        player.inventory.items[slot2].SetAsCopy(item1);
        player.inventory.items[slot1].SetAsCopy(item2);
    }

    private void SwapIndex(ref int slot1Index, ref int slot2Index)
    {
        var t = slot1Index;
        slot1Index = slot2Index;
        slot2Index = t;
    }
}
