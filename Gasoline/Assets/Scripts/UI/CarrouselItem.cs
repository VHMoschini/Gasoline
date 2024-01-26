using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarrouselItem : MonoBehaviour
{

    #region [ VARIABLES ]

    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemPrice;
    [SerializeField] private Image itemBaseImage;
    [SerializeField] private Image itemAuxiliarImage;

    internal Item itemConfig;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetScale;

    private RectTransform localRectTransform;

    #endregion

    #region [ MESSAGES ]

    private void Start()
    {
        localRectTransform = GetComponent<RectTransform>();

        targetPosition = transform.position;
        targetRotation = transform.rotation;
        targetScale = transform.localScale;
    }

    private void Update()
    {

        localRectTransform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10);
        localRectTransform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * 10);
    }

    #endregion

    #region [ METHODS ]

    internal void initializeItem(Item config)
    {
        itemConfig = config;

        if (config.data.isColor)
            itemBaseImage.color = config.data.color;



    }

    internal void manipulateItem(RectTransform _transform, bool _interpolation = true)
    {
        targetPosition = _transform.position;
        localRectTransform.rotation = _transform.rotation;
        targetScale = _transform.localScale;

        if (_interpolation)
            return;

        localRectTransform.position = _transform.position;
        localRectTransform.rotation = _transform.rotation;
        localRectTransform.localScale = _transform.localScale;
    }

    internal void ChooseItem()
    {
        itemPrice.text = "Acquired";
    }

    #endregion
}
