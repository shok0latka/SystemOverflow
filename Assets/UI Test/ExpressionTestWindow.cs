using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.Core.Expressions.BinaryExpressions;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Addition;
using Unity.VisualScripting;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Multiplication;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Subtraction;

public class ExpressionTestWindow : EditorWindow
{
    [MenuItem("Tools/Expression Test Window")]
    public static void Open()
    {
        var wnd = GetWindow<ExpressionTestWindow>();
        wnd.titleContent = new GUIContent("Expression Test");
    }

    public void CreateGUI()
    {
        var add_system = new BinaryOperatorOverloadSystem<AdditionOperator>();
        var mul_system = new BinaryOperatorOverloadSystem<MultiplicationOperator>();
        var sub_system = new BinaryOperatorOverloadSystem<SubtractionOperator>();
        var root = rootVisualElement;

        LoadStyles(root);
        var toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;
        
        var resultLabel = new Label("Result: ");
        var evalButton = new Button(() => EvaluateSelected(resultLabel))
        {
            text = "Evaluate"
        };
        toolbar.Add(evalButton);
        toolbar.Add(resultLabel);
        toolbar.AddToClassList("expr-toolbar");

        var graph = new GraphRoot();

        root.Add(toolbar);
        root.Add(graph);
        

        var literal1 = new ExpressionBlockView(new NumeralExpression() { RawText="1" }, "Literal_1");
        var literal2 = new ExpressionBlockView(new NumeralExpression() { RawText="5" }, "Literal_2");
        var literal3 = new ExpressionBlockView(new NumeralExpression() { RawText="5" }, "Literal_3");
        var literal4 = new ExpressionBlockView(new NumeralExpression() { RawText="5" }, "Literal_4");
        var literal5 = new ExpressionBlockView(new NumeralExpression() { RawText="5" }, "Literal_5");
        var addBlock = new ExpressionBlockView(new BinaryExpression(add_system), "Addition_Op");
        var mulBlock = new ExpressionBlockView(new BinaryExpression(mul_system), "Multiplication_Op");
        var subBlock = new ExpressionBlockView(new BinaryExpression(sub_system), "Subtraction_Op");

        graph.AddFreeBlock(literal1);
        graph.AddFreeBlock(literal2);
        graph.AddFreeBlock(literal3);
        graph.AddFreeBlock(literal4);
        graph.AddFreeBlock(literal5);
        graph.AddFreeBlock(addBlock);
        graph.AddFreeBlock(mulBlock);
        graph.AddFreeBlock(subBlock);
    }

    async void EvaluateSelected(Label resultLabel)
    {
        var controller = ExpressionGraphController.Instance;

        if (controller.SelectedBlock == null)
        {
            Debug.LogWarning("No block selected");
            return;
        }

        var expr = controller.SelectedBlock.Expression;
        var result = await expr.EvaluateAsync();

        resultLabel.text = $"Result: {result}";
    }

    void LoadStyles(VisualElement root)
    {
        var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/UI Test/expression-block.uss");

        if (style != null)
            root.styleSheets.Add(style);
    }
}
