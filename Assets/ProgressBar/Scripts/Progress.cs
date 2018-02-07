using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Progress : MonoBehaviour {

    Image foregroundImage;

    //[SerializeField]
    //private int value;

    public float Value
    {
        get
        {
            if (foregroundImage != null)
                return (foregroundImage.fillAmount * 100);
            else
                return 0;
        }
        set
        {
            if (foregroundImage != null)
                foregroundImage.fillAmount = value / 100f;
        }
    }

    //void OnValidate()
    //{
    //    Value = value;
    //}

    void Start()
    {
        foregroundImage = gameObject.GetComponent<Image>();
        Value = 0;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
