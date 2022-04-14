using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.CarouselGallery2.Scripts
{
    public class GalleryItemView : UIBehaviour, IPointerClickHandler
    {
        public Image ImageRef;
        public UnityEvent<GalleryItemView> OnClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Invoke(this);
        }

        public void SetLayout(Vector2 position, Vector2 size)
        {
            if (ImageRef == null)
            {
                return;
            }

            ImageRef.rectTransform.sizeDelta = size;
            ImageRef.rectTransform.anchoredPosition = position;
        }

        public void SetSprite(Sprite sprite)
        {
            if (ImageRef == null)
            {
                return;
            }

            ImageRef.sprite = sprite;
        }

        public Sprite GetSprite()
        {
            if (ImageRef == null)
            {
                return null;
            }

            return ImageRef.sprite;
        }
    }
}