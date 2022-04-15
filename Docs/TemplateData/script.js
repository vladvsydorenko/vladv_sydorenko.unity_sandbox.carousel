function startUnityHandler() {
  var pathPrefix = "./";
  var buildUrl = pathPrefix + "WebGL/PublicBuild";
  var loaderUrl = buildUrl + "/WebGL.loader.js";
  var config = {
    dataUrl: buildUrl + "/WebGL.data",
    frameworkUrl: buildUrl + "/WebGL.framework.js",
    codeUrl: buildUrl + "/WebGL.wasm",
    streamingAssetsUrl: "StreamingAssets",
    companyName: "Vladnets",
    productName: "Carousel Gallery",
    productVersion: "1.1.1.0",
    webglContextAttributes: {"preserveDrawingBuffer": true}
  };

  var container = document.querySelector("#unity-container");
  var canvas = document.querySelector("#unity-canvas");
  var canvasWrapper = document.querySelector("#unity-canvas-wrapper");
  var loadingBar = document.querySelector("#unity-loading-bar");
  var progressBarFull = document.querySelector("#unity-progress-bar-full");
  var fullscreenButton = document.querySelector("#unity-fullscreen-button");
  var mobileWarning = document.querySelector("#unity-mobile-warning");

  // By default Unity keeps WebGL canvas render target size matched with
  // the DOM size of the canvas element (scaled by window.devicePixelRatio)
  // Set this to false if you want to decouple this synchronization from
  // happening inside the engine, and you would instead like to size up
  // the canvas DOM size and WebGL render target sizes yourself.
  config.matchWebGLToCanvasSize = false;

  if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
    container.className = "unity-mobile";
    // Avoid draining fillrate performance on mobile devices,
    // and default/override low DPI mode on mobile browsers.
    config.devicePixelRatio = 1;
    mobileWarning.style.display = "block";
    setTimeout(() => {
      mobileWarning.style.display = "none";
    }, 5000);
  } else {
    container.className = "unity-desktop";
    // canvas.style.width = "1280px";
    // canvas.style.height = "800px";
  }
  loadingBar.style.display = "block";

  var script = document.createElement("script");
  script.src = loaderUrl;
  script.onload = () => {
    canvas.style.display = "none";

    createUnityInstance(canvas, config, (progress) => {
      progressBarFull.style.width = 100 * progress + "%";
    }).then((unityInstance) => {
      let timeoutId = -1;

      canvas.width = `${canvasWrapper.offsetWidth}`;
      canvas.height = `${canvasWrapper.offsetHeight}`;
      canvas.style.display = "block";

      window.canvas = canvas;

      window.addEventListener("resize", () => {
        clearTimeout(timeoutId);

        canvas.style.display = "none";
        timeoutId = setTimeout(() => {
          canvas.width = `${canvasWrapper.offsetWidth}`;
          canvas.height = `${canvasWrapper.offsetHeight}`;
          canvas.style.display = "block";
        }, 500);

      });

      loadingBar.style.display = "none";
      fullscreenButton.onclick = () => {
        unityInstance.SetFullscreen(1);
      };
    }).catch((message) => {
      alert(message);
    });
  };
  document.body.appendChild(script);
}

startUnityHandler();
