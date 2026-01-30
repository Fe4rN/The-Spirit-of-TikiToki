using System;
using UnityEngine;

public class ScriptVida : MonoBehaviour
{

    public int maxVida = 5;
    public int vida;
    public SliderVida slidervida;
    public Boolean damage = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vida = maxVida;
        slidervida.SetVidaMax(maxVida);
    }

    // Update is called once per frame
    void Update()
    {
        if(damage)
        {
            reducirSalud();
        }
    }

    public void reducirSalud()
    {
        vida--;
        slidervida.SetSlider(vida);
    }
}
