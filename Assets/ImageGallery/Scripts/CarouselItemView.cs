using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery.Scripts
{
    public class CarouselItemView : CarouselItemViewBase
    {
        public Image ImageRef;
        public RectTransform RectTransformRef;
        public GameObject OutlineElement;

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

            RectTransformRef.anchoredPosition = position;
            RectTransformRef.sizeDelta = size;
        }
        public override void Select()
        {
            if (OutlineElement == null)
            {
                return;
            }

            OutlineElement.SetActive(true);
        }
        public override void Deselect()
        {
            if (OutlineElement == null)
            {
                return;
            }

            OutlineElement.SetActive(false);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            OnClick.Invoke(Id);
        }
    }
}
