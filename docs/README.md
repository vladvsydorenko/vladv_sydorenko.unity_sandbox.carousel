# Image Gallery Test

This is a quick test implementation of image gallery with infinite virtual scrolling.

# In few words

## CarouselView
To render items, `CarouselView` determines the number of images to render based on size of viewport (root content element where images will be added to).

If first rendered item position if changed during update (see `CarouselView.UpdateItemState` method) - items are "dirty" and need to reposition and images update.

`CarouselView` doesn't create all elements for all images - it creates only those to fit in viewport (even if there are less images to fill viewport size - extra items will be created with going by round).

To instantiate image elements, `CarouselView` takes reference to prefab with `CarouselItemViewBase` component. It's an abstract class, so user could implement his own item view.

Items images and amount could be changed anytime by setting `Items` field in `CarouselView`.

## GalleryView
`GalleryView` takes refs to `CarouselView` components to update data (images to be rendered).

Image data (`GalleryItemData`) contains image and big image sprites.
Image is used for rendering inside scrollbar, big image (if provided) for full image view.

