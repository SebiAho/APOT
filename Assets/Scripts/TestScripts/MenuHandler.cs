using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    //MAKE SURE THAT THERE IS AN ACTIVE EVENT HANDLER AT THE SCENE!!!
    
    public GameObject currentMenu;

    [Header("Graphics Menu")]
    [Tooltip("Leave this empty if graphics settings arent used")]
    public GraphicsSettings graphics;
    public Toggle fullscreen;
    public Slider vSynch;
    public TMPro.TMP_Dropdown resolution;

    // Start is called before the first frame update
    void Awake()
    {
    }

    private void Start()
    {
        //Graphics settings
        if (graphics != null)
        {
            fullscreen.isOn = Screen.fullScreen;
            vSynch.value = QualitySettings.vSyncCount;

            //Add resolution options and set the current resolution index
            List<string> t_resOptions = new List<string>();
            int t_resIndex = 0;

            resolution.options.Clear();
            for(int i = 0; i < graphics.resolutions.Length; i++)
            {
                string t_resOption = graphics.resolutions[i].width + " x " + graphics.resolutions[i].height;
                t_resOptions.Add(t_resOption);
                if (graphics.resolutions[i].width == Screen.width && graphics.resolutions[i].height == Screen.height)
                    t_resIndex = i;
            }
            resolution.AddOptions(t_resOptions);
            resolution.value = t_resIndex;
            resolution.RefreshShownValue();

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Change menu
    public void ButtonChangeMenu(GameObject p_menu)
    {
        p_menu.SetActive(true);

        currentMenu.SetActive(false);
        currentMenu = p_menu;
    }

    public void ButtonLoadScene(int p_index)
    {
        SceneManager.LoadScene(p_index);
    }

    public void ButtonLoadScene(string p_sceneName)
    {
        SceneManager.LoadScene(p_sceneName);
    }

    //Set object inactive if it is active and active if it isin't
    public void ButtonActiveObject(GameObject p_object)
    {
        if (p_object.activeSelf)
            p_object.SetActive(false);
        else
            p_object.SetActive(true);
    }

    //Graphics Settings
}
