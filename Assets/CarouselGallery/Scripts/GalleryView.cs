using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.CarouselGallery.Scripts
{

    public class GalleryView : UIBehaviour, ILayoutGroup
    {
        public enum LayoutOrientation
        {
            Horizontal,
            Vertical
        }

        [Serializable]
        private struct Item
        {
            public Sprite Sprite;
        }

        public LayoutOrientation Orientation;
        public float ExtraSpace;
        public float ReactionSpace;
        public float DefaultItemSize;
        public ScrollRect ScrollView;
        public GalleryItemView ItemViewPrefab;

        [Header("Render")]
        private int _renderVersion;
        private int _offsetVersion;

        [Header("Rects")]
        [SerializeField]
        private float _renderOffset;
        [SerializeField]
        private Rect _renderRect;
        [SerializeField]
        private Rect _reactionRect;
        [SerializeField]
        private Rect _viewportRect;

        [Header("Items")]
        [SerializeField]
        private List<GalleryItemView> _views;
        [SerializeField]
        private List<Item> _items;

        [Header("Debug")]
        [SerializeField]
        private bool _doDebug;
        [SerializeField]
        private float _debugContentSize;
        [SerializeField]
        private RectTransform _debugRenderElement;
        [SerializeField]
        private RectTransform _debugReactionElement;

        protected override void Awake()
        {
            if (ScrollView == null)
            {
                ScrollView = GetComponent<ScrollRect>();
            }

            _renderVersion = 0;
        }

        private void Update()
        {
            _renderVersion++;
            UpdateRects();
            UpdateItemLayouts();
        }

        private void UpdateLayout()
        {
            UpdateRects();
        }

        private void UpdateRects()
        {
            if (ScrollView == null)
            {
                return;
            }

            var viewport = ScrollView.viewport;
            var content = ScrollView.content;

            if (viewport == null || content == null)
            {
                return;
            }

            if (Orientation == LayoutOrientation.Horizontal)
            {
                if (_reactionRect.xMax < _viewportRect.xMax)
                {
                    _renderOffset += _viewportRect.xMax - _reactionRect.xMax + ReactionSpace;
                    _offsetVersion = _renderVersion;
                }
                else if (_reactionRect.x > _viewportRect.x)
                {
                    _renderOffset -= _reactionRect.x - _viewportRect.x + ReactionSpace;
                    _offsetVersion = _renderVersion;
                }

                _viewportRect.Set(-content.anchoredPosition.x, 0f, viewport.rect.width, viewport.rect.height);
                _renderRect.Set(_renderOffset - ExtraSpace, 0f, viewport.rect.width + (ExtraSpace * 2f), viewport.rect.height);
                _reactionRect.Set(_renderOffset - ReactionSpace, 0f, viewport.rect.width + (ReactionSpace * 2f), viewport.rect.height);

                if (_doDebug && _debugRenderElement != null && _debugReactionElement != null)
                {
                    // Debug.Log("Update debug");

                    //_debugRenderElement.anchoredPosition = new Vector2((-content.anchoredPosition.x) + _renderRect.x, 0f);
                    _debugRenderElement.anchoredPosition = new Vector2(_renderRect.x, 0f);
                    _debugRenderElement.sizeDelta = new Vector2(_renderRect.width, _renderRect.height);

                    //_debugReactionElement.anchoredPosition = new Vector2((-content.anchoredPosition.x) + _reactionRect.x, 0f);
                    _debugReactionElement.anchoredPosition = new Vector2(_reactionRect.x, 0f);
                    _debugReactionElement.sizeDelta = new Vector2(_reactionRect.width, _reactionRect.height);
                }
            }
            else
            {
                _renderRect.Set(0f, content.anchoredPosition.y - ExtraSpace, viewport.rect.width, viewport.rect.height + (ExtraSpace * 2f));
                _reactionRect.Set(0f, content.anchoredPosition.y - ReactionSpace, viewport.rect.width, viewport.rect.height + (ReactionSpace * 2f));
            }

        }

        private void UpdateItemLayouts()
        {
            if (_views.Count == 0)
            {
                InitViews();
            }
            
            if (_views.Count == 0)
            {
                return;
            }

            if (_offsetVersion == _renderVersion)
            {
                var firstView = _views[0];
                var firstX = firstView.ImageElement.rectTransform.rect.x;

                if (firstX > _renderRect.x)
                {
                    FillBackward(firstX - _reactionRect.x);
                }
            }
        }

        private void FillBackward(float space)
        {
            var firstView = _views[0];
            // var firstX = firstItem.ImageElement.rectTransform.rect.x;
            var firstItemIndex = firstView.Index;
            if (firstItemIndex < 0)
            {
                firstItemIndex = 0;
            }

            if (firstItemIndex >= _items.Count)
            {
                firstItemIndex = _items.Count - 1;
            }

            var firstItem = _items[firstItemIndex];

            var renderXMax = _renderRect.x;
            var renderHeight = _renderRect.height;
            var viewIndex = 0;
            var renderCount = 0;
            var itemIndex = 0;

            while (renderXMax > _renderRect.xMax)
            {
                if (viewIndex >= _views.Count)
                {
                    _views.Add(Instantiate(ItemViewPrefab, ScrollView.content));
                }

                var view = _views[viewIndex];
                var item = _items[itemIndex];

                var position = new Vector2(renderXMax, 0f);
                var size = new Vector2(DefaultItemSize, renderHeight);

                view.SetPosition(position);
                view.SetSize(size);
                view.SetSprite(item.Sprite);

                view.gameObject.SetActive(true);

                renderCount++;
                renderXMax += DefaultItemSize;
                viewIndex++;
                itemIndex = (itemIndex - 1) % _items.Count;
                itemIndex = itemIndex > 0 ? itemIndex : _items.Count - 1 - itemIndex;
            }
        }

        private void InitViews()
        {
            if (Orientation == LayoutOrientation.Horizontal)
            {
                if (ItemViewPrefab == null || ScrollView == null || ScrollView.content == null || DefaultItemSize <= 0f || _items == null || _items.Count < 1)
                {
                    return;
                }

                var renderX = _renderRect.x;
                var renderHeight = _renderRect.height;
                var viewIndex = 0;
                var renderCount = 0;
                var itemIndex = 0;

                while (renderX < _renderRect.xMax)
                {
                    if (viewIndex >= _views.Count)
                    {
                        _views.Add(Instantiate(ItemViewPrefab, ScrollView.content));
                    }

                    var view = _views[viewIndex];
                    var item = _items[itemIndex];

                    var position = new Vector2(renderX, 0f);
                    var size = new Vector2(DefaultItemSize, renderHeight);

                    view.SetPosition(position);
                    view.SetSize(size);
                    view.SetSprite(item.Sprite);

                    view.gameObject.SetActive(true);

                    renderCount++;
                    renderX += DefaultItemSize;
                    viewIndex++;
                    itemIndex = (itemIndex + 1) % _items.Count;
                }

                // deactivate extra views
                if (renderCount < _views.Count)
                {
                    for (int i = renderCount; i < _views.Count; i++)
                    {
                        var view = _views[i];
                        if (view == null)
                        {
                            continue;
                        }

                        view.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void SetLayoutHorizontal()
        {
            UpdateLayout();
        }

        public void SetLayoutVertical()
        {
            UpdateLayout();
        }
    }
}
