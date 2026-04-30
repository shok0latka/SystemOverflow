using UnityEngine;
using UnityEngine.UIElements;

public class EnemyCommandStatementSpawner : VisualElement
{
    public EnemyCommandStatementSpawner(HackCommand command, VisualElement editor)
    {
        var field = editor.Q("Field");
        var graph = field.Q<GraphRoot>();

        AddToClassList("stmt-block");
        Add(new Label(EnemyCommandStatement.GetDisplayName(command)));

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();

            var block = new StatementBlockView(new EnemyCommandStatement(command));
            var editorCenter = new Vector2(
                editor.layout.width * 0.5f,
                editor.layout.height * 0.5f);
            var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

            graph.AddFreeBlock(block, centerInField);
        });
    }
}
