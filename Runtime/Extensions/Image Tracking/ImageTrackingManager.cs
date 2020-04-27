using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Manages AR image target addition, tracking and removal.
    /// Uses the Reference Image Library selected to display each tracked image where the user can then assign the image tracking handler to that image.
    /// Currently tracked image manager assumes tracking of one image at a time, not simultaneous tracked images
    /// </summary>
    [DisallowMultipleComponent]
    public class ImageTrackingManager : MonoBehaviour
    {
        [Tooltip("The Session Origins for this manager to track.")]
        [SerializeField] ARSessionOrigin[] sessionOrigins = default;
        [Tooltip("The Reference Image Library to use.")]
        [SerializeField] XRReferenceImageLibrary referenceImageLibrary;
        /// <summary>
        /// General event fired when tracking is found
        /// </summary>
        public Action<ARTrackedImage> TrackingFound;
        /// <summary>
        /// General event fired when tracking is lost
        /// </summary>
        public Action<ARTrackedImage> TrackingLost;
        /// <summary>
        /// Event fired when AR Capability check is complete including if the device is AR compatible
        /// </summary>
        public Action<bool> InitialARCapabilityCheck;
        /// <summary>
        /// If AR is supported on this device.
        /// </summary>
        /// <value>True if AR is supported, false otherwise.</value>
        public bool ARSupported { get { return aRSupported; } }
        /// <summary>
        /// The currently tracked image
        /// </summary>
        /// <value>The currently tracked ARTrackedImage</value>
        public ARTrackedImage CurrentlyTrackedImage { get { return currentlyTrackedImage; } }

        Dictionary<string, string> imageTargetHandlerLookup;
        List<IHandleImageTargets> imageTargetHandlerNotifyList = new List<IHandleImageTargets>();
        List<ILocateImageTargets> imageTargetLocateList = new List<ILocateImageTargets>();
        List<ARTrackedImageManager> imageManagers = new List<ARTrackedImageManager>();
        ARTrackedImage foundImage;
        ARTrackedImage currentlyTrackedImage;
        bool aRSupported;

        static ImageTrackingManager _instance;
        /// <summary>
        /// The Image Target Manager
        /// </summary>
        /// <value>The singleton instance of the image target manager</value>
        public static ImageTrackingManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<ImageTrackingManager>();

                return _instance;
            }
            set => _instance = value;
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;

            // Get Tracked Image Managers
            if (sessionOrigins != null)
            {
                foreach (var sessionOrigin in sessionOrigins)
                {
                    var imageManager = sessionOrigin.GetComponent<ARTrackedImageManager>();
                    if (imageManager != null && !imageManagers.Contains(imageManager))
                        imageManagers.Add(imageManager);
                }
            }

            // Build dictionary since you can not serialize it in the editor
            imageTargetHandlerLookup = new Dictionary<string, string>();
            foreach (var info in handlerInfoList)
            {
                if (info == null || string.IsNullOrEmpty(info.imageGUID))
                {
                    Debug.LogWarningFormat("There was a problem creating the Image Target handler lookup. Unexpected results most likely will occur.");
                    continue;
                }
                if (string.IsNullOrEmpty(info.handlerName))
                {
                    Debug.Log(info.imageGUID + " does not have an handler.");
                    continue;
                }
                if (!imageTargetHandlerLookup.ContainsKey(info.imageGUID))
                {
                    imageTargetHandlerLookup.Add(info.imageGUID, info.handlerName);
                }
            }
        }

        void Start()
        {
            // Is AR supported?
            StartCoroutine(CheckARAvailability());
        }

        void OnEnable()
        {
            foreach (var imageManager in imageManagers)
                imageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }
        void OnDisable()
        {
            foreach (var imageManager in imageManagers)
                imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
            ARSession.stateChanged -= ARStateChanged;
        }

        IEnumerator CheckARAvailability()
        {
            if ((ARSession.state == ARSessionState.None) || (ARSession.state == ARSessionState.CheckingAvailability))
            {
                yield return ARSession.CheckAvailability();
            }
            aRSupported = ARSession.state != ARSessionState.Unsupported;
            // Fire event passing if AR is supported
            InitialARCapabilityCheck?.Invoke(aRSupported);

            // Subscribe to future changes
            ARSession.stateChanged += ARStateChanged;
        }

        // If AR support has changed
        void ARStateChanged(ARSessionStateChangedEventArgs args)
        {
            aRSupported = args.state != ARSessionState.Unsupported;
        }

        // AR Foundation's AR Tracked Image changes
        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.updated)
            {
                if (trackedImage.trackingState == TrackingState.None)
                {
                    if (trackedImage == foundImage)
                    {
                        // Lost currently tracked image
                        foundImage = null;
                        currentlyTrackedImage = null;

                        // Notify image target's handler of lost tracking
                        if (imageTargetHandlerLookup.ContainsKey(trackedImage.referenceImage.guid.ToString()))
                        {
                            string imageTargetHandler = imageTargetHandlerLookup[trackedImage.referenceImage.guid.ToString()];
                            foreach (var handler in imageTargetHandlerNotifyList)
                            {
                                if (imageTargetHandler == handler.GetType().Name)
                                {
                                    handler.LostTracking(trackedImage);
                                    break;
                                }
                            }
                        }

                        // Fire general event in case it is used
                        TrackingOff(foundImage);
                    }
                }
                else if (trackedImage.trackingState == TrackingState.Tracking && foundImage != trackedImage)
                {
                    // Now tracking this image
                    foundImage = trackedImage;
                    currentlyTrackedImage = trackedImage;

                    // Notify image target's handler of found tracking
                    if (imageTargetHandlerLookup.ContainsKey(trackedImage.referenceImage.guid.ToString()))
                    {
                        string imageTargetHandler = imageTargetHandlerLookup[trackedImage.referenceImage.guid.ToString()];
                        foreach (var handler in imageTargetHandlerNotifyList)
                        {
                            if (imageTargetHandler == handler.GetType().Name)
                            {
                                handler.FoundTracking(trackedImage);
                                break;
                            }
                        }
                    }

                    // Fire general event in case it is used
                    TrackingOn(foundImage);
                }
                else if (trackedImage.trackingState == TrackingState.Limited)
                {
                    // Still tracking same image
                    if (trackedImage == currentlyTrackedImage)
                    {
                        foundImage = null;
                    }
                }
            }

            foreach (var trackedImage in eventArgs.removed)
            {
                if (trackedImage == currentlyTrackedImage)
                {
                    // Not currently any image target
                    currentlyTrackedImage = null;
                    foundImage = null;

                    // Notify image target's handler of lost tracking
                    if (imageTargetHandlerLookup.ContainsKey(trackedImage.referenceImage.guid.ToString()))
                    {
                        string imageTargetHandler = imageTargetHandlerLookup[trackedImage.referenceImage.guid.ToString()];
                        foreach (var handler in imageTargetHandlerNotifyList)
                        {
                            if (imageTargetHandler == handler.GetType().Name)
                            {
                                handler.LostTracking(trackedImage);
                                break;
                            }
                        }
                    }

                    // Fire general event in case it is used
                    TrackingOff(foundImage);
                }
            }
        }

        #region General Tracking Events
        // Fire general found tracking event
        void TrackingOn(ARTrackedImage target)
        {
            TrackingFound?.Invoke(target);
        }

        // Fire general lost tracking event
        void TrackingOff(ARTrackedImage target)
        {
            TrackingLost?.Invoke(target);
        }
        #endregion

        #region Public Access Methods
        /// <summary>
        /// This image target handler wants to be notified of tracking changes
        /// </summary>
        /// <param name="handler">Implementation of the IHandleImageTargets interface</param>
        public void AttachTrackingHandler(IHandleImageTargets handler)
        {
            if (!imageTargetHandlerNotifyList.Contains(handler))
                imageTargetHandlerNotifyList.Add(handler);
        }

        /// <summary>
        /// This image target handler no longer wants to be notified of tracking changes
        /// </summary>
        /// <param name="handler">Implementation of the IHandleImageTargets interface</param>
        public void DetachTrackingHandler(IHandleImageTargets handler)
        {
            if (imageTargetHandlerNotifyList.Contains(handler))
                imageTargetHandlerNotifyList.Remove(handler);
        }

        /// <summary>
        /// This image target handler wants to be notified of image location changes
        /// </summary>
        /// <param name="locater">Implementation of the ILocateImageTargets interface</param>
        public void AttachLocater(ILocateImageTargets locater)
        {
            if (!imageTargetLocateList.Contains(locater))
                imageTargetLocateList.Add(locater);
        }

        /// <summary>
        /// This image target handler no longer wants to be notified of image location changes
        /// </summary>
        /// <param name="locater">Implementation of the ILocateImageTargets interface</param>
        public void DetachLocater(ILocateImageTargets locater)
        {
            if (imageTargetLocateList.Contains(locater))
                imageTargetLocateList.Remove(locater);
        }

        /// <summary>
        /// Send location via the Bounds property to the listening image target handlers
        /// </summary>
        /// <param name="_bounds">Bounds use to calculate location of image target</param>
        public void RelocateImageTarget(Bounds _bounds)
        {
            foreach (var locater in imageTargetLocateList)
            {
                locater.LocateImageTarget(_bounds);
            }
        }

        /// <summary>
        /// Send location via the Vector3 position to the listening image target handlers
        /// </summary>
        /// <param name="_position">The Vector3 position to locate the target</param>
        public void RelocateImageTarget(Vector3 _position)
        {
            foreach (var locater in imageTargetLocateList)
            {
                locater.LocateImageTarget(_position);
            }
        }

        /// <summary>
        /// Notify listening image target handlers to stop handling AR
        /// </summary>
        public void StopARMode()
        {
            var tempNotifyList = new List<IHandleImageTargets>(imageTargetHandlerNotifyList);
            foreach (var handler in tempNotifyList)
                handler.StopHandlingAR();
        }
        #endregion

        /// <summary>
        /// Info to match up image target with handler. Used by the custom editor.
        /// </summary>
        [Serializable]
        public class ImageTargetHandlerInfo
        {
            /// <summary>
            /// The tracked image GUID
            /// </summary>
            public string imageGUID;
            /// <summary>
            /// The image target handler name to use
            /// </summary>
            public string handlerName;
        }

        // Populated by the custome editor
        [SerializeField, HideInInspector]
        List<ImageTargetHandlerInfo> handlerInfoList = new List<ImageTargetHandlerInfo>();
    }
}