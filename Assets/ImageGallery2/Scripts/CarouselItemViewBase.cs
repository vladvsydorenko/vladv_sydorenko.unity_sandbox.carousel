using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery2.Scripts
{
    public abstract class CarouselItemViewBase : UIBehaviour
    {
        public Image ImageRef;

        public abstract void SetImage(Sprite sprite);
        public abstract void SetLayout(Vector2 position, Vector2 size);
    }
}
