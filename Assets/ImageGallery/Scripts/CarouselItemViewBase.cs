using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery.Scripts
{
    public abstract class CarouselItemViewBase : UIBehaviour, IPointerClickHandler
    {
        public int Id;
        public UnityEvent<int> OnClick;

        public abstract void Select();
        public abstract void Deselect();
        public abstract void SetImage(Sprite sprite);
        public abstract void SetLayout(Vector2 position, Vector2 size);
        public abstract void OnPointerClick(PointerEventData eventData);
    }
}
