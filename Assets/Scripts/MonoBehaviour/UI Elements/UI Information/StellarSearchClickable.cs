using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StellarSearchClickable : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ObjectManager objectManager;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        var text = GetComponent<TextMeshProUGUI>();
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (linkIndex == -1) return;

        var linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();
        var itemData = objectManager.celestialBodyManager.myObjectBodies.Find(obj => obj.name == linkId);
        if (itemData == null) return;

        Vector3 directionToCamera = (Camera.main.transform.position - itemData.position).normalized;
        Camera.main.transform.position = itemData.position + directionToCamera * ViewHelper.CalculateOffset(itemData.mass);
        Camera.main.transform.LookAt(itemData.position);
    }
}
