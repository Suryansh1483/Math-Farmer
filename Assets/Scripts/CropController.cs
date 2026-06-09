using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CropController : MonoBehaviour
{
    public enum CropState { Empty, Planted, Watered, Grown }
    public CropState currentState = CropState.Empty;

    [Header("Sprites")]
    public Image cropImage;
    public Sprite emptySprite;
    public Sprite plantedSprite;
    public Sprite wateredSprite;
    public Sprite grownSprite;

    [Header("Growth Settings")]
    public float growthTime = 30f;

    [Header("Visual Feedback")]
    public GameObject selectionIndicator;

    [Header("Growth Bar")]
    public GameObject growthBar;
    public Image growthFill;
    public TextMeshProUGUI growthText;

    private Coroutine growthCoroutine;
    private float growthProgress = 0f;

    private void Start()
    {
        UpdateSprite();

        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);

        UpdateGrowthUI();
    }

    // ---------------------------
    // Soil Plot Click
    // ---------------------------
    public void OnSoilClick()
    {
        Debug.Log("Soil plot clicked: " + gameObject.name);
        GameManager.Instance.SelectSoilPlot(this);
        ShowSelectionIndicator();
    }

    private void ShowSelectionIndicator()
    {
        CropController[] allPlots = FindObjectsOfType<CropController>();

        foreach (CropController plot in allPlots)
        {
            if (plot.selectionIndicator != null)
                plot.selectionIndicator.SetActive(false);
        }

        if (selectionIndicator != null)
            selectionIndicator.SetActive(true);
    }

    // ---------------------------
    // Gameplay Actions
    // ---------------------------
    public void PlantCrop()
    {
        if (currentState == CropState.Empty)
        {
            currentState = CropState.Planted;
            UpdateSprite();
            UpdateGrowthUI();

            Debug.Log($"{gameObject.name}: Crop planted!");
        }
        else
        {
            GameManager.Instance.messageManager?.ShowMessage("This plot is already in use!");
        }
    }

    public void WaterCrop()
    {
        if (currentState == CropState.Planted)
        {
            currentState = CropState.Watered;
            growthProgress = 0f;

            UpdateSprite();
            UpdateGrowthUI();

            if (growthCoroutine != null)
                StopCoroutine(growthCoroutine);

            growthCoroutine = StartCoroutine(GrowCrop());

            Debug.Log($"{gameObject.name}: Crop watered! Growing...");
        }
        else if (currentState == CropState.Empty)
        {
            GameManager.Instance.messageManager?.ShowMessage("Plant a seed first!");
        }
        else if (currentState == CropState.Watered)
        {
            GameManager.Instance.messageManager?.ShowMessage("Already watered!");
        }
        else if (currentState == CropState.Grown)
        {
            GameManager.Instance.messageManager?.ShowMessage("Ready to harvest!");
        }
    }

    public void HarvestCrop()
    {
        if (currentState == CropState.Grown)
        {
            if (growthCoroutine != null)
            {
                StopCoroutine(growthCoroutine);
                growthCoroutine = null;
            }

            currentState = CropState.Empty;
            growthProgress = 0f;

            UpdateSprite();
            UpdateGrowthUI();

            Debug.Log($"{gameObject.name}: Crop harvested!");
        }
        else if (currentState == CropState.Empty)
        {
            GameManager.Instance.messageManager?.ShowMessage("Nothing to harvest!");
        }
        else if (currentState == CropState.Planted)
        {
            GameManager.Instance.messageManager?.ShowMessage("Needs water first!");
        }
        else if (currentState == CropState.Watered)
        {
            GameManager.Instance.messageManager?.ShowMessage("Still growing...");
        }
    }

    // ---------------------------
    // Growth Coroutine
    // ---------------------------
    private IEnumerator GrowCrop()
    {
        // IMPORTANT:
        // We DO NOT reset growthProgress here.
        // This fixes the save/load bug.

        while (growthProgress < 1f && currentState == CropState.Watered)
        {
            growthProgress += Time.deltaTime / growthTime;

            if (cropImage != null)
            {
                Color tint = Color.Lerp(Color.white, Color.green, growthProgress * 0.3f);
                cropImage.color = tint;
            }

            UpdateGrowthUI();

            yield return null;
        }

        if (currentState == CropState.Watered)
        {
            growthProgress = 1f;
            currentState = CropState.Grown;

            UpdateSprite();
            UpdateGrowthUI();

            if (cropImage != null)
                cropImage.color = Color.white;

            GameManager.Instance.messageManager?.ShowMessage($"{gameObject.name} is ready to harvest!");

            Debug.Log($"{gameObject.name}: Crop fully grown!");
        }

        growthCoroutine = null;
    }

    // ---------------------------
    // Growth Bar UI
    // ---------------------------
    private void UpdateGrowthUI()
    {
        if (growthBar == null)
            return;

        if (currentState == CropState.Empty)
        {
            growthBar.SetActive(false);
            return;
        }

        growthBar.SetActive(true);

        float progress = 0f;

        switch (currentState)
        {
            case CropState.Planted:
                progress = 0f;
                break;

            case CropState.Watered:
                progress = growthProgress;
                break;

            case CropState.Grown:
                progress = 1f;
                break;
        }

        if (growthFill != null)
            growthFill.fillAmount = progress;

        if (growthText != null)
        {
            if (currentState == CropState.Grown)
            {
                growthText.text = "READY!";
            }
            else
            {
                int percent = Mathf.Clamp(
                    Mathf.FloorToInt(progress * 100f),
                    0,
                    99
                );

                growthText.text = percent + "%";
            }
        }
    }

    // ---------------------------
    // Sprite Updates
    // ---------------------------
    private void UpdateSprite()
    {
        if (cropImage == null)
            return;

        switch (currentState)
        {
            case CropState.Empty:
                cropImage.sprite = emptySprite;
                cropImage.color = Color.white;
                break;

            case CropState.Planted:
                cropImage.sprite = plantedSprite;
                cropImage.color = Color.white;
                break;

            case CropState.Watered:
                cropImage.sprite = wateredSprite;
                cropImage.color = Color.white;
                break;

            case CropState.Grown:
                cropImage.sprite = grownSprite;
                cropImage.color = Color.white;
                break;
        }

        UpdateGrowthUI();
    }

    // ---------------------------
    // Public Getters
    // ---------------------------
    public bool CanPlant() => currentState == CropState.Empty;
    public bool CanWater() => currentState == CropState.Planted;
    public bool CanHarvest() => currentState == CropState.Grown;

    public float GetGrowthProgress() => growthProgress;

    public string GetStateString()
    {
        return currentState switch
        {
            CropState.Empty => "Empty",
            CropState.Planted => "Planted",
            CropState.Watered => $"Growing ({(growthProgress * 100):F0}%)",
            CropState.Grown => "Ready!",
            _ => "Unknown"
        };
    }

    // ---------------------------
    // Reset Support
    // ---------------------------
    public void LoadEmpty()
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }

        currentState = CropState.Empty;
        growthProgress = 0f;

        if (cropImage != null)
        {
            cropImage.sprite = emptySprite;
            cropImage.color = Color.white;
        }

        if (selectionIndicator != null)
            selectionIndicator.SetActive(false);

        UpdateGrowthUI();

        Debug.Log($"{gameObject.name}: Reset to empty plot.");
    }

    // ---------------------------
    // Save Data
    // ---------------------------
    [System.Serializable]
    public class CropData
    {
        public CropState state;
        public float progress;
    }

    public CropData GetSaveData()
    {
        return new CropData
        {
            state = currentState,
            progress = growthProgress
        };
    }

    public void LoadSaveData(CropData data)
    {
        currentState = data.state;
        growthProgress = data.progress;

        UpdateSprite();
        UpdateGrowthUI();

        if (currentState == CropState.Watered && growthProgress < 1f)
        {
            if (growthCoroutine != null)
                StopCoroutine(growthCoroutine);

            growthCoroutine = StartCoroutine(GrowCrop());
        }
    }
}