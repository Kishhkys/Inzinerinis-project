using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] public int ID;
    [SerializeField] public string Name;


    public virtual void UseItem()
    {
        Debug.Log("Using item" + name);
    }

    public virtual void PickUp()
    {
        Sprite itemIcon = GetComponent<Image>().sprite;

        if (ItemPickupUIController.Instance != null)
        {
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
        }
    }
}
