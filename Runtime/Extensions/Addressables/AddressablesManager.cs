using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Manager for loading Addressables
    /// </summary>
    [DisallowMultipleComponent]
    public class AddressablesManager : MonoBehaviour
    {
        [Tooltip("The label in the Addressabeles Group for the logo that will be automatically displayed on startup.")]
        [SerializeField] string oneLogoLabel = "OneLogo";
        /// <summary>
        /// Event for getting the sprites that were loaded
        /// </summary>
        public System.Action SpritesAdded;
        /// <summary>
        /// Event for a sprite has loaded
        /// </summary>
        public System.Action<Sprite> SpriteLoaded;
        List<Sprite> loadedSprites;
        /// <summary>
        /// The list of sprites loaded from the Addressables
        /// </summary>
        public List<Sprite> LoadedSprites { get { return loadedSprites; } }
        static AddressablesManager _instance;
        /// <summary>
        /// The Addressables Manager
        /// </summary>
        /// <value>The singleton instance of the addressables manager</value>
        public static AddressablesManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<AddressablesManager>();

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
        }

        /// <summary>
        /// Load the Sprite with the OneLogo label
        /// </summary>
        public void LoadOneLogoSprite()
        {
            Addressables.LoadAssetAsync<Sprite>(oneLogoLabel).Completed += SpriteLoadedCheck;
        }

        void SpriteLoadedCheck(AsyncOperationHandle<Sprite> obj)
        {
            switch (obj.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    Debug.Log("Single Sprite load success.");
                    SpriteLoaded?.Invoke(obj.Result);
                    break;
                case AsyncOperationStatus.Failed:
                    Debug.Log("Single Sprite load failed.");
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Load the Addressables sprites with the sent label
        /// </summary>
        /// <param name="logoLabel">Label for Addressable sprites</param>
        public void LoadSpritesWithLabel(string logoLabel)
        {
            if (!string.IsNullOrEmpty(logoLabel))
            {
                loadedSprites = new List<Sprite>();
                Addressables.LoadAssetsAsync<Sprite>(logoLabel, null).Completed += SpritesLoadedCheck;
            }
        }

        void SpritesLoadedCheck(AsyncOperationHandle<IList<Sprite>> objects)
        {
            switch (objects.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    Debug.Log("Sprite List load success.");
                    BuildSpriteList(objects.Result);
                    break;
                case AsyncOperationStatus.Failed:
                    Debug.Log("Sprite List load failed.");
                    break;
                default:
                    break;
            }
        }

        void BuildSpriteList(IList<Sprite> sprites)
        {
            if (sprites != null && sprites.Count > 0)
            {
                foreach (var _sprite in sprites)
                {
                    Debug.Log("Found sprite " + _sprite.name);
                    if (!loadedSprites.Contains(_sprite))
                        loadedSprites.Add(_sprite);
                }
                SpritesAdded?.Invoke();
            }
        }
    }
}