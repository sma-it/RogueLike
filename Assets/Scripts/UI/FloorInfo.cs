using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FloorInfo : MonoBehaviour
{
    private VisualElement root;
    private Label floorLabel;
    private Label enemiesLabel;

    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        floorLabel = root.Q<Label>("Floor");
        enemiesLabel = root.Q<Label>("Enemies");
    }

    public void SetEnemies(int value)
    {
        enemiesLabel.text = $"{value} enemies left"; 
    }

    public void SetFloor(int value)
    {
        floorLabel.text = $"floor {value}";
    }
}
