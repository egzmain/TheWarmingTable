using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles virtual shopping logic: item tracking, carbon & calorie calculations, UI updates, and 3D instantiations.
/// </summary>
public class NumberToIncrease : MonoBehaviour
{
    [System.Serializable]
    public class FoodItem
    {
        public string itemName;              // Name of the food item
        public GameObject prefab;            // Prefab to instantiate in the basket
        public int carbonValue;              // Carbon footprint per unit (gCO2e)
        public int calories;                 // Calories per unit
        [HideInInspector] public int count;  // Number of items added (internal use)
    }

    [Header("Food Data")]
    public List<FoodItem> foodItems; // List of all food items to display and track

    [Header("UI Elements")]
    public Button[] addButtons;           // "+" buttons for each item
    public Button[] subtractButtons;      // "-" buttons for each item
    public TextMeshProUGUI[] numberTexts; // Displays item count
    public TextMeshProUGUI[] carbonTexts; // Displays carbon value for each item
    public TextMeshProUGUI[] caloriesTexts; // Displays calories for each item

    [Header("Progress Bars")]
    public ProgressBar progressBar;           // Linear bar for carbon tracking
    public ProgressBar progressBarRadial;     // Radial bar for carbon
    public ProgressBar progressBarRadialCalories; // Radial bar for calorie tracking

    [Header("Total Display")]
    public TextMeshProUGUI totalCarbonText;
    public TextMeshProUGUI warningText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI caloriesText;

    [Header("3D Basket Display")]
    public Transform basketTransform;         // Parent transform where 3D food models are spawned
    private List<GameObject> allSpawnedItems = new List<GameObject>(); // Track all spawned models
    private List<GameObject>[] spawnedItems;  // Track spawned items by type

    [Header("Environment Feedback")]
    public GameObject ocean;                  // Ocean GameObject to reflect environmental impact
    private Material oceanMaterial;
    private WaterMovement waterMovement;      // Script controlling water movement visuals

    [Header("Thresholds")]
    public int carbonLimit = 36000;           // Max carbon budget
    public int caloriesIntakeRequired = 20726;// Suggested daily calorie intake

    void Start()
    {
        // Initialize spawn tracking for each food item
        spawnedItems = new List<GameObject>[foodItems.Count];
        for (int i = 0; i < foodItems.Count; i++)
        {
            spawnedItems[i] = new List<GameObject>();

            int index = i; // Capture loop variable for closure
            addButtons[i].onClick.AddListener(() => UpdateItem(index, 1));
            subtractButtons[i].onClick.AddListener(() => UpdateItem(index, -1));
        }

        // Get material for visual feedback (ocean rising, etc.)
        if (ocean != null)
        {
            oceanMaterial = ocean.GetComponent<Renderer>().material;
            waterMovement = ocean.GetComponent<WaterMovement>();
        }

        // Initialize UI with current (zeroed) data
        UpdateAllDisplays();
    }

    /// <summary>
    /// Handles adding/removing item and updating everything accordingly.
    /// </summary>
    void UpdateItem(int index, int change)
    {
        FoodItem item = foodItems[index];
        item.count += change;

        // Prevent negative counts
        if (item.count < 0)
        {
            item.count = 0;
            return;
        }

        // Update individual UI displays
        numberTexts[index].text = item.count.ToString();
        carbonTexts[index].text = (item.count * item.carbonValue) + " gCO2e";
        caloriesTexts[index].text = (item.count * item.calories) + " kcal";

        // Add or remove 3D object in basket
        if (change > 0)
            AddFoodItem(index);
        else
            RemoveFoodItem(index);

        // Update totals
        CalculateTotalCarbon();
        CalculateTotalCalories();
    }

    /// <summary>
    /// Adds food prefab to basket with appropriate position.
    /// </summary>
    void AddFoodItem(int index)
    {
        FoodItem item = foodItems[index];

        if (item.prefab != null)
        {
            GameObject newFood = Instantiate(item.prefab, basketTransform);

            // Position based on total number of items
            int total = allSpawnedItems.Count;
            int row = total / 10;
            int col = total % 10;

            float xOffset = col * 0.2f;
            float yOffset = -row * 0.2f;

            // Optional: slight randomness for realism
            float randomOffset = UnityEngine.Random.Range(-0.05f, 0.05f);

            newFood.transform.localPosition = new Vector3(xOffset + randomOffset, yOffset, 0);

            // Track item
            allSpawnedItems.Add(newFood);
            spawnedItems[index].Add(newFood);
        }
    }

    /// <summary>
    /// Removes last-added instance of this food type from the basket.
    /// </summary>
    void RemoveFoodItem(int index)
    {
        if (spawnedItems[index].Count > 0)
        {
            GameObject last = spawnedItems[index][spawnedItems[index].Count - 1];

            spawnedItems[index].RemoveAt(spawnedItems[index].Count - 1);
            allSpawnedItems.Remove(last);
            Destroy(last);

            // Re-layout items
            RepositionItems();
        }
    }

    /// <summary>
    /// Repositions all basket items after one is removed.
    /// </summary>
    void RepositionItems()
    {
        for (int i = 0; i < allSpawnedItems.Count; i++)
        {
            GameObject obj = allSpawnedItems[i];
            int row = i / 10;
            int col = i % 10;

            float xOffset = col * 0.2f;
            float yOffset = -row * 0.2f;
            float randomOffset = UnityEngine.Random.Range(-0.05f, 0.05f);

            obj.transform.localPosition = new Vector3(xOffset + randomOffset, yOffset, 0);
        }
    }

    /// <summary>
    /// Calculates and updates total carbon footprint.
    /// </summary>
    void CalculateTotalCarbon()
    {
        int totalCarbon = 0;

        foreach (var item in foodItems)
        {
            totalCarbon += item.count * item.carbonValue;
        }

        totalCarbonText.text = "Total: " + totalCarbon + " gCO2e";

        // Show warning if over limit
        warningText.text = totalCarbon >= carbonLimit ? "Carbon limit reached" : "";

        // Update progress bars
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

        // Optional: update environment visuals
        if (waterMovement != null)
        {
            waterMovement.UpdateWaterLevel(totalCarbon);
        }
    }

    /// <summary>
    /// Calculates and updates total calories consumed.
    /// </summary>
    void CalculateTotalCalories()
    {
        int totalCalories = 0;

        foreach (var item in foodItems)
        {
            totalCalories += item.count * item.calories;
        }

        caloriesText.text = "Calories: " + totalCalories + " kcal";

        if (progressBarRadialCalories != null)
        {
            progressBarRadialCalories.current = totalCalories;
            progressBarRadialCalories.GetCurrentFill();
        }
    }

    /// <summary>
    /// Updates the entire UI display on scene start or refresh.
    /// </summary>
    void UpdateAllDisplays()
    {
        for (int i = 0; i < foodItems.Count; i++)
        {
            FoodItem item = foodItems[i];
            numberTexts[i].text = item.count.ToString();
            carbonTexts[i].text = (item.count * item.carbonValue) + " gCO2e";
            caloriesTexts[i].text = (item.count * item.calories) + " kcal";
        }

        CalculateTotalCarbon();
        CalculateTotalCalories();
    }
}
