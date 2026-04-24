using Script.Core.Expressions.BinaryExpressions;
using UnityEngine.UIElements;
using UnityEngine;
using Script.Core.Utils;

public class ExpressionElementSpawner: VisualElement
{
    private readonly BinaryOperatorOverloadSystem _system;

    public ExpressionElementSpawner(BinaryOperatorOverloadSystem system, VisualElement editor)
    {
        _system = system;

        var field = editor.Q("Field");
        var graph = field.Q<GraphRoot>();

        AddToClassList("expr-block");

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            var expr = new ExpressionBlockView(new BinaryExpression(_system));

            var editorCenter = new Vector2(
                editor.layout.width * 0.5f,
                editor.layout.height * 0.5f
            );

            var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

            Debug.Log($"Spawn block: {_system.Tag}, to position: {centerInField}");
            graph.AddFreeBlock(expr, centerInField);
        });

        var slot1 = new VisualElement();
        var slot2 = new VisualElement();
        var op = new Label(BinaryTagOperations.GetOperatorText(_system.Tag));


        slot1.AddToClassList("expr-slot");
        slot2.AddToClassList("expr-slot");
        op.AddToClassList("expr-separator");
        
        Add(slot1);
        Add(op);
        Add(slot2);
    }  
}