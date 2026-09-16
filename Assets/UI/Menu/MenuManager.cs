using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject velvet;
    public GameObject config;
    public GameObject gachaMenu; // Referencia al menú/pantalla del Gacha

    public enum Menu
    {
        Main,
        Velvet,
        Config,
        Gacha
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
        gachaMenu.SetActive(menu == Menu.Gacha);
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

    public void IrAGacha()
    {
        CambiarMenu(Menu.Gacha);
    }
}