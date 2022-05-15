using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasShake : MonoBehaviour
{
    [SerializeField]
    public float shakeStrength;

    [SerializeField]
    public float ShakeSpeed = 5f;
    [SerializeField]
    public float ShakeStrengthFactor = 2f;

    Color originalImageColor, originalTextColor;

    [SerializeField]
    public bool ChangeColor;

    [SerializeField]
    public Color ShakeColor;
    [SerializeField]
    public Image image;

    [SerializeField]
    public TextMeshPro text;
    public Vector3 _originalPosition;
    void Start()
    {
        if (image != null) originalImageColor = image.color;
        if (text != null) originalTextColor = text.color;
        _originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (ChangeColor)
        {
            if (image != null) image.color = originalImageColor;
            if (text != null) text.color = originalTextColor;
        }

        if (shakeStrength > 0)
        {
            transform.localPosition = _originalPosition + (Vector3)(Random.insideUnitCircle * shakeStrength * ShakeStrengthFactor);
            shakeStrength -= Time.deltaTime * ShakeSpeed;
            if (ChangeColor)
            {
                if (image != null) image.color = new Color(ShakeColor.r, ShakeColor.g, ShakeColor.b, originalImageColor.a);
                if (text != null) text.color = new Color(255f, 0f, 0f, originalTextColor.a);
            }
        }
        else
        {
            transform.localPosition = _originalPosition;
        }
    }

    public void Shake(float value)
    {
        shakeStrength = value;
    }

    public void Stop()
    {
        shakeStrength = 0f;
    }
}
