using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    RectTransform rectTransform;

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = null;

        if (eventData.pointerEnter != null)
        {
            eventData.pointerEnter.TryGetComponent(out dropSlot);
        }
        
        if (dropSlot == null) 
        {
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null) 
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            
            }
        
        }

        originalParent.TryGetComponent(out Slot originalSlot);

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                if (originalSlot != null)
                {
                    dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                    originalSlot.currentItem = dropSlot.currentItem;
                }

                if (dropSlot.currentItem.TryGetComponent(out RectTransform dropItemTransform))
                {
                    dropItemTransform.anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                if (originalSlot != null)
                {
                    originalSlot.currentItem = null;
                }

            }

            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;

        }
        else 
        {
            transform.SetParent(originalParent);
        }
        rectTransform.anchoredPosition = Vector2.zero;
    }

    private void Awake()
    {
        TryGetComponent(out canvasGroup);
        TryGetComponent(out rectTransform);
    }


}
