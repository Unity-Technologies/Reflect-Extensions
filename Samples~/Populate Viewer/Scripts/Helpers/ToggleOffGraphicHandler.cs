using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleOffGraphicHandler : MonoBehaviour
{
    [SerializeField] Graphic offGraphic = default;

    private void Awake()
    {
        if (offGraphic)
            GetComponent<Toggle>().onValueChanged.AddListener((x) => offGraphic.enabled = !x);
    }

    [ContextMenu("Set State")]
    void SetState()
    {
        if (offGraphic)
            offGraphic.enabled = !GetComponent<Toggle>().isOn;
    }
}