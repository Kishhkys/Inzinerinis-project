using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;


    public virtual void UseItem()
    {
    }

    public virtual void PickUp(string group = "Item", string sound = "Item_pick_up")
    {

        TryGetComponent(out Image image);
        Sprite itemIcon = image != null ? image.sprite : null;
        if (ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }

        if (!string.IsNullOrEmpty(sound) && !string.IsNullOrEmpty(group))
        {
            SoundEffectManager.PlayClip(group, sound, 1f);
        }
    }

}
