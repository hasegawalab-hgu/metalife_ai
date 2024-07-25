using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextBackBround : MonoBehaviour
{
    RectTransform parentRT;
    RectTransform rt;

    // Start is called before the first frame update
    void Start()
    {
        parentRT = this.transform.parent.GetComponent<RectTransform>();
        rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rt.sizeDelta = new Vector2(parentRT.rect.width, parentRT.rect.height);
    }
}
