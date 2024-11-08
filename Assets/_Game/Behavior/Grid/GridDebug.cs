using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GridDebug : MonoBehaviour
{
    private TextMeshProUGUI m_x;
    private TextMeshProUGUI m_y;

    private void Awake()
    {
        m_x = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        m_y = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void SetCoordinates(Vector2Int coordinates)
    {
        m_x.text = coordinates.x.ToString();
        m_y.text = coordinates.y.ToString();
    }
}
