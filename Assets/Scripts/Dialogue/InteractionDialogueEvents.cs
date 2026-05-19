using System.Collections.Generic;
using UnityEngine;

public static class InteractionDialogueEvents
{
    private static readonly string[] HealthPotionPickups =
    {
        "This will be useful later.",
        "I should keep this close.",
        "Good. I might need this soon.",
        "This could save me later."
    };

    private static readonly string[] KeyPickups =
    {
        "Where can I use this?",
        "This has to open something.",
        "A key. Now I need the lock.",
        "I wonder what this belongs to."
    };

    private static readonly string[] LockpickPickups =
    {
        "A lockpick. This could get me through a locked door.",
        "This should help with a stubborn lock.",
        "A lockpick. Better keep this close.",
        "Good. Now I just need the right lock."
    };

    private static readonly string[] ToolPickups =
    {
        "This could come in handy.",
        "I can probably use this somewhere.",
        "Better take this with me.",
        "This might solve a problem later."
    };

    private static readonly string[] TapePickups =
    {
        "Tape. That might be useful.",
        "I should keep this.",
        "This could patch something up.",
        "Not sure yet, but I'll take it."
    };

    private static readonly string[] PotionDrinks =
    {
        "This feels refreshing.",
        "That helps.",
        "I needed that.",
        "I feel a little better."
    };

    private static readonly string[] RatDamage =
    {
        "Those pesky rats.",
        "Get off me!",
        "Nasty little thing.",
        "Not again."
    };

    private static readonly string[] ZombieDamage =
    {
        "Aah!",
        "Ahhhhh!",
        "Get away from me!",
        "No, no, no!"
    };

    private static readonly string[] FirstZombieSight =
    {
        "What the hell is that?",
        "Ahhhhh!",
        "What is that thing?",
        "I need to stay away from that."
    };

    private static readonly string[] InventoryFull =
    {
        "I can't carry anything else.",
        "My bag is full.",
        "I need to make room first."
    };

    private static readonly HashSet<string> triggeredOnce = new();

    public static void ItemPickedUp(Item item)
    {
        if (item == null)
        {
            return;
        }

        if (item is Potion)
        {
            InteractionDialogueManager.Show(RandomLine(HealthPotionPickups));
            return;
        }

        if (item is KeyItem)
        {
            InteractionDialogueManager.Show(IsLockpick(item) ? RandomLine(LockpickPickups) : RandomLine(KeyPickups));
            return;
        }

        if (item is PliersItem)
        {
            InteractionDialogueManager.Show(RandomLine(ToolPickups));
            return;
        }

        if (item is TapeItem)
        {
            InteractionDialogueManager.Show(RandomLine(TapePickups));
        }
    }

    public static void PotionDrunk()
    {
        InteractionDialogueManager.Show(RandomLine(PotionDrinks));
    }

    public static void PlayerDamagedBy(GameObject damageSource)
    {
        if (damageSource == null)
        {
            return;
        }

        if (damageSource.GetComponentInParent<MouseAI>() != null)
        {
            InteractionDialogueManager.Show(RandomLine(RatDamage));
            return;
        }

        if (damageSource.GetComponentInParent<EnemyAI>() != null)
        {
            InteractionDialogueManager.Show(RandomLine(ZombieDamage));
        }
    }

    public static void ZombieSeen(GameObject zombie)
    {
        if (zombie == null)
        {
            ShowOnce("first_zombie_seen", RandomLine(FirstZombieSight));
            return;
        }

        string key = $"first_zombie_seen_{zombie.GetInstanceID()}";
        ShowOnce(key, RandomLine(FirstZombieSight));
    }

    public static void LockedDoorChecked(bool hasMatchingKey)
    {
        InteractionDialogueManager.Show(hasMatchingKey ? "I can pick this lock." : "It's locked. I need something to pick it.");
    }

    public static void LockedContainerChecked(bool hasMatchingKey)
    {
        InteractionDialogueManager.Show(hasMatchingKey ? "This key should open it." : "Locked. There has to be a key nearby.");
    }

    public static void DrainChecked(bool hasRequiredItem)
    {
        InteractionDialogueManager.Show(hasRequiredItem ? "I can pull it out with this." : "I need something to pull that out.");
    }

    public static void InventoryFullChecked()
    {
        InteractionDialogueManager.Show(RandomLine(InventoryFull));
    }

    public static void ElevatorChecked(bool isRepaired, bool hasKeycardAccepted, bool isOpened)
    {
        if (!isRepaired)
        {
            InteractionDialogueManager.Show("The elevator has no power. I need to fix the panel first.");
            return;
        }

        if (!hasKeycardAccepted)
        {
            InteractionDialogueManager.Show("The doors won't respond. I should use a keycard on the reader.");
            return;
        }

        if (isOpened)
        {
            InteractionDialogueManager.Show("This is my way out.");
        }
    }

    public static void ElevatorOpened()
    {
        InteractionDialogueManager.Show("The elevator is open. Time to leave.");
    }

    public static void ElevatorPanelChecked(bool hasSelectedItem, bool hasCorrectItem, bool willCompleteRepair)
    {
        if (!hasSelectedItem)
        {
            InteractionDialogueManager.Show("The panel is damaged. I need something useful from my inventory.");
            return;
        }

        if (!hasCorrectItem)
        {
            InteractionDialogueManager.Show("That won't help here. I need the right repair item.");
            return;
        }

        InteractionDialogueManager.Show(willCompleteRepair ? "That should bring the elevator back to life." : "Good. One more part needs attention.");
    }

    public static void CardReaderChecked(bool elevatorRepaired, bool hasCorrectKeycard)
    {
        if (!elevatorRepaired)
        {
            InteractionDialogueManager.Show("The reader is dark. I need to restore power first.");
            return;
        }

        InteractionDialogueManager.Show(hasCorrectKeycard ? "Access granted." : "This needs the right keycard.");
    }

    private static void ShowOnce(string key, string message)
    {
        if (triggeredOnce.Contains(key))
        {
            return;
        }

        triggeredOnce.Add(key);
        InteractionDialogueManager.Show(message);
    }

    private static string RandomLine(IReadOnlyList<string> lines)
    {
        return lines[Random.Range(0, lines.Count)];
    }

    private static bool IsLockpick(Item item)
    {
        return item != null && !string.IsNullOrWhiteSpace(item.Name) && item.Name.ToLowerInvariant().Contains("lockpick");
    }
}
