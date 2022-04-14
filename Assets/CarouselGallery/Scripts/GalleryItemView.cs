using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.CarouselGallery.Scripts
{
    public class GalleryItemView : UIBehaviour
    {
        public int Index;
        public Image ImageElement;
        public RectTransform ItemRectTransform;

        public void SetSprite(Sprite sprite)
        {
            if (ImageElement == null)
            {
                return;
            }

            ImageElement.sprite = sprite;
        }

        public void SetPosition(Vector2 position)
        {
            if (ImageElement == null)
            {
                return;
            }

            ImageElement.rectTransform.anchoredPosition = position;
        }

        public void SetSize(Vector2 size)
        {
            if (ImageElement == null)
            {
                return;
            }

            ImageElement.rectTransform.sizeDelta = size;
        }
    }
}
