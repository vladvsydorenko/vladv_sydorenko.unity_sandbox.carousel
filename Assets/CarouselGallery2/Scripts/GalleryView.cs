using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.CarouselGallery2.Scripts
{
    public class GalleryView : UIBehaviour
    {
        public enum ScrollOrientation
        {
            Horizontal,
            Vertical
        }

        [Header("Settings")]
        public float SafeArreaOffset;
        public float RenderArreaOffset;
        public ScrollOrientation Orientation;
        public GalleryItemView ItemViewPrefab;
        public ScrollRect ScrollViewRef;
        public Image FullImageRef;

        [Header("Items")]
        public float ItemSize;
        public Sprite[] Items;

        private float _itemScale;
        private List<GalleryItemView> _views;

        [Header("Layout")]
        [SerializeField]
        private int _activeItems;
        private Vector2 _offset;
        private Rect _safeArea;
        private Rect _renderArea;
        [SerializeField]
        private Rect _contentArea;
        private Rect _viewportArea;
        private bool _isDirty;

        [Header("Debug")]
        [SerializeField]
        private RectTransform _safeAreaElement;
        [SerializeField]
        private RectTransform _renderAreaElement;
        [SerializeField]
        private RectTransform _contentAreaElement;
        [SerializeField]
        private RectTransform _viewportAreaElement;

        protected override void Start()
        {
            if (ScrollViewRef == null)
            {
                ScrollViewRef = GetComponent<ScrollRect>();
            }

            if (ScrollViewRef == null)
            {
                return;
            }

            ScrollViewRef.onValueChanged.AddListener(OnScrollViewChange);
            InitGallery();
        }

        private void OnScrollViewChange(Vector2 scroll)
        {
            UpdateGallery();
        }

        private void OnItemClick(GalleryItemView view)
        {
            if (FullImageRef == null)
            {
                return;
            }

            FullImageRef.sprite = view.GetSprite();
            FullImageRef.gameObject.SetActive(true);
        }

        private void InitGallery()
        {
            UpdateSrollView();
            UpdateLayoutData();

            if (Orientation == ScrollOrientation.Horizontal)
            {
                _itemScale = _renderArea.height / ItemSize;
                var size = ItemSize * _itemScale;

                var width = _renderArea.width;
                var itemsInViewport = (int)(width / size);
                var excesss = width % size;
                if (excesss > 0f)
                {
                    itemsInViewport += 1;
                }

                _contentArea.Set(_renderArea.x, _renderArea.y, itemsInViewport * size, _viewportArea.height);
                _activeItems = itemsInViewport;
            }
            else
            {
            }

            UpdateLayoutData();
        }

        private void UpdateGallery()
        {
            UpdateSrollView();
            UpdateLayoutData();
        }

        private void UpdateSrollView()
        {
            if (ScrollViewRef == null)
            {
                return;
            }

            ScrollViewRef.movementType = ScrollRect.MovementType.Unrestricted;
            ScrollViewRef.horizontal = Orientation == ScrollOrientation.Horizontal;
            ScrollViewRef.vertical = !ScrollViewRef.horizontal;
        }

        private void UpdateLayoutData()
        {
            if (ScrollViewRef == null || ScrollViewRef.viewport == null || ScrollViewRef.content == null)
            {
                return;
            }

            _viewportArea.position = new Vector2(-ScrollViewRef.content.anchoredPosition.x, -ScrollViewRef.content.anchoredPosition.y);
            _viewportArea.size = new Vector2(ScrollViewRef.viewport.rect.width, ScrollViewRef.viewport.rect.height);

            if (_viewportArea.x < _safeArea.x || _viewportArea.xMax > _safeArea.xMax)
            {
                _offset = _viewportArea.position;
                _isDirty = true;
            }

            if (_isDirty)
            {
                if (Orientation == ScrollOrientation.Horizontal)
                {
                    _safeArea.Set(_offset.x - SafeArreaOffset, _offset.y, _viewportArea.width + (SafeArreaOffset * 2f), _viewportArea.height);
                    _renderArea.Set(_offset.x - RenderArreaOffset, _offset.y, _viewportArea.width + (RenderArreaOffset * 2f), _viewportArea.height);
                }
                else
                {
                    _safeArea.Set(_offset.x, _offset.y - SafeArreaOffset, _viewportArea.width, _viewportArea.height + (SafeArreaOffset * 2f));
                    _renderArea.Set(_offset.x, _offset.y - RenderArreaOffset, _viewportArea.width, _viewportArea.height + (RenderArreaOffset * 2f));
                }

                RenderContent();
            }

            // Debug: area elements
            if (_safeAreaElement)
            {
                _safeAreaElement.sizeDelta = _safeArea.size;
                _safeAreaElement.anchoredPosition = _safeArea.position;
            }
            if (_renderAreaElement)
            {
                _renderAreaElement.sizeDelta = _renderArea.size;
                _renderAreaElement.anchoredPosition = _renderArea.position;
            }
            if (_contentAreaElement)
            {
                _contentAreaElement.sizeDelta = _contentArea.size;
                _contentAreaElement.anchoredPosition = _contentArea.position;
            }
            if (_viewportAreaElement)
            {
                _viewportAreaElement.sizeDelta = _viewportArea.size;
                _viewportAreaElement.anchoredPosition = _viewportArea.position;
            }
        }

        private void RenderContent()
        {
            if (Items == null || Items.Length < 1)
            {
                return;
            }

            int itemIndex = 0;

            var fullWidth = (ItemSize * _itemScale) * Items.Length;
            Vector2 itemPosition;

            if (Orientation == ScrollOrientation.Horizontal)
            {
                var contentX = _renderArea.x % fullWidth;
                if (contentX < 0f)
                {
                    contentX = fullWidth + contentX;
                }

                itemIndex = (int)(contentX / (ItemSize * _itemScale));
                itemPosition = _renderArea.position;
                float xExcess = _renderArea.x % (ItemSize * _itemScale);
                itemPosition.x -= xExcess;
            }
            else
            {
                itemPosition = _offset;
            }

            var itemLayout = Orientation == ScrollOrientation.Horizontal ? new Vector2(ItemSize * _itemScale, 0f) : new Vector2(0f, ItemSize * _itemScale);
            var itemSize = new Vector2(ItemSize * _itemScale, ItemSize * _itemScale);

            _activeItems = (int)(_renderArea.width / (ItemSize * _itemScale)) + 1;

            for (int viewIndex = 0; viewIndex < _activeItems; viewIndex++)
            {
                var view = GetOrCreateView(viewIndex);

                if (view == null)
                {
                    continue;
                }

                var item = Items[itemIndex % Items.Length];

                view.SetSprite(item);
                view.SetLayout(itemPosition, itemSize);
                view.gameObject.SetActive(true);

                itemPosition += itemLayout;
                itemIndex++;
            }

            if (_views == null)
            {
                return;
            }

            for (int i = _activeItems; i < _views.Count; i++)
            {
                var view = _views[i];
                if (view != null)
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private GalleryItemView GetOrCreateView(int viewIndex)
        {
            if (_views == null)
            {
                _views = new List<GalleryItemView>(_activeItems);
            }

            if (ItemViewPrefab == null)
            {
                return null;
            }

            if (_views.Count > viewIndex)
            {
                return _views[viewIndex];
            }
            var view = Instantiate(ItemViewPrefab, ScrollViewRef?.content);
            view.OnClick.AddListener(OnItemClick);
            _views.Add(view);
            return view;
        }

        protected override void OnDestroy()
        {
            if (ScrollViewRef == null)
            {
                return;
            }

            ScrollViewRef.onValueChanged.RemoveListener(OnScrollViewChange);
        }
    }
}