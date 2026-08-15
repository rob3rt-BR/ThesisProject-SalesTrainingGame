using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SalesTopic", menuName = "Scriptable Objects/SalesTopic")]
public class SalesTopic : ScriptableObject
{
    public string topicId;
    public string topicTitle;
    public List<MicroModule> modules = new();
}
