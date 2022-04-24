using UnityEngine;
public class PixelizerMaterial
{
    public Material _material;
    public Color _originalOutlineColor;
    public int _originalOutlineID;

    public PixelizerMaterial(Material m)
    {
        _originalOutlineColor = m.GetColor("_OutlineColor");
        _originalOutlineID = m.GetInt("_ID");
        _material = m;
    }

    public void ApplyOutline(Color c, int id)
    {
        _material.SetColor("_OutlineColor", c);
        _material.SetInt("_ID", id);
    }

    public void RevertOutline()
    {
        _material.SetColor("_OutlineColor", _originalOutlineColor);
        _material.SetInt("_ID", _originalOutlineID);
    }
}
