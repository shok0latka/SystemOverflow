#nullable enable

using UnityEngine.UIElements;
using UnityEngine;

public class GraphRoot : VisualElement
{
    public static GraphRoot? Instance;

    public GraphRoot()
    {
        Instance = this;
        Debug.Log("[GraphRoot] GraphRoot created");

        style.position = Position.Relative;
        style.flexGrow = 1;

        RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("[GraphRoot] Empty space click");
            ExpressionGraphController.Instance.ClickOnEmptySpace(evt.localPosition);
        });
    }

    public void AddFreeBlock(ExpressionBlockView block)
    {
        block.RemoveFromHierarchy();

        block.style.position = Position.Absolute;

        Add(block);
    }
}