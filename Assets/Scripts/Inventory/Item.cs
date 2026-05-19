using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] public string Name;


    public virtual void UseItem()
    {
    }

    //public virtual void PickUp()
    //{
    //    Sprite itemIcon = GetComponent<Image>().sprite;

    //    if (ItemPickupUIController.Instance != null)
    //    {
    //        ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
    //        SoundEffectManager.Play("Item", 1f);
    //    }
    //}

    public virtual void PickUp(string group = "Item", string sound = "Item_pick_up")
    {

        Sprite itemIcon = GetComponent<Image>().sprite;
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
