using UnityEngine.UI;

namespace UnityEngine.Reflect.Extensions
{
    /// <summary>
    /// Builds the logo list to choose from and then handles replacing the logo when it is choosen
    /// </summary>
    public class LogoController : MonoBehaviour
    {
        [Tooltip("If you want to automatically load the Logo with the OneLogo label in the Addressabeles Group.")]
        [SerializeField] bool useOneLogoLabel = default;
        [Tooltip("Label for the Addressables sprites.")]
        [SerializeField] string logoLabel = default;
        [Tooltip("Panel where all the logo choices are displayed.")]
        [SerializeField] RectTransform LogoPanel = default;
        [Tooltip("Logo Button template to be used for the choices.")]
        [SerializeField] Button LogoButton = default;
        [Tooltip("The logo image that is displayed in top corner.")]
        [SerializeField] Image BrandedLogoImage = default;

        const float BUTTONSPACE = 0.02f;
        float buttonHeight;
        float initialLogoWidth;
        Vector2 newAnchorMax, newAnchorMin, newOffsetMin, newOffsetMax;

        void OnEnable()
        {
            AddressablesManager.Instance.SpritesAdded += BuildLogoButtons;
            AddressablesManager.Instance.SpriteLoaded += LoadSingleLogo;
        }

        void OnDisable()
        {
            AddressablesManager.Instance.SpritesAdded -= BuildLogoButtons;
            AddressablesManager.Instance.SpriteLoaded -= LoadSingleLogo;
        }

        void Start()
        {
            if (BrandedLogoImage != null && BrandedLogoImage.GetComponent<RectTransform>() != null)
            {
                initialLogoWidth = BrandedLogoImage.GetComponent<RectTransform>().rect.width;
            }
            else
                initialLogoWidth = 88f;

            if (useOneLogoLabel)
                AddressablesManager.Instance.LoadOneLogoSprite();

            if (!string.IsNullOrEmpty(logoLabel))
                AddressablesManager.Instance.LoadSpritesWithLabel(logoLabel);
            else
                Debug.LogWarning("No Logo Label specified. No sprites will be loaded.");
        }

        void BuildLogoButtons()
        {
            if (LogoPanel != null && LogoButton != null)
            {
                buttonHeight = LogoButton.GetComponent<RectTransform>().anchorMax.y - LogoButton.GetComponent<RectTransform>().anchorMin.y;
                newOffsetMax = Vector2.zero;
                newOffsetMin = Vector2.zero;
                newAnchorMax = LogoButton.GetComponent<RectTransform>().anchorMax;
                newAnchorMin = LogoButton.GetComponent<RectTransform>().anchorMin;

                // Displaying the logo choices
                foreach (var _sprite in AddressablesManager.Instance.LoadedSprites)
                {
                    var newButton = NewButton();
                    var newButtonTransform = newButton.GetComponent<RectTransform>();
                    newButtonTransform.anchorMax = newAnchorMax;
                    newButtonTransform.anchorMin = newAnchorMin;
                    newButtonTransform.offsetMax = newOffsetMax;
                    newButtonTransform.offsetMin = newOffsetMin;
                    newButton.GetComponent<Image>().sprite = _sprite;
                    newButton.GetComponent<Button>().onClick.AddListener(() => LogoWasSelected(newButton));
                    newButton.gameObject.SetActive(true);

                    // Set up the next RectTransform
                    newAnchorMax = new Vector2(newAnchorMax.x, newAnchorMin.y - BUTTONSPACE);
                    newAnchorMin = new Vector2(newAnchorMin.x, newAnchorMax.y - buttonHeight);
                }
            }
        }

        void LoadSingleLogo(Sprite _sprite)
        {
            if (BrandedLogoImage != null && _sprite != null)
            {
                BrandedLogoImage.sprite = _sprite;
                ResizeLogo();
            }
        }

        GameObject NewButton()
        {
            return Instantiate(LogoButton.gameObject, LogoPanel);
        }

        // The logo was selected so replace and size it accordingly
        void LogoWasSelected(GameObject button)
        {
            if (BrandedLogoImage != null && button.GetComponent<Image>().sprite != null)
            {
                BrandedLogoImage.sprite = button.GetComponent<Image>().sprite;
                ResizeLogo();
            }
        }

        void ResizeLogo()
        {
            BrandedLogoImage.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            // If in landscape
            if (Screen.width > Screen.height)
            {
                var offset = (BrandedLogoImage.sprite.textureRect.width / BrandedLogoImage.sprite.textureRect.height * BrandedLogoImage.GetComponent<RectTransform>().rect.height) - BrandedLogoImage.GetComponent<RectTransform>().rect.width;
                // If there's a need to relocate image
                if (offset < -4)
                    BrandedLogoImage.GetComponent<RectTransform>().offsetMax = new Vector2(offset, BrandedLogoImage.GetComponent<RectTransform>().offsetMax.y);
            }
        }
    }
}