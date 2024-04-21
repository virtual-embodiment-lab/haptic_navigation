using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterFog : MonoBehaviour
{
    private Color underwaterColor;

    // Start is called before the first frame update
    void Start()
    {
        underwaterColor = new Color(0.22f, 0.65f, 0.77f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        RenderSettings.fogColor = underwaterColor;
        RenderSettings.fogDensity = 0.2f;
    }
}
