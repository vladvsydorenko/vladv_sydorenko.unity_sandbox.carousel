using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery.Scripts
{
    public abstract class ImageCarouselItemViewBase : UIBehaviour
    {
        public Image ImageRef;

        public abstract void SetImage(Sprite sprite);
        public abstract void SetLayout(Vector2 position, Vector2 size);
    }
}
