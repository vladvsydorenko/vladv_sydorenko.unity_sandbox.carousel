using UnityEngine;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery2.Scripts
{
    public class CarouselItemView : CarouselItemViewBase
    {
        public override void SetImage(Sprite sprite)
        {
            if (ImageRef == null)
            {
                return;
            }

            ImageRef.sprite = sprite;
        }

        public override void SetLayout(Vector2 position, Vector2 size)
        {
            if (ImageRef == null)
            {
                return;
            }

            ImageRef.rectTransform.anchoredPosition = position;
            ImageRef.rectTransform.sizeDelta = size;
        }
    }
}
