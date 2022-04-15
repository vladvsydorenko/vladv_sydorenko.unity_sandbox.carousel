using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery2.Scripts
{
    [ExecuteInEditMode]
    public class CarouselView : UIBehaviour, ILayoutGroup
    {
        #region Definitions
        public enum ScrollOrientation
        {
            Horizontal,
            Vertical
        }

        [Serializable]
        private struct StateData
        {
            public ScrollRect ScrollView;
            public RectTransform ContentElement;
            public RectTransform ViewportElement;

            public Vector2 ItemCursor;
            public Vector2 ItemScaledSize;
            public Vector2 ItemScaledPadding;

            public int FirstItem;
            public Vector2 FirstItemPosition;

            public int ActiveItems;
            public List<CarouselItemViewBase> ItemViews;
        }
        #endregion

        #region Settings
        [Header("Settings")]
        public bool UpdateInEditor;
        public ScrollOrientation Orientation;
        public CarouselItemViewBase ItemViewPrefab;
        [Header("Settings: Items")]
        public Vector2 ItemSize;
        public Vector2 ItemPadding;
        public bool ScalePaddings;
        #endregion

        #region Data
        [Header("Data")]
        public GalleryItemData[] Items;
        #endregion

        #region State
        [Header("State")]
        // number of item being redering
        [SerializeField]
        private StateData _state;
        #endregion

        #region Lifecycle
        private void Update()
        {
            if (Application.isEditor && !UpdateInEditor)
            {
                return;
            }

            UpdateScrollView();

            PrepareViews();
            ActivateViews();
            DeactivateUnusedViews();
            UpdateItemState();
            UpdateViews();
        }
        public void SetLayoutHorizontal()
        {
            if (Application.isEditor && !UpdateInEditor)
            {
                return;
            }
        }

        public void SetLayoutVertical()
        {
        }

        #endregion

        #region ScrollView
        private void UpdateScrollView()
        {
            if (_state.ScrollView == null)
            {
                _state.ScrollView = GetComponent<ScrollRect>();
                if (_state.ScrollView == null)
                {
                    _state.ScrollView = GetComponentInChildren<ScrollRect>();
                }
            }

            if (_state.ScrollView == null)
            {
                return;
            }

            _state.ScrollView.movementType = ScrollRect.MovementType.Unrestricted;
            _state.ScrollView.horizontal = Orientation == ScrollOrientation.Horizontal;
            _state.ScrollView.vertical = !_state.ScrollView.horizontal;
        }
        #endregion

        #region Items
        private void UpdateItemState()
        {
            if (_state.ViewportElement == null || _state.ContentElement == null)
            {
                return;
            }

            float scale;

            if (Orientation == ScrollOrientation.Horizontal)
            {
                scale = ItemSize.y > 0f ? _state.ViewportElement.rect.height / ItemSize.y : 0f;
            }
            else
            {
                scale = ItemSize.x > 0f ? _state.ViewportElement.rect.width / ItemSize.x : 0f;
            }

            _state.ItemScaledSize = ItemSize * scale;
            _state.ItemScaledPadding = ItemPadding * scale;
            _state.ItemCursor = _state.ItemScaledSize;

            if (Orientation == ScrollOrientation.Horizontal)
            {
                _state.ItemCursor.y = 0f;
                _state.ActiveItems = (int)(_state.ViewportElement.rect.width / _state.ItemScaledSize.x) + 2;

                var offsetX = -_state.ContentElement.anchoredPosition.x;

                var index = (int)(offsetX / _state.ItemScaledSize.x);
                index %= Items.Length;

                float localOffsetX = -1f * (offsetX % _state.ItemScaledSize.x);

                if (offsetX < 0)
                {
                    index = Items.Length - 1 - Mathf.Abs(index);
                    localOffsetX = -1f * (_state.ItemScaledSize.x - localOffsetX);
                }

                _state.FirstItem = index;
                _state.FirstItemPosition.Set(offsetX + localOffsetX, _state.ContentElement.anchoredPosition.y);
            }
            else
            {
                _state.ItemCursor.x = 0f;
                _state.ActiveItems = (int)(_state.ViewportElement.rect.height / _state.ItemScaledSize.y) + 2;

                var offsetY = -_state.ContentElement.anchoredPosition.y;

                var index = (int)(offsetY / _state.ItemScaledSize.y);
                index %= Items.Length;
                index = Mathf.Abs(index);

                float localOffsetY = -(offsetY % _state.ItemScaledSize.y);

                if (offsetY > 0f)
                {
                    index = Items.Length - 1 - index;
                    localOffsetY = (_state.ItemScaledSize.y + localOffsetY);
                }

                _state.FirstItem = index;
                _state.FirstItemPosition.Set(_state.ContentElement.anchoredPosition.x, offsetY + localOffsetY);
            }
        }
        #endregion

        #region Views
        // Ensures there is enough views for each active item
        private void PrepareViews()
        {
            if (_state.ItemViews == null || Items == null)
            {
                return;
            }

            // there are enough views for each active item
            if (_state.ItemViews.Count < _state.ActiveItems)
            {
                CreateViews(_state.ActiveItems - _state.ItemViews.Count);
            }
        }
        private void CreateViews(int count)
        {
            if (ItemViewPrefab == null || _state.ItemViews == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                _state.ItemViews.Add(Instantiate(ItemViewPrefab, _state.ContentElement));
            }
        }

        private void ActivateViews()
        {
            if (_state.ItemViews == null)
            {
                return;
            }

            // activate active items
            for (int i = 0; i < _state.ActiveItems; i++)
            {
                var view = _state.ItemViews[i];

                if (view == null)
                {
                    continue;
                }

                if (!view.gameObject.activeSelf)
                {
                    view.gameObject.SetActive(true);
                }
            }
        }
        private void DeactivateUnusedViews()
        {
            if (_state.ItemViews == null)
            {
                return;
            }

            if (_state.ItemViews.Count <= _state.ActiveItems)
            {
                return;
            }

            var count = _state.ItemViews.Count;
            for (int i = _state.ActiveItems; i < count; i++)
            {
                var view = _state.ItemViews[i];

                if (view == null)
                {
                    continue;
                }

                if (view.gameObject.activeSelf)
                {
                    view.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateViews()
        {
            if (Items == null || _state.ItemViews == null)
            {
                return;
            }

            var views = _state.ItemViews;

            var size = _state.ItemScaledSize;
            var padding = ScalePaddings ? _state.ItemScaledPadding : ItemPadding;
            var position = _state.FirstItemPosition;

            var cursor = _state.ItemCursor;
            cursor.y *= -1f;

            var paddingCursor = padding;
            paddingCursor.y *= -1f;

            var itemIndex = _state.FirstItem;

            var count = Mathf.Min(_state.ItemViews.Count, _state.ActiveItems);
            for (int i = 0; i < count; i++)
            {
                var view = views[i];
                var item = Items[itemIndex];
                if (view == null)
                {
                    continue;
                }

                view.SetImage(item.Image);
                view.SetLayout(position + paddingCursor, size - (padding * 2f));

                position += cursor;
                // next index, looping
                itemIndex = (itemIndex + 1) % Items.Length;
            }
        }

        [ContextMenu(nameof(DestroyViews))]
        private void DestroyViews()
        {
            if (_state.ItemViews == null)
            {
                return;
            }

            foreach (var view in _state.ItemViews)
            {
                if (view == null)
                {
                    continue;
                }

                if (Application.isEditor)
                {
                    DestroyImmediate(view.gameObject);
                }
                else
                {
                    Destroy(view.gameObject);
                }
            }

            _state.ItemViews.Clear();
        }
        #endregion
    }
}
