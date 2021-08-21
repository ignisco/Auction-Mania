using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{
    // Start is called before the first frame update

    private Text text;
    void Start()
    {
        text = GetComponentInChildren<Text>();
        GetComponent<Slider>().onValueChanged.AddListener(sliderValueChanged);
    }

    // Update is called once per frame
    void sliderValueChanged(float value)
    {
        text.text = "Number of Players: " + value;
    }
}
