using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery.Scripts
{
    public class ImageCarouselView : UIBehaviour, ILayoutGroup
    {
        #region Definitions
        public enum ScrollOrientation
        {
            Horizontal,
            Vertical
        }
        #endregion

        #region Properties
        [Header("Settings")]
        [Tooltip("Orientation of carousel scrolling")]
        public ScrollOrientation Orientation;

        [Tooltip("Extra space on sides where content doesn't rebuild")]
        public float SafeExtraSpace;

        [Tooltip("Extra space on sides; x for horizontal orientation, y for vertical")]
        public float RenderExtraSpace;

        [Header("Settings: Layout")]
        [Tooltip("Preferred size of image")]
        public Vector2 ImageSize;

        [Tooltip("Spacing around each image")]
        public Vector2 ImagePadding;

        [Header("Settings: Prefabs")]
        public ImageCarouselItemViewBase ImagePrefab;

        [Header("Settings: References")]
        [Tooltip("Set it if ScrollRect is not attached to this GameObject")]
        public ScrollRect ScrollRectRef;

        [Header("Data")]
        [Tooltip("Sprites to be rendered")]
        public Sprite[] Images;
        #endregion

        #region State
        [Header("State")]
        [SerializeField]
        private bool _isDirty;

        [SerializeField]
        private Vector2 _normal;

        [SerializeField]
        private Rect _safeArea;
        private Rect _renderArea;
        private Rect _contentArea;
        private Rect _viewportArea;

        [SerializeField]
        private Rect _imageArea;
        [SerializeField]
        private Vector2 _imageSizeScaled;

        private int _firstItem;
        private int _activeItems;
        [SerializeField]
        private List<ImageCarouselItemViewBase> _views;

        private ScrollRect _scrollRect;
        private RectTransform _content;
        private RectTransform _viewport;

        [Header("Debug")]
        [SerializeField]
        private RectTransform _testElement;
        #endregion

        #region Life Cycle
        protected override void Start()
        {
            UpdateNormal();

            // find reference to scroll rect
            CollectScrollRect();

            UpdateViewportArea();
            UpdateImageLayout();
            UpdateContentLayout();
            RenderImages();

            UpdateDirtyState();


            // subscribe to scroll changes
            _scrollRect.onValueChanged.AddListener(OnScrollRectChanged);
        }

        private void Update()
        {
            UpdateDirtyState();
            UpdateContentLayout();
            UpdateViewportArea();
            RenderImagesIfDirty();
        }

        protected override void OnDestroy()
        {
            // unsubscribe from scroll changes
            _scrollRect.onValueChanged?.RemoveListener(OnScrollRectChanged);
        }
        #endregion

        #region Layout Group
        public void SetLayoutHorizontal()
        {
            UpdateNormal();

            CollectScrollRect();
            UpdateViewportArea();
            UpdateImageLayout();
            UpdateContentLayout();

            UpdateDirtyState();
            _isDirty = true;

            // RenderImagesIfDirty();
        }

        public void SetLayoutVertical()
        {
        }
        #endregion

        #region ScrollRect
        private void OnScrollRectChanged(Vector2 scroll)
        {
            UpdateViewportArea();
            UpdateDirtyState();
            RenderImagesIfDirty();
        }

        private void CollectScrollRect()
        {
            if (_scrollRect != null)
            {
                return;
            }

            _scrollRect = ScrollRectRef;

            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            if (_scrollRect == null)
            {
                _scrollRect = GetComponentInChildren<ScrollRect>();
            }

            if (_scrollRect == null)
            {
                return;
            }

            _content = _scrollRect.content;
            _viewport = _scrollRect.viewport;
        }
        #endregion

        #region Updates
        private void UpdateViewportArea()
        {
            if (_viewport == null || _content == null)
            {
                return;
            }

            _viewportArea.size = new Vector2(_viewport.rect.width, _viewport.rect.height);
            _viewportArea.position = new Vector2(-_content.anchoredPosition.x, -_content.anchoredPosition.y);
        }
        private void UpdateImageLayout()
        {
            float scale = Orientation == ScrollOrientation.Horizontal ?
                _viewportArea.height / ImageSize.y :
                _viewportArea.width / ImageSize.x;

            _imageSizeScaled = ImageSize * scale;

            _imageArea.size = _imageSizeScaled - ImagePadding * 2f;
            _imageArea.position = new Vector2(ImagePadding.x, -ImagePadding.y);

            if (_content != null)
            {
                // _content.sizeDelta = _imageArea.size;
                // _content.anchoredPosition = _imageArea.position;
            }
        }
        private void UpdateContentLayout()
        {
            if (_content == null)
            {
                return;
            }

            var pivot = new Vector2(0f, 1f);
            var anchorMin = pivot;
            var anchorMax = pivot;

            _content.pivot = pivot;
            _content.anchorMin = anchorMin;
            _content.anchorMax = anchorMax;
            //_content.sizeDelta = Vector2.zero;
        }
        private void UpdateNormal()
        {
            _normal = Orientation == ScrollOrientation.Horizontal ? Vector2.right : Vector2.down;
        }
        private void UpdateDirtyState()
        {
            if (Orientation == ScrollOrientation.Horizontal)
            {
                if (_viewportArea.x < _safeArea.x || _viewportArea.xMax > _safeArea.xMax)
                {
                    _isDirty = true;
                }
            }
            else
            {
                if (_viewportArea.y < _safeArea.y || _viewportArea.yMax > _safeArea.yMax)
                {
                    _isDirty = true;
                }
            }
        }
        #endregion

        #region Renders
        private void RenderImagesIfDirty()
        {
            if (_isDirty)
            {
                RenderImages();

                _isDirty = false;
            }
        }

        [ContextMenu("DestroyImages")]
        private void DestroyImages()
        {
            for (int i = 0; i < _views.Count; i++)
            {
                var view = _views[i];
                if (view == null)
                {
                    continue;
                }

                DestroyImmediate(view.gameObject);
            }

            _views.Clear();
        }

        [ContextMenu("RenderImages")]
        private void RenderImages()
        {
            var safeExtraSpace = (_imageSizeScaled.magnitude * SafeExtraSpace);
            var renderExtraSpace = (_imageSizeScaled.magnitude * RenderExtraSpace);

            _safeArea.position = _viewportArea.position - (_normal * safeExtraSpace);
            _renderArea.position = _viewportArea.position - (_normal * renderExtraSpace);

            var _normalAbs = new Vector2(Mathf.Abs(_normal.x), Mathf.Abs(_normal.y));
            _safeArea.size = _viewportArea.size + (_normalAbs * safeExtraSpace * 2f);
            _renderArea.size = _viewportArea.size + (_normalAbs * renderExtraSpace * 2f);

            _contentArea = _renderArea;

            Vector2 offset = Vector2.zero;
            Vector2 itemPosition = Vector2.zero;

            if (Orientation == ScrollOrientation.Horizontal)
            {
                _activeItems = (int)(_renderArea.width / _imageSizeScaled.x) + 2;
                var realWidth = _imageSizeScaled.x * Images.Length;
                var x = _renderArea.x % realWidth;
                if (x < 0f)
                {
                    x = realWidth + x;
                }

                _firstItem = (int)(x / _imageSizeScaled.x);
                itemPosition = _renderArea.position;
                var excess = x % _imageSizeScaled.x;
                itemPosition.x -= excess;

                offset = new Vector2(_imageSizeScaled.x, 0f);
            }
            else
            {
                _activeItems = (int)(_renderArea.height / _imageSizeScaled.y) + 2;
                var realHeight = _imageSizeScaled.y * Images.Length;
                var y = _renderArea.y % realHeight;
                if (y > 0f)
                {
                    y = -realHeight + y;
                }

                _firstItem = Mathf.Abs((int)(Mathf.Abs(y) / _imageSizeScaled.y));
                itemPosition = _renderArea.position;
                var excess = y % _imageSizeScaled.y;
                itemPosition.y -= excess;

                offset = new Vector2(0f, -_imageSizeScaled.y);
            }

            if (_testElement != null)
            {
                _testElement.anchoredPosition = _safeArea.position;
                _testElement.sizeDelta = _safeArea.size;
            }

            var index = _firstItem;
            for (int i = 0; i < _activeItems; i++)
            {
                var view = GetOrCreateView(i);
                if (view == null)
                {
                    continue;
                }

                index = index % Images.Length;

                view.SetImage(Images[index]);
                view.SetLayout(itemPosition + _imageArea.position, _imageArea.size);
                view.gameObject.SetActive(true);

                itemPosition += offset;
                index++;
            }

            if (_views != null)
            {
                for (int i = _activeItems; i < _views.Count; i++)
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
        private ImageCarouselItemViewBase GetOrCreateView(int viewIndex)
        {
            if (_views == null)
            {
                _views = new List<ImageCarouselItemViewBase>(_activeItems);
            }

            if (ImagePrefab == null)
            {
                return null;
            }

            if (_views.Count > viewIndex)
            {
                return _views[viewIndex];
            }
            var view = Instantiate(ImagePrefab, _content);
            _views.Add(view);
            return view;
        }
        #endregion
    }
}
