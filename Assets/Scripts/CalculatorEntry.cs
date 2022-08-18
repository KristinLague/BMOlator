using UnityEngine.UIElements;

public class CalculatorEntry : UIBase
{
    public string Value { get; private set; }
    
    private VisualElement _root;
    private Label _entryLB;

    public void SetVisualElement(VisualElement root)
    {
        _root = root;
        _entryLB = root.Q<Label>("entryLB");
    }

    public void SetData(string value)
    {
        Value = value;
        _entryLB.text = Value;
    }
}
