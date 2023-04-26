using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceOptimizationHandler : MonoBehaviour
{
    public PerformanceDataTracker dataTracker;
    public GraphicsSettings graphics;

    public List<SValue> settingList = new List<SValue>();

    private void Awake()
    {
        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();
    }

    // Start is called before the first frame update
    void Start()
    {
        AddSettings();
        TempSetPriorities();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AddSettings()
    {
        settingList.Clear();

        //Add settings
        settingList.Add(graphics.settingValues.fullScreen);
        settingList.Add(graphics.settingValues.vSynch);
        settingList.Add(graphics.settingValues.resolutionIndex);
        settingList.Add(graphics.settingValues.textureQuality);
        settingList.Add(graphics.settingValues.aaMethod);
        settingList.Add(graphics.settingValues.aaQuality);
        settingList.Add(graphics.settingValues.shadowQuality);
        settingList.Add(graphics.settingValues.shadowDistance);
    }

    void SortSettings()
    {
        
    }

    void SelectSettings()
    {

    }

    void ApplySettings()
    {

    }

    void TempSetPriorities()
    {
        //fullscreen
        graphics.settingValues.fullScreen.use = false;

        //v-synch
        graphics.settingValues.vSynch.pImpact = 1;
        graphics.settingValues.vSynch.gImpact = 2;

        //resolution
        graphics.settingValues.resolutionIndex.pImpact = 2;
        graphics.settingValues.resolutionIndex.gImpact = 5;

        //texture quality
        graphics.settingValues.textureQuality.pImpact = 3;
        graphics.settingValues.textureQuality.pImpact = 5;

        //antialiazing method
        graphics.settingValues.aaMethod.pImpact = 4;
        graphics.settingValues.aaMethod.pImpact = 4;

        //antialiazing quality
        graphics.settingValues.aaQuality.pImpact = 4;
        graphics.settingValues.aaQuality.pImpact = 4;

        //shadow quality
        graphics.settingValues.shadowQuality.pImpact = 4;
        graphics.settingValues.shadowQuality.pImpact = 3;

        //shadow distance
        graphics.settingValues.shadowDistance.pImpact = 5;
        graphics.settingValues.shadowDistance.pImpact = 3;
    }
}
