using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery2.Scripts
{
    public class BigImageView : UIBehaviour, IPointerClickHandler
    {
        public int Id;
        public UnityEvent<int> OnClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(Id);
        }

    }
}
