using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class StellarSearchClickable : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ObjectManager objectManager;
    public void OnPointerClick(PointerEventData eventData)
    {
        var text = GetComponent<TextMeshProUGUI>();
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
            if (linkIndex > -1)
            {
                var linkInfo = text.textInfo.linkInfo[linkIndex];
                var linkId = linkInfo.GetLinkID();

                var itemData = objectManager.openClBodies.myObjectBodies.Find(obj => obj.name == linkId);
                if (itemData != null)
                {
                    Vector3 directionToCamera = (Camera.main.transform.position - itemData.position).normalized;
                    Vector3 newCameraPosition = itemData.position + directionToCamera * ViewHelper.CalculateOffset(itemData.mass);

                    Camera.main.transform.position = newCameraPosition;
                    Camera.main.transform.LookAt(itemData.position);
                }
            }
        }
    }
}
