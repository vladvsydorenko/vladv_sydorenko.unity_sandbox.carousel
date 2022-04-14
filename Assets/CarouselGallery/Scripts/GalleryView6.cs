using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.CarouselGallery.Scripts
{
    public class GalleryView6 : UIBehaviour
    {
        [Header("Settings")]
        public float SafeZoneSize;
        public float RenderZoneSize;
        public ScrollRect ScrollView;
        public GalleryItemView2 ItemViewPrefab;

        [Header("Layout")]
        private float _offset;

        private int _updateVersion;
        private int _renderVersion;

        private Rect _safeArea;

        private Rect _renderArea;
        private Rect _renderAreaCache;

        private Rect _contentArea;
        private Rect _viewportArea;

        [Header("Items")]
        private List<GalleryItemView2> _views;
        private List<int> _viewsPool;

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
            if (ScrollView == null)
            {
                ScrollView = GetComponent<ScrollRect>();
            }

            _offset = 0f;

            _views = new List<GalleryItemView2>();
            _viewsPool = new List<int>();

            UpdateGallery();
        }

        public void OnScrollViewChange(Vector2 position)
        {
            UpdateGallery();
        }

        private void UpdateGallery()
        {
            _updateVersion++;

            UpdateViewportArea();
            UpdateSafeArea();
            UpdateRenderArea();
            UpdateContent();
        }

        private void UpdateViewportArea()
        {
            if (ScrollView == null || ScrollView.viewport == null || ScrollView.content == null)
            {
                return;
            }

            _viewportArea.x = -ScrollView.content.anchoredPosition.x;
            _viewportArea.size = new Vector2(ScrollView.viewport.rect.width, ScrollView.viewport.rect.height);

            if (_viewportAreaElement)
            {
                _viewportAreaElement.sizeDelta = _viewportArea.size;
                _viewportAreaElement.anchoredPosition = _viewportArea.position;
            }
        }

        private void UpdateSafeArea()
        {
            var zoneSize = Mathf.Min(SafeZoneSize, RenderZoneSize);

            var x = _offset - zoneSize;
            var width = _viewportArea.width + (zoneSize * 2f);

            if (x > _viewportArea.x || x + width < _viewportArea.xMax)
            {
                _offset = _viewportArea.x;
                x = _offset - zoneSize;
            }

            _safeArea.Set(x, 0f, width, _viewportArea.height);

            if (_safeAreaElement != null)
            {
                _safeAreaElement.sizeDelta = _safeArea.size;
                _safeAreaElement.anchoredPosition = _safeArea.position;
            }
        }

        private void UpdateRenderArea()
        {
            _renderArea.Set(_offset - RenderZoneSize, 0f, _viewportArea.width + (RenderZoneSize * 2f), _viewportArea.height);

            if (_renderArea != _renderAreaCache)
            {
                _renderVersion = _updateVersion;
            }

            _renderAreaCache = _renderArea;

            if (_renderAreaElement != null)
            {
                _renderAreaElement.sizeDelta = _renderArea.size;
                _renderAreaElement.anchoredPosition = _renderArea.position;
            }
        }
        
        private void UpdateContent()
        {
            if (_renderVersion != _updateVersion)
            {
                return;
            }

            UpdateViews();

            if (_contentArea.xMax < _renderArea.xMax)
            {
                Debug.Log($"ADD: {_renderArea.xMax - _contentArea.xMax} at the end");

                while (_contentArea.xMax < _renderArea.xMax)
                {
                    var view = GetOrCreateView();
                    view.SetSize(new Vector2(100f, _renderArea.height));
                    view.SetPosition(new Vector2(_contentArea.xMax, 0f));
                    view.gameObject.SetActive(true);

                    _contentArea.width += 100f;
                }
            }
            else if (_contentArea.xMax > _renderArea.xMax)
            {
            }

            if (_contentArea.x > _renderArea.x)
            {
            }
            else if (_contentArea.x < _renderArea.x)
            {
            }

            _contentArea.height = _renderArea.height;

            if (_contentAreaElement != null)
            {
                _contentAreaElement.sizeDelta = new Vector2(_contentArea.width, _contentArea.height);
            }
        }

        private void UpdateViews()
        {
            if (_views.Count < 1 || _viewsPool.Count == _views.Count)
            {
                return;
            }

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;

            // remove invisble
            for (int i = 0; i < _views.Count; i++)
            {
                var view = _views[i];

                if (!view.gameObject.activeSelf)
                {
                    continue;
                }

                var area = view.ItemArea;

                if (area.x > _renderArea.xMax || area.xMax < _renderArea.x)
                {
                    view.gameObject.SetActive(false);
                    _viewsPool.Add(view.Index);

                    continue;
                }

                if (area.x < minX)
                {
                    minX = area.x;
                }
                if (area.xMax > maxX)
                {
                    maxX = area.xMax;
                }
            }

            _contentArea.x = minX;
            _contentArea.width = maxX - minX;

            // calculate 
/*            for (int i = 0; i < _views.Count; i++)
            {
                var view = _views[i];
                if (!view.gameObject.activeSelf)
                {
                    continue;
                }

                var area = view.ItemArea;

            }
*/
        }

        private GalleryItemView2 GetOrCreateView()
        {
            if (_viewsPool.Count > 0)
            {
                var lastIndex = _viewsPool.Count - 1;
                var index = _viewsPool[lastIndex];
                _viewsPool.RemoveAt(lastIndex);
                return _views[index];
            }

            var view = Instantiate(ItemViewPrefab, ScrollView.content);
            view.Index = _views.Count;
            _views.Add(view);

            return view;
        }
    }
}