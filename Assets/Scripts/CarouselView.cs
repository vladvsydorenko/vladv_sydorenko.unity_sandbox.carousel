using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Carousel
{
    public class CarouselView : UIBehaviour
    {
        [Serializable]
        private struct Item
        {
            public Vector2 Size;
            public Vector2 Position;
            public Sprite Sprite;
        }

        public enum CarouselDirection { Horizontal, Vertical };

        public float ExtraSpace;
        public CarouselDirection Direction;
        public ScrollRect ScrollView;
        public CarouselItemView ItemViewPrefab;

        [Header("Items")]
        [SerializeField]
        private List<Item> _items;
        [SerializeField]
        private List<CarouselItemView> _views;

        [Header("Cursor")]
        [SerializeField]
        private float _cursorStart;
        [SerializeField]
        private float _cursorEnd;

        [Header("Content")]
        [SerializeField]
        private float _contentSize;
        [SerializeField]
        private float _contentStart;

        [Header("Debug")]
        [SerializeField]
        private int _firstItem;
        [SerializeField]
        private int _renderedItemsCount;

        private void Update()
        {
            UpdateCursor();
            RenderItems();
        }

        private void RenderItems()
        {
            if (ItemViewPrefab == null)
            {
                return;
            }

            // find first item by cursor
            _firstItem = FindFirstItem();

            // reset rendererd items counter
            _renderedItemsCount = 0;

            if (Direction == CarouselDirection.Horizontal)
            {
                var firstItem = _items[_firstItem];
                float offset = _cursorStart - (_contentStart - firstItem.Position.x);

                int index = _firstItem;
                int viewIndex = 0;
                while (offset < _cursorEnd)
                {
                    var item = _items[index];

                    CarouselItemView view;

                    if (viewIndex < _views.Count)
                    {
                        view = _views[viewIndex];
                    }
                    else
                    {
                        view = Instantiate(ItemViewPrefab, ScrollView.content);
                        _views.Add(view);
                    }

                    view.SetSize(item.Size);
                    view.SetPosition(new Vector2
                    {
                        x = offset,
                        y = 0,
                    });
                    view.SetSprite(item.Sprite);

                    _renderedItemsCount++;
                    offset += item.Size.x;

                    // increase index cyclic
                    index = (index + 1) % _items.Count;
                    viewIndex++;
                }
            }

            // deactivate not used views
            if (_renderedItemsCount < _views.Count)
            {
                for (int i = _renderedItemsCount; i < _views.Count; i++)
                {
                    _views[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateCursor()
        {
            if (Direction == CarouselDirection.Horizontal)
            {
                _cursorStart = -ScrollView.content.anchoredPosition.x - ExtraSpace;
                _cursorEnd = -ScrollView.content.anchoredPosition.x + ScrollView.viewport.rect.width + ExtraSpace;

                _contentStart = -(_cursorStart % _contentSize);
                _contentStart = _contentStart >= 0f ? _contentStart : _contentSize + _contentStart;
            }
        }

        private int FindFirstItem()
        {
            if (Direction == CarouselDirection.Horizontal)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];

                    if (item.Position.x <= _contentStart && item.Position.x + item.Size.x >= _contentStart)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }
    }
}
