using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFillStep : MonoBehaviour
{
    public float step;

    Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (step == 0f)
        {
            return;
        }

        float target = 0f;
        for (float f = 0f; f <= 1f; f += step)
        {
            if (f < image.fillAmount)
            {
                target = f;
                continue;
            }

            break;
        }

        image.fillAmount = target;
    }
}
