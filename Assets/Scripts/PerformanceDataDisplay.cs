using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PerformanceDataDisplay : MonoBehaviour
{
    public PerformanceDataTracker dataTracker;
    public TextMeshProUGUI valuesText;

    [SerializeField]
    private PerformanceDataContainer data;

    string dataValues;

    private void Awake()
    {
        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();

        if (valuesText == null)
            valuesText = GetComponent<TextMeshProUGUI>();

        //Get the performance data from PerfromanceDataTracker
        data = dataTracker.data;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ConvertDataValuesToString();
        valuesText.text = dataValues;
    }

    void ConvertDataValuesToString()
    {
        dataValues = "\n" +
            data.currentFrameRate.ToString() + "\n" +
            data.averageFrameRate.ToString() + "\n" +
            data.lowestFrameRate.ToString() + "\n" +
            data.highestFrameRate.ToString() + "\n" +
            data.lowestAverageFrameRate.ToString() + "\n" +
            data.highestAverageFrameRate.ToString();
    }
}
