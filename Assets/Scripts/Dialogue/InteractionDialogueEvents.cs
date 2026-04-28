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
            InteractionDialogueManager.Show(RandomLine(KeyPickups));
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
        ShowOnce("first_zombie_seen", RandomLine(FirstZombieSight));
    }

    public static void LockedDoorChecked(bool hasMatchingKey)
    {
        InteractionDialogueManager.Show(hasMatchingKey ? "This key should fit here." : "It's locked. I need the right key.");
    }

    public static void LockedContainerChecked(bool hasMatchingKey)
    {
        InteractionDialogueManager.Show(hasMatchingKey ? "This key should open it." : "Locked. There has to be a key nearby.");
    }

    public static void DrainChecked(bool hasRequiredItem)
    {
        InteractionDialogueManager.Show(hasRequiredItem ? "I can pull it out with this." : "I need something to pull that out.");
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
}
