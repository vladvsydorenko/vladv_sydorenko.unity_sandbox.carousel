using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Carousel
{
    public class CarouselItemView : UIBehaviour
    {
        public Image ImageElement;

        public void SetSprite(Sprite sprite)
        {
            ImageElement.sprite = sprite;
        }

        public void SetSize(Vector2 size)
        {
            ImageElement.rectTransform.sizeDelta = size;
        }

        public void SetPosition(Vector2 position)
        {
            ImageElement.rectTransform.anchoredPosition = position;
        }
    }
}
