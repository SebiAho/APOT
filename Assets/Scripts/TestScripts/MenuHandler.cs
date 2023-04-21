using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    //MAKE SURE THAT THERE IS AN ACTIVE EVENT HANDLER AT THE SCENE!!!
    
    public GameObject currentMenu;

    // Start is called before the first frame update
    void Awake()
    {
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

}
