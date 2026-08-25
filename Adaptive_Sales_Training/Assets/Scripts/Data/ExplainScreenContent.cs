using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExplainScreenContent", menuName = "Scriptable Objects/ExplainScreenContent")]
public class ExplainScreenContent : ScriptableObject
{
    [TextArea(8, 12)]
    public List<string> texts = new();
}
