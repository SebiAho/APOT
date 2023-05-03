using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Container for perfroamance data
[System.Serializable]
public class PerformanceDataContainer
{
    [Header("Frame Rate Data")]
    public float currentFrameRate;
    public float averageFrameRate;
    public float lowestFrameRate;
    public float highestFrameRate;
    public float lowestAverageFrameRate;
    public float highestAverageFrameRate;
}

public class PerformanceDataTracker : MonoBehaviour
{
    //Tacking Delay
    [Tooltip("Time to the start of tracking is delayed(in seconds), set to 0 to disable, this should help to avoid any potential startup lag of the program from messing the results")]
    [SerializeField]
    float delayDataTracking = 0f;
    public bool startTracking { get; private set; } = true;

     //Average fps calculation
    [Tooltip("Time frame the average fps is calculated, note that until the time frame has passed at least once, the averageFrameRate value will be 0")]
    public float averageFPSTimeFrame = 1f;
    float averageSum = 0f, elapsedAverageTime = 0f, averageFPS = 0f;
    int averageIndex = 0;
    public bool firstAverageCalculated { get; private set; } = false;//Has the average fps been calculated at least once

    public PerformanceDataContainer data; //Note: In a larger project it might be preverable to only allow this to be passed to others scripts as read only to prevent other scripts from modifying it's values.

    private void Awake()
    {
        //delayTracking
        if (delayDataTracking > 0)
            startTracking = false;

        //Set the lowest and lowest frame rate values to the highest possiple integer values to allow the system to properly calculate them
        data.lowestFrameRate = int.MaxValue;
        data.lowestAverageFrameRate = int.MaxValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (startTracking)
        {
            //FPS
            data.currentFrameRate = trackFPS();
            data.averageFrameRate = CalculateAverageFPS();
            HighestAndLowestFrameRateValues();
        }
        else
        {
            //Subtract deltaTime from delayDataTrackingTime
            if (delayDataTracking > 0)
                delayDataTracking -= Time.unscaledDeltaTime;
            else
                startTracking = true;
        }
    }

    private float trackFPS()
    {
        return 1f / Time.unscaledDeltaTime;
    }

    //Caluclate the average fps of a set time frame
    private float CalculateAverageFPS()
    {
        if(elapsedAverageTime < averageFPSTimeFrame)
        {
            elapsedAverageTime += Time.unscaledDeltaTime;
            averageSum += trackFPS();
            averageIndex++;
            
        }
        else if(elapsedAverageTime >= averageFPSTimeFrame)
        {
            //Calculate the average fps, while making sure theres no attempt to divide  by 0;
            if (averageIndex == 0)
                averageIndex = 0;
            else
                averageFPS = averageSum / averageIndex;

            firstAverageCalculated = true;

            elapsedAverageTime = 0f;
            averageSum = 0;
            averageIndex = 0;
        }
        return averageFPS;
    }

    private void HighestAndLowestFrameRateValues()
    {
        //Lowest frame rate value
        if (data.lowestFrameRate > data.currentFrameRate)
            data.lowestFrameRate = data.currentFrameRate;

        //Highest frame rate value
        if (data.highestFrameRate < data.currentFrameRate)
            data.highestFrameRate = data.currentFrameRate;

        if (firstAverageCalculated)
        {
            //Lowest frame rate value
            if (data.lowestAverageFrameRate > data.averageFrameRate)
                data.lowestAverageFrameRate = data.averageFrameRate;

            //Highest frame rate value
            if (data.highestAverageFrameRate < data.averageFrameRate)
                data.highestAverageFrameRate = data.averageFrameRate;
        }
    }

    public void StoreData()
    {
        PerfromanceData.currentFrameRate = data.currentFrameRate;
        PerfromanceData.averageFrameRate = data.averageFrameRate;
        PerfromanceData.lowestFrameRate = data.lowestFrameRate;
        PerfromanceData.highestFrameRate = data.highestFrameRate;
        PerfromanceData.lowestAverageFrameRate = data.lowestAverageFrameRate;
        PerfromanceData.highestAverageFrameRate = data.highestAverageFrameRate;
    }
}
