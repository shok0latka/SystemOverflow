#nullable enable

using System;
using Script.Core.Statements;
using UnityEngine;
using UnityEngine.UIElements;

public enum CondStatementType
{
    WhileStatement,
    IfStatement,
    BreakStatement
}

public class CondStatementSpawner: VisualElement
{
    private readonly VisualElement content;
    public CondStatementSpawner(CondStatementType type, VisualElement editor)
    {
        content = new VisualElement();
        content.AddToClassList("stmt-block");

        Add(content);

        switch(type)
        {
            case CondStatementType.WhileStatement:
                BuildWhileSpawner(editor);
                break;
            case CondStatementType.IfStatement:
                BuildIfSpawner(editor);
                break;
            case CondStatementType.BreakStatement:
                BuildBreakSpawner(editor);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void BuildWhileSpawner(VisualElement editor)
    {
        var field = editor.Q("Field");
        var graph = field.Q<GraphRoot>();

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            var expr = new StatementBlockView(new WhileStatement());

            var editorCenter = new Vector2(
                editor.layout.width * 0.5f,
                editor.layout.height * 0.5f
            );

            var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

            Debug.Log($"Spawn block: \"While\", to position: {centerInField}");
            graph.AddFreeBlock(expr, centerInField);
        });

        BuildTitle("While");
        BuildStatementSlot("Body");
    }

    private void BuildIfSpawner(VisualElement editor)
    {
        var field = editor.Q("Field");
        var graph = field.Q<GraphRoot>();

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            var expr = new StatementBlockView(new IfStatement());

            var editorCenter = new Vector2(
                editor.layout.width * 0.5f,
                editor.layout.height * 0.5f
            );

            var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

            Debug.Log($"Spawn block: \"If\", to position: {centerInField}");
            graph.AddFreeBlock(expr, centerInField);
        });

        BuildTitle("If");
        BuildStatementSlot("Do");
        BuildStatementSlot("Else");
    }

    private void BuildBreakSpawner(VisualElement editor)
    {
        var field = editor.Q("Field");
        var graph = field.Q<GraphRoot>();

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            var expr = new StatementBlockView(new BreakStatement());

            var editorCenter = new Vector2(
                editor.layout.width * 0.5f,
                editor.layout.height * 0.5f
            );

            var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

            Debug.Log($"Spawn block: \"Break\", to position: {centerInField}");
            graph.AddFreeBlock(expr, centerInField);
        });

        BuildTitle("Break");
    }

    void BuildTitle(string text)
    {
        var title = new Label(text);
        title.AddToClassList("stmt-title");
        content.Add(title);
    }

    void BuildStatementSlot(string labelText)
    {
        var label = new Label(labelText);
        label.AddToClassList("stmt-arg-label");
        content.Add(label);

        var slot = new VisualElement();
        slot.AddToClassList("stmt-slot");
        slot.AddToClassList("stmt-slot-placeholder");
        content.Add(slot);
    }
}