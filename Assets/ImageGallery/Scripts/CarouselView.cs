using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery.Scripts
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
            public bool IsDirty;

            public ScrollRect ScrollView;
            public RectTransform ContentElement;
            public RectTransform ViewportElement;

            public Vector2 ItemCursor;
            public Vector2 ItemScaledSize;
            public Vector2 ItemScaledPadding;

            public int SelectedItem;
            public int FirstItem;
            public Vector2 FirstItemPosition;

            public int ActiveItems;
            public List<CarouselItemViewBase> ItemViews;
        }
        #endregion

        #region Settings
        [Header("Settings")]
        public bool UpdateInEditor;
        public bool Enabled;
        public ScrollOrientation Orientation;
        public CarouselItemViewBase ItemViewPrefab;
        public RectTransform DisabledOverlay;
        [Header("Settings: Items")]
        public Vector2 ItemSize;
        public Vector2 ItemPadding;
        public bool ScalePaddings;
        #endregion

        #region Data
        [Header("Data")]
        public GalleryItemData[] Items;
        #endregion

        #region Events
        [Header("Events")]
        public UnityEvent<int> OnItemSelect;
        #endregion

        #region State
        [Header("State")]
        // number of item being redering
        [SerializeField]
        private StateData _state;
        #endregion

        #region Lifecycle
        protected override void Start()
        {
            if (!enabled || !gameObject.activeSelf)
            {
                return;
            }

            if (Application.isEditor && !UpdateInEditor)
            {
                return;
            }

            UpdateScrollView();
            SubcribeToViews();
        }
        private void Update()
        {
            if (!enabled || !gameObject.activeSelf)
            {
                return;
            }

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

            UpdateOverlays();
        }
        public void SetLayoutHorizontal()
        {
            if (!enabled || !gameObject.activeSelf)
            {
                return;
            }

            if (Application.isEditor && !UpdateInEditor)
            {
                return;
            }

            UpdateItemState();

            // set dirty as layout have changed
            _state.IsDirty = true;
        }

        public void SetLayoutVertical()
        {
        }

        protected override void OnDestroy()
        {
            _state.ScrollView?.onValueChanged.RemoveListener(OnScrollViewValueChanged);
        }
        #endregion

        #region Overlay
        private void UpdateOverlays()
        {
            if (DisabledOverlay != null)
            {
                DisabledOverlay.gameObject.SetActive(!Enabled);
            }
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

            _state.ScrollView.horizontal = !Enabled ? false : Orientation == ScrollOrientation.Horizontal;
            _state.ScrollView.vertical = !Enabled ? false : !_state.ScrollView.horizontal;
        }
        private void SubscribeToScrollViewEvents()
        {
            if (_state.ScrollView == null)
            {
                return;
            }

            _state.ScrollView.onValueChanged.AddListener(OnScrollViewValueChanged);
        }
        private void OnScrollViewValueChanged(Vector2 scrollValue)
        {
            if (!enabled || !gameObject.activeSelf)
            {
                return;
            }

            UpdateItemState();
            UpdateViews();
        }
        #endregion

        #region Items
        private void UpdateItemState()
        {
            if (_state.ViewportElement == null || _state.ContentElement == null || Items == null)
            {
                return;
            }

            if (Items.Length < 1)
            {
                _state.ActiveItems = 0;
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

            Vector2 firstItemPosition = Vector2.zero;

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
                firstItemPosition.Set(offsetX + localOffsetX, _state.ContentElement.anchoredPosition.y);
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
                    localOffsetY = _state.ItemScaledSize.y + localOffsetY;
                }

                _state.FirstItem = index;
                firstItemPosition.Set(_state.ContentElement.anchoredPosition.x, offsetY + localOffsetY);
            }

            if (!firstItemPosition.Equals(_state.FirstItemPosition))
            {
                _state.IsDirty = true;
            }

            _state.FirstItemPosition = firstItemPosition;
        }

        public void SetItemSelected(int id)
        {
            _state.SelectedItem = id;
            _state.IsDirty = true;

            UpdateItemState();
            UpdateViews();
        }

        public void DeselectItems()
        {
            _state.SelectedItem = -1;
            _state.IsDirty = true;

            UpdateItemState();
            UpdateViews();

        }
        #endregion

        #region Views
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
                SubcribeToViews();
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
                var view = Instantiate(ItemViewPrefab, _state.ContentElement);
                _state.ItemViews.Add(view);
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
            if (!_state.IsDirty || Items == null || _state.ItemViews == null || Items.Length < 1)
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

                view.Id = item.Id;
                view.SetImage(item.Image);
                view.SetLayout(position + paddingCursor, size - padding * 2f);

                if (item.Id == _state.SelectedItem)
                {
                    view.Select();
                }
                else
                {
                    view.Deselect();
                }

                position += cursor;
                // next index, looping
                itemIndex = (itemIndex + 1) % Items.Length;
            }

            _state.IsDirty = false;
        }

        private void OnItemClick(int id)
        {
            OnItemSelect.Invoke(id);
        }

        private void SubcribeToViews()
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

                view.OnClick.RemoveListener(OnItemClick);
                view.OnClick.AddListener(OnItemClick);
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
            _state.IsDirty = true;
        }
        #endregion
    }
}
