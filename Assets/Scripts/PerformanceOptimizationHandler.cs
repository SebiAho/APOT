using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceOptimizationHandler : MonoBehaviour
{
    public PerformanceDataTracker dataTracker;
    public GraphicsSettings graphics;

    [Tooltip("If true the handler will add the ingame settings to the list to the game")]
    bool useGameSettings = true;
    public List<SValue> settingList = new List<SValue>();
    [Tooltip("If the the combined priorities are the same, sort based on the favored impact type")]
    public bool favorGraphics = false;

    private void Awake()
    {
        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (useGameSettings)
        {
            TempSetPriorities();
            AddSettings();
        }
        SortSettings();
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
        for (int i = 0; i < settingList.Count; i++)
            settingList[i].combinedImpact = settingList[i].pImpact - settingList[i].gImpact;

        //Sort the list in order based on priority so that the values with the highest priorities are first
        settingList.Sort(delegate(SValue p_x, SValue p_y)
        {
            int t_xImpact = p_x.pImpact - p_x.gImpact;
            int t_yImpact = p_y.pImpact - p_y.gImpact;

            if (p_x.combinedImpact > p_y.combinedImpact)
                return -1;
            else if (p_x.combinedImpact < p_y.combinedImpact)
                return 1;
            else if(t_xImpact == t_yImpact)
            {
                //If the combined priorities are the same, sort based on the favored impact type
                if(favorGraphics)
                {
                    if (p_x.gImpact > p_y.gImpact)
                        return -1;
                    else
                        return 1;
                }
                else
                {
                    if (p_x.pImpact > p_y.pImpact)
                        return -1;
                    else
                        return 1;
                }
            }    
            return 0;
        });
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
        graphics.settingValues.textureQuality.gImpact = 5;

        //antialiazing method
        graphics.settingValues.aaMethod.pImpact = 4;
        graphics.settingValues.aaMethod.gImpact = 4;

        //antialiazing quality
        graphics.settingValues.aaQuality.pImpact = 4;
        graphics.settingValues.aaQuality.gImpact = 4;

        //shadow quality
        graphics.settingValues.shadowQuality.pImpact = 4;
        graphics.settingValues.shadowQuality.gImpact = 3;

        //shadow distance
        graphics.settingValues.shadowDistance.pImpact = 5;
        graphics.settingValues.shadowDistance.gImpact = 3;
    }
}
