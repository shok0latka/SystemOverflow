using Script.Core.Expressions;
using Script.Core.Statements;
using Script.Core.Variables;
using UnityEngine;
using UnityEngine.UIElements;

public class VarSpawnSection: Foldout
{

    public VarSpawnSection(Variable variable, VisualElement editor)
    {
        text = $"{variable.Name}: {ScriptTypeOperations.GetTypeText(variable.Type)}";
        value = false;

        var expr = new VarExpressionSpawn(variable, editor);
        Add(expr);

        var assign = new VarAssignSpawn(variable, editor);
        Add(assign);
    }
}

public class VarExpressionSpawn: VisualElement
{
    public VarExpressionSpawn(Variable variable, VisualElement editor)
    {
        AddToClassList("expr-block");

        var label = new Label($"{variable.Name}: {ScriptTypeOperations.GetTypeText(variable.Type)}");
        label.AddToClassList("expr-variable");

        Add(label);

        var field = editor.Q("Field");
        var graph = field.Q<GraphRoot>();

        
        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            var expr = new ExpressionBlockView(new VariableExpression(variable));

            var editorCenter = new Vector2(
                editor.layout.width * 0.5f,
                editor.layout.height * 0.5f
            );

            var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

            Debug.Log($"Spawn var expr: {variable.Name}: {variable.Type}, to position: {centerInField}");
            graph.AddFreeBlock(expr, centerInField);
        });
    }
}

public class VarAssignSpawn: VisualElement
{
    public VarAssignSpawn(Variable variable, VisualElement editor)
    {
        var field = editor.Q("Field");
        var graph = field.Q<GraphRoot>();

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            var expr = new StatementBlockView(new AssignStatement(variable));

            var editorCenter = new Vector2(
                editor.layout.width * 0.5f,
                editor.layout.height * 0.5f
            );

            var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

            Debug.Log($"Spawn assign block {{{variable.Name}: {variable.Type}}}, to position: {centerInField}");
            graph.AddFreeBlock(expr, centerInField);
        });

        var title = new Label($"Assign ({variable.Name}: {ScriptTypeOperations.GetTypeText(variable.Type)})");
        title.AddToClassList("stmt-title");
        Add(title);
    }
}
