using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VladvSydorenko.UnitySandbox.Assets.ImageGallery.Scripts
{
    public class DemoController : MonoBehaviour
    {
        public GalleryView Gallery;
        public Sprite[] Sprites;

        [ContextMenu(nameof(CreateGalleryItemsFromSprites))]
        public void CreateGalleryItemsFromSprites()
        {
            if (Sprites == null)
            {
                return;
            }

            var items = new List<GalleryItemData>(Sprites.Length);
            var id = 0;
            for (int i = 0; i < Sprites.Length; i++)
            {
                var sprite = Sprites[i];

                if (sprite == null)
                {
                    continue;
                }

                items.Add(new GalleryItemData
                {
                    Id = id,
                    Image = sprite,
                    BigImage = sprite
                });

                id++;
            }

            Gallery.Items = items.ToArray();
        }
    }
}
