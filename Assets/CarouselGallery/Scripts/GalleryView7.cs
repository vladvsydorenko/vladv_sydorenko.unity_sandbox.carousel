using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.CarouselGallery.Scripts
{

    public class GalleryView7 : UIBehaviour
    {
        [Header("Settings")]
        public float SafeZoneSize;
        public float RenderZoneSize;
        public ScrollRect ScrollView;
        public GalleryItemView7 ItemViewPrefab;

        [Header("Layout")]
        [SerializeField]
        private float _offset;

        private int _updateVersion;
        private int _renderVersion;

        private Rect _safeArea;

        private Rect _renderArea;
        private Rect _renderAreaCache;

        [SerializeField]
        private Rect _contentArea;
        private Rect _viewportArea;

        [Header("Items")]
        private List<GalleryItemView7> _views;
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
            _offset = 0f;
            _views = new List<GalleryItemView7>();
            _viewsPool = new List<int>();

            if (ScrollView == null)
            {
                ScrollView = GetComponent<ScrollRect>();
            }

            UpdateGallery();

            _contentArea = _viewportArea;
            _contentArea.width = 0f;

            RenderGallery();

            ScrollView.onValueChanged.AddListener(OnScrollViewChange);
        }

        public void OnScrollViewChange(Vector2 position)
        {
            UpdateGallery();
            RenderGallery();
        }

        private void UpdateGallery()
        {
            _updateVersion++;

            UpdateViewportArea();
            UpdateSafeArea();
            UpdateRenderArea();
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


        private void RenderGallery()
        {
            if (_renderVersion != _updateVersion)
            {
                return;
            }

            Debug.Log("Re-render");
        }

        private GalleryItemView7 GetOrCreateView()
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