using Script.Core.Expressions.BinaryExpressions;
using UnityEngine.UIElements;
using UnityEngine;
using Script.Core.Utils;
using System;
using Script.Core.Expressions.LiteralExpressions;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.UI.Controllers;
using Script.UI.Views;


namespace Script.UI.Spawners 
{
    public enum UserInputExpressionType
    {
        Numeral,
        Literal
    }

    public class ExpressionElementSpawner: VisualElement
    {
        public ExpressionElementSpawner(BinaryOperatorOverloadSystem system, VisualElement editor)
        {
            var field = editor.Q("Field");
            var graph = field.Q<GraphRoot>();

            AddToClassList("expr-block");

            RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                var expr = new ExpressionBlockView(new BinaryExpression(system));

                var editorCenter = new Vector2(
                    editor.layout.width * 0.5f,
                    editor.layout.height * 0.5f
                );

                var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

                Debug.Log($"Spawn block: {system.Tag}, to position: {centerInField}");
                graph.AddFreeBlock(expr, centerInField);
            });

            var slot1 = new VisualElement();
            var slot2 = new VisualElement();
            var op = new Label(BinaryTagOperations.GetOperatorText(system.Tag));


            slot1.AddToClassList("expr-slot");
            slot2.AddToClassList("expr-slot");
            op.AddToClassList("expr-separator");
            
            Add(slot1);
            Add(op);
            Add(slot2);
        }  

        public ExpressionElementSpawner(UserInputExpressionType type, VisualElement editor)
        {
            var field = editor.Q("Field");
            var graph = field.Q<GraphRoot>();
            
            string prefix = type switch
            {
                UserInputExpressionType.Numeral => "num:",
                UserInputExpressionType.Literal => "str:",
                _ => throw new NotImplementedException()
            };

            RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();

                var expr = type switch 
                {
                    UserInputExpressionType.Numeral => new ExpressionBlockView(new NumeralExpression()),
                    UserInputExpressionType.Literal => new ExpressionBlockView(new LiteralExpression()),
                    _ => throw new NotImplementedException()
                };

                var editorCenter = new Vector2(
                    editor.layout.width * 0.5f,
                    editor.layout.height * 0.5f
                );

                var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

                Debug.Log($"Spawn block: {type}, to position: {centerInField}");
                graph.AddFreeBlock(expr, centerInField);
            });

            AddToClassList("expr-block");

            var label = new Label(prefix);
            label.AddToClassList("expr-prefix");
            Add(label);

            var fieldPlaceholder = new Label();
            fieldPlaceholder.AddToClassList("expr-input");

            Add(fieldPlaceholder);
        }

        public ExpressionElementSpawner(bool constant, VisualElement editor)
        {
            var field = editor.Q("Field");
            var graph = field.Q<GraphRoot>();

            AddToClassList("expr-block");

            var label = new Label(constant ? "true" : "false");
            label.AddToClassList("expr-bool");
            Add(label);

            RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                var expr = constant ? new ExpressionBlockView(new TrueConstant()) : new ExpressionBlockView(new FalseConstant());

                var editorCenter = new Vector2(
                    editor.layout.width * 0.5f,
                    editor.layout.height * 0.5f
                );

                var centerInField = editor.ChangeCoordinatesTo(field, editorCenter);

                Debug.Log($"Spawn block: {constant}, to position: {centerInField}");
                graph.AddFreeBlock(expr, centerInField);
            });
        }
    }
}