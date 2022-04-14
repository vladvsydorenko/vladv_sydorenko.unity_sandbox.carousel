using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.CarouselGallery.Scripts
{

    public class GalleryView5 : UIBehaviour
    {
        private enum RenderDirection
        {
            Forward,
            Backward
        }

        private struct Item
        {
            public float Size;
        }

        private Rect _renderArea;
        private Rect _contentArea;
        private List<Item> _items;

        private void RenderItems(int index, RenderDirection direction)
        {

        }

        private void RenderItem(int index, RenderDirection direction)
        {
            var item = _items[index];

            if (direction == RenderDirection.Backward)
            {
                _contentArea.x -= item.Size;
            }

            _contentArea.width += item.Size;
        }

        private int NormalizeItemIndex(int index)
        {
            return -1;
        }
    }

    public class GalleryView4 : UIBehaviour
    {
        private enum RenderDirection
        {
            Forward,
            Backward
        }

        private struct Item
        {
            public Vector2 Size;
        }

        private Rect _renderArea;
        private Rect _contentArea;

        private List<Item> _items;

        private int _firstItem;
        private int _lastItem;

        private void RenderItems(float end, RenderDirection direction)
        {
            if (direction == RenderDirection.Forward)
            {
                int index = NormalizeItemIndex(_lastItem + 1);

                while (_contentArea.xMax < _renderArea.xMax)
                {
                    RenderItem(index, direction);
                }
            }
            else
            {
                while (_contentArea.x > end)
                {

                }
            }
        }

        private void RenderItem(int index, RenderDirection direction)
        {
            if (index < 0 || index >= _items.Count)
            {
                return;
            }

            var item = _items[index];

        }

        private int NormalizeItemIndex(int index)
        {
            if (_items.Count < 1)
            {
                return -1;
            }

            var normalizedIndex = index % _items.Count;

            if (normalizedIndex < 0)
            {
                normalizedIndex = _items.Count - 1 - normalizedIndex;
            }

            return normalizedIndex;
        }
    }

    public class GalleryView3 : UIBehaviour, ILayoutGroup
    {
        public enum ScrollOrientation
        {
            Horizontal,
            Vertical
        }

        public struct Item
        {
            // width or height according to scroll orientation
            public float Size;
            public Sprite Sprite;
        }

        [Header("Settings")]
        public ScrollOrientation Orientation;
        public float SafeAreaOffset;
        public float RenderAreaOffset;
        public ScrollRect ScrollView;

        [Header("Layout")]
        [SerializeField]
        private Rect _safeArea;
        [SerializeField]
        private Rect _renderArea;
        [SerializeField]
        private Rect _viewportArea;

        [Header("Content")]
        private float _contentOffset;
        private Rect _contentArea;

        private void Update()
        {
            UpdateScrollView();
            UpdateLayout();
        }

        private void UpdateScrollView()
        {
            if (ScrollView == null)
            {
                return;
            }

            var isHorizontal = Orientation == ScrollOrientation.Horizontal;

            ScrollView.horizontal = isHorizontal;
            ScrollView.vertical = !isHorizontal;
        }

        private void UpdateLayout()
        {
            if (Orientation == ScrollOrientation.Horizontal)
            {
                UpdateLayoutHorizontal();
            }
            else
            {
                UpdateLayoutVertical();
            }
        }

        private void UpdateLayoutHorizontal()
        {
            if (ScrollView == null || ScrollView.viewport == null || ScrollView.content == null)
            {
                return;
            }

            var content = ScrollView.content;
            var viewport = ScrollView.viewport;

            _viewportArea.Set(-content.anchoredPosition.x, 0f, viewport.rect.width, viewport.rect.height);
            _renderArea.Set(_contentOffset - RenderAreaOffset, 0f, viewport.rect.width + (RenderAreaOffset * 2f), viewport.rect.height);
            _safeArea.Set(_contentOffset - SafeAreaOffset, 0f, viewport.rect.width + (SafeAreaOffset * 2f), viewport.rect.height);
        }

        private void UpdateLayoutVertical()
        {

        }

        public void SetLayoutHorizontal()
        {
            if (Orientation == ScrollOrientation.Horizontal)
            {
                UpdateLayout();
            }
        }

        public void SetLayoutVertical()
        {
            if (Orientation == ScrollOrientation.Vertical)
            {
                UpdateLayout();
            }
        }
    }

    public class GalleryView2 : UIBehaviour, ILayoutGroup
    {
        #region structs
        public enum ScrollOrientation
        {
            Horizontal,
            Vertical
        }

        [Serializable]
        private struct Item
        {
            public Sprite Sprite;
        }

        [Serializable]
        private struct RenderLayout
        {
            public int Version;
            public float Offset;
            public Rect SafeArea;
            public Rect RenderArea;
            public Rect ContentArea;
            public Rect ViewportArea;
        }
        #endregion

        [Header("Settings")]
        public float ExtraSafeSpace;
        public float ExtraRenderSpace;
        public float DefaultItemSpace;
        public ScrollOrientation Orientation;
        public GalleryItemView2 ItemViewPrefab;
        public ScrollRect ScrollView;

        [Header("State")]
        [SerializeField]
        private int _renderVersion;
        [SerializeField]
        private RenderLayout _layout;
        [SerializeField]
        private int _firstItemIndex;
        [SerializeField]
        private int _lastItemIndex;
        private int _lastViewIndex;
        [SerializeField]
        private List<Item> _items;
        [SerializeField]
        private List<GalleryItemView2> _views;
        private List<int> _freeViews;

        [Header("Debug")]
        [SerializeField]
        private RectTransform _SafeAreaDebug;
        [SerializeField]
        private RectTransform _RenderAreaDebug;

        protected override void Start()
        {
            var renderArea = _layout.RenderArea;
            _layout.ContentArea.Set(renderArea.x, renderArea.y, 0, renderArea.height);
            _freeViews = new List<int>();

        }

        private void Update()
        {
            RecalculateLayout();
            RenderViews();

            _renderVersion++;
        }

        #region Views
        private void RenderViews()
        {
            if (Orientation == ScrollOrientation.Horizontal)
            {
                RenderViewsHorizontal();
            }
        }
        private void RenderViewsHorizontal()
        {
            if (_renderVersion != _layout.Version)
            {
                return;
            }

            if (_items == null || _freeViews == null || ScrollView == null || ItemViewPrefab == null || _items.Count < 1 || DefaultItemSpace <= 0f)
            {
                return;
            }

            if (_views == null)
            {
                _views = new List<GalleryItemView2>();
            }

            var content = ScrollView.content;
            var renderArea = _layout.RenderArea;
            var contentArea = _layout.ContentArea;

            if (contentArea.xMax < renderArea.xMax)
            {
                int itemIndex = _lastItemIndex;
                float contentXMax = contentArea.xMax;

                while (contentXMax < renderArea.xMax)
                {
                    int viewIndex;
                    if (_freeViews.Count > 0)
                    {
                        viewIndex = _freeViews[_freeViews.Count - 1];
                        _freeViews.RemoveAt(_freeViews.Count - 1);
                    }
                    else
                    {
                        viewIndex = _views.Count;
                    }

                    if (viewIndex >= _views.Count)
                    {
                        _views.Add(Instantiate(ItemViewPrefab, content));
                    }

                    var item = _items[itemIndex];
                    var view = _views[viewIndex];

                    view.SetSprite(item.Sprite);
                    view.SetSize(new Vector2(DefaultItemSpace, renderArea.height));
                    view.SetPosition(new Vector2(contentXMax, 0f));
                    view.gameObject.SetActive(true);

                    contentXMax += DefaultItemSpace;
                    itemIndex = NormalizeItemIndex(itemIndex + 1);
                }

                contentArea.width += contentXMax - contentArea.xMax;

                float contentX = contentArea.xMax;
/*                for (int i = 0; i < _views.Count; i++)
                {
                    var view = _views[i];
                    var viewRect = view.ImageElement.rectTransform.rect;

                    if (viewRect.xMax < _layout.RenderArea.x || viewRect.x > _layout.RenderArea.xMax)
                    {
                        view.gameObject.SetActive(false);
                        _freeViews.Add(i);
                    }
                    else
                    {
                        if (viewRect.x < contentX)
                        {
                            contentX = viewRect.x;
                        }
                    }
                }
*/
                var xMax = contentArea.xMax;
                contentArea.x += contentX;
                contentArea.width -= xMax - contentX;

                _layout.ContentArea = contentArea;
            }
        }
        private int NormalizeItemIndex(int index)
        {
            if (_items.Count < 1)
            {
                return -1;
            }

            var normalizedIndex = index % _items.Count;

            if (normalizedIndex < 0)
            {
                normalizedIndex = _items.Count - 1 - normalizedIndex;
            }

            return normalizedIndex;
        }
        #endregion

        #region Layout
        public void SetLayoutHorizontal()
        {
            RecalculateLayout();
        }

        public void SetLayoutVertical() {}

        private void RecalculateLayout()
        {
            if (Orientation == ScrollOrientation.Horizontal)
            {
                RecalculateLayoutHorizontal();
            }
        }

        private void RecalculateLayoutHorizontal()
        {
            if (ScrollView == null || ScrollView.viewport == null || ScrollView.content == null)
            {
                return;
            }

            var content = ScrollView.content;
            var viewport = ScrollView.viewport;

            var offset = _layout.Offset;
            var safeArea = _layout.SafeArea;
            var renderArea = _layout.RenderArea;
            var viewportArea = _layout.ViewportArea;

            viewportArea.Set(-content.anchoredPosition.x, 0f, viewport.rect.width, viewport.rect.height);

            if (safeArea.x > viewportArea.x)
            {
                offset -= safeArea.x - viewportArea.x + ExtraSafeSpace;
                _layout.Version = _renderVersion;
            }
            else if (safeArea.xMax < viewportArea.xMax)
            {
                offset += viewportArea.xMax - safeArea.xMax + ExtraSafeSpace;
                _layout.Version = _renderVersion;
            }

            renderArea.Set(offset - ExtraRenderSpace, 0f, viewport.rect.width + (ExtraRenderSpace * 2f), viewport.rect.height);
            safeArea.Set(offset - ExtraSafeSpace, 0f, viewport.rect.width + (ExtraSafeSpace * 2f), viewport.rect.height);

            _layout.Offset = offset;
            _layout.SafeArea = safeArea;
            _layout.RenderArea = renderArea;
            _layout.ViewportArea = viewportArea;

            if (_SafeAreaDebug != null)
            {
                _SafeAreaDebug.anchoredPosition = safeArea.position;
                _SafeAreaDebug.sizeDelta = safeArea.size;
            }

            if (_RenderAreaDebug != null)
            {
                _RenderAreaDebug.anchoredPosition = renderArea.position;
                _RenderAreaDebug.sizeDelta = renderArea.size;
            }
        }
        #endregion
    }
}
