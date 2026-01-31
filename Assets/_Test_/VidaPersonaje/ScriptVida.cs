using System;
using UnityEngine;

public class ScriptVida : MonoBehaviour
{

    public int maxVida = 5;
    public int vida;
    public SliderVida sliderVida;
    public Boolean damage = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vida = maxVida;
        sliderVida.SetVidaMax(maxVida);
    }

    // Update is called once per frame
    void Update()
    {
        if(damage) //Al recibir daño
        {
            reducirSalud();
        }
    }

    public void reducirSalud() //Reduce la salud y la barra
    {
        vida--;
        sliderVida.SetSlider(vida);
    }
}
