using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SelectedCharacters", menuName = "SelectedCharacters")]
public class SelectedCharacters : ScriptableObject
{
    public GameObject[] Characters = new GameObject[2];
}
