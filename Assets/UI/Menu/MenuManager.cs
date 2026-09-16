using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MenuManager : MonoBehaviour
{

    public GameObject mainMenu;
    public GameObject velvet;
    public GameObject config;

    public enum Menu
    {
        Main,
        Velvet,
        Config
    } 
    public Menu currentMenu;

    void Start()
    {
        CambiarMenu(Menu.Main);
    }
    public void CambiarMenu(Menu menu)
    {
        currentMenu = menu;

        mainMenu.SetActive(menu == Menu.Main);
        velvet.SetActive(menu == Menu.Velvet);
        config.SetActive(menu == Menu.Config);
    }
    public void IrAMain()
    {
        CambiarMenu(Menu.Main);
    }

    public void IrAVelvet()
    {
        CambiarMenu(Menu.Velvet);
    }

    public void IrAConfig()
    {
        CambiarMenu(Menu.Config);
    }
}
