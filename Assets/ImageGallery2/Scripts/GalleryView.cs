﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery2.Scripts
{
    [ExecuteInEditMode]
    public class GalleryView : UIBehaviour
    {
        #region Settings
        [Header("Settings")]
        public bool UpdateInEditMode;
        public CarouselItemViewBase BigImage;
        public RectTransform HelpText;
        public RectTransform CloseHelpText;
        public CarouselView[] Carousels;
        #endregion

        #region Data
        [Header("Data")]
        public GalleryItemData[] Items;
        #endregion

        protected override void Start()
        {
            SubscribeToEvents();

            if (BigImage != null)
            {
                BigImage.gameObject.SetActive(false);
            }

            if (HelpText != null)
            {
                HelpText.gameObject.SetActive(true);
            }

            if (CloseHelpText != null)
            {
                CloseHelpText.gameObject.SetActive(false);
            }

            EnableCarousels();
        }

        private void Update()
        {
            if (Application.isEditor && !UpdateInEditMode)
            {
                return;
            }

            if (Carousels == null)
            {
                return;
            }

            for (int i = 0; i < Carousels.Length; i++)
            {
                var carousel = Carousels[i];
                if (carousel == null)
                {
                    continue;
                }

                carousel.Items = Items;
            }
        }

        private void OnItemSelect(int id)
        {
            if (BigImage == null || id >= Items.Length)
            {
                return;
            }

            var item = Items[id];
            BigImage.Id = item.Id;
            BigImage.SetImage(item.BigImage != null ? item.BigImage : item.Image);
            BigImage.gameObject.SetActive(true);

            if (HelpText != null)
            {
                HelpText.gameObject.SetActive(false);
            }

            if (CloseHelpText != null)
            {
                CloseHelpText.gameObject.SetActive(true);
            }

            DisableCarousels();
        }

        private void OnBigImageClick(int id)
        {
            if (BigImage != null)
            {
                BigImage.gameObject.SetActive(false);
            }

            if (HelpText != null)
            {
                HelpText.gameObject.SetActive(true);
            }

            if (CloseHelpText != null)
            {
                CloseHelpText.gameObject.SetActive(false);
            }

            EnableCarousels();
        }

        private void SubscribeToEvents()
        {
            foreach (var carousel in Carousels)
            {
                if (carousel == null)
                {
                    continue;
                }

                carousel.OnItemSelect.AddListener(OnItemSelect);
            }

            if (BigImage != null)
            {
                BigImage.OnClick.AddListener(OnBigImageClick);
            }
        }

        private void EnableCarousels()
        {
            foreach (var carousel in Carousels)
            {
                if (carousel == null)
                {
                    return;
                }

                carousel.Enabled = true;
            }
        }

        private void DisableCarousels()
        {
            foreach (var carousel in Carousels)
            {
                if (carousel == null)
                {
                    return;
                }

                carousel.Enabled = false;
            }
        }

        protected override void OnDestroy()
        {
            foreach (var carousel in Carousels)
            {
                if (carousel == null)
                {
                    return;
                }

                carousel.OnItemSelect.RemoveListener(OnItemSelect);
            }

            if (BigImage != null)
            {
                BigImage.OnClick.RemoveListener(OnBigImageClick);
            }
        }
    }
}
