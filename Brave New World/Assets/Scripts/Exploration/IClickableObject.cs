using UnityEngine;
using System.Collections;

namespace BraveNewWorld
{
    public interface IClickableObject
    {

        ClickableObjectType ObjectType {get; set; }
        string InfoText { get; set; }

    }
}
