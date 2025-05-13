using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class FoodItem
{
    public string itemName;
    public GameObject prefab;
    public int carbonValue;
    public int calories;
    public int count;
}

public class NumberToIncrease : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] addButtons;
    public Button[] subtractButtons;
    public TextMeshProUGUI[] numberTexts;
    public TextMeshProUGUI[] carbonTexts;
    public TextMeshProUGUI[] caloriesTexts;

    public TextMeshProUGUI totalCarbonText;
    public TextMeshProUGUI warningText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI caloriesText;

    public ProgressBar progressBar;
    public ProgressBar progressBarRadial;
    public ProgressBar progressBarRadialCalories;

    [Header("Food Items")]
    public List<FoodItem> foodItems = new List<FoodItem>();
    public Transform basketTransform;

    private List<GameObject>[] spawnedItems;
    private List<GameObject> allSpawnedItems = new List<GameObject>();

    [Header("Environment Elements")]
    public GameObject ocean;
    private WaterMovement waterMovement;
    private Material oceanMaterial;

    private int carbonLimit = 36000;
    private int calorieTarget = 20726;

    void Start()
    {
        // Setup water system
        waterMovement = ocean.GetComponent<WaterMovement>();
        Renderer oceanRenderer = ocean.GetComponent<Renderer>();
        if (oceanRenderer != null)
            oceanMaterial = oceanRenderer.material;

        // Setup food tracking
        spawnedItems = new List<GameObject>[foodItems.Count];
        for (int i = 0; i < foodItems.Count; i++)
        {
            spawnedItems[i] = new List<GameObject>();
            int index = i;
            addButtons[i].onClick.AddListener(() => UpdateItem(index, +1));
            subtractButtons[i].onClick.AddListener(() => UpdateItem(index, -1));
        }

        UpdateAllDisplays();
    }

    private void UpdateItem(int index, int change)
    {
        FoodItem item = foodItems[index];
        item.count += change;
        if (item.count < 0)
        {
            item.count = 0;
            return;
        }

        UpdateUI(index);

        if (change > 0)
            AddFoodItem(index);
        else
            RemoveFoodItem(index);

        CalculateTotalCarbon();
        CalculateTotalCalories();
    }

    private void UpdateUI(int index)
    {
        FoodItem item = foodItems[index];
        numberTexts[index].text = item.count.ToString();
        carbonTexts[index].text = (item.count * item.carbonValue) + " gCO2e";
        caloriesTexts[index].text = (item.count * item.calories) + " kcal";
    }

    private void AddFoodItem(int index)
    {
        FoodItem item = foodItems[index];
        if (item.prefab != null)
        {
            GameObject obj = Instantiate(item.prefab, basketTransform);

            int totalCount = allSpawnedItems.Count;
            int row = totalCount / 10;
            int col = totalCount % 10;
            float xOffset = col * 0.2f;
            float yOffset = -row * 0.2f;

            obj.transform.localPosition = new Vector3(xOffset, yOffset, 0);

            allSpawnedItems.Add(obj);
            spawnedItems[index].Add(obj);
        }
    }

    private void RemoveFoodItem(int index)
    {
        if (spawnedItems[index].Count > 0)
        {
            GameObject last = spawnedItems[index][spawnedItems[index].Count - 1];
            spawnedItems[index].RemoveAt(spawnedItems[index].Count - 1);
            allSpawnedItems.Remove(last);
            Destroy(last);
            RepositionItems();
        }
    }

    private void RepositionItems()
    {
        for (int i = 0; i < allSpawnedItems.Count; i++)
        {
            int row = i / 10;
            int col = i % 10;
            float xOffset = col * 0.2f;
            float yOffset = -row * 0.2f;

            allSpawnedItems[i].transform.localPosition = new Vector3(xOffset, yOffset, 0);
        }
    }

    private void CalculateTotalCarbon()
    {
        int totalCarbon = 0;
        foreach (var item in foodItems)
            totalCarbon += item.count * item.carbonValue;

        totalCarbonText.text = "Total: " + totalCarbon + " gCO2e";
        warningText.text = totalCarbon >= carbonLimit ? "Carbon limit reached!" : "";

        if (progressBar != null)
        {
            progressBar.current = totalCarbon;
            progressBar.GetCurrentFill();
        }
        if (progressBarRadial != null)
        {
            progressBarRadial.current = totalCarbon;
            progressBarRadial.GetCurrentFill();
        }
    }

    private void CalculateTotalCalories()
    {
        int totalCalories = 0;
        foreach (var item in foodItems)
            totalCalories += item.count * item.calories;

        caloriesText.text = "Calories: " + totalCalories + " kcal";

        if (progressBarRadialCalories != null)
        {
            progressBarRadialCalories.current = totalCalories;
            progressBarRadialCalories.GetCurrentFill();
        }
    }

    private void UpdateAllDisplays()
    {
        for (int i = 0; i < foodItems.Count; i++)
        {
            UpdateUI(i);
        }

        CalculateTotalCarbon();
        CalculateTotalCalories();
    }
}
