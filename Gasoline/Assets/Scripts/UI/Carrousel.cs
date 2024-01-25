using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Carrousel : MonoBehaviour
{
    #region [ VARIABLES ]

    [SerializeField] private CarrouselComponent carrouselComponent;
    [SerializeField] private Button ctaButton;
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private GameObject carrouselObject;

    private Action<Item> calllbak;

    #endregion

    #region [ MESSAGES ]

    private void OnEnable()
    {
        Shop.OpenStoreEvent.AddListener(OpenCarrousel);
        Closet.OpenStoreEvent.AddListener(OpenCarrousel);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCarrousel();
        }
    }

    #endregion

    #region [ METHODS ]

    private void OpenCarrousel(StoreInteractionConfig config)
    {
        calllbak = config.calllback;
        carrouselComponent.items = config.items;
        header.text = config.header;
        ctaButton.GetComponentInChildren<TextMeshProUGUI>().text = config.ctaText;

        carrouselObject.SetActive(true);
        ctaButton.onClick.AddListener(CTA);
    }

    private void CloseCarrousel()
    {
        ctaButton.onClick.RemoveAllListeners();
        carrouselObject.SetActive(false);

        Player.MobilityChange.Invoke(arg0: true);
    }

    public void CTA()
    {
        Item item = carrouselComponent.GetSelectedItem();
        calllbak.Invoke(item);
    }

    #endregion
}
