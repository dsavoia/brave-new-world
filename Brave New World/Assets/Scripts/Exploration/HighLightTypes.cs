using UnityEngine;
using System.Collections;

public class HighLightTypes {

    public enum HighLightType
    {
        Enemy,
        Ally,
        Neutral
    }

    public GameObject highLightPB;
    public HighLightType highLightType;

}
