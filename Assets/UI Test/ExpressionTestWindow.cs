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
using Script.Core.Statements;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Modulo;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Division;
using Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.LessOrEqual;
using Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Equality;
using Script.Core.Variables.Implementations;
using Script.Core.Expressions;

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
        var root = rootVisualElement;

        LoadStyles(root);

        var toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;

        var resultLabel = new Label("Result: ");

        var evalButton = new Button(() => EvaluateSelected(resultLabel))
        {
            text = "Execute"
        };

        toolbar.Add(evalButton);
        toolbar.Add(resultLabel);
        toolbar.AddToClassList("expr-toolbar");

        root.Add(toolbar);

        var graph = new GraphRoot();
        root.Add(graph);

        var addSystem = new BinaryOperatorOverloadSystem<AdditionOperator>();
        var mulSystem = new BinaryOperatorOverloadSystem<MultiplicationOperator>();
        var divSystem = new BinaryOperatorOverloadSystem<DivisionOperator>();
        var modSystem = new BinaryOperatorOverloadSystem<ModuloOperator>();
        var leSystem = new BinaryOperatorOverloadSystem<LessOrEqualOperator>();
        var eqSystem = new BinaryOperatorOverloadSystem<EqualityOperator>();

        var n = new IntVariable("n");
        var i = new IntVariable("i");

        graph.AddFreeBlock(new StatementBlockView(new AssignStatement(i), "Assign_0"));
        graph.AddFreeBlock(new StatementBlockView(new AssignStatement(n), "Assign_1"));
        graph.AddFreeBlock(new ExpressionBlockView(new NumeralExpression() { RawText = "1" }, "Num_0"));
        graph.AddFreeBlock(new ExpressionBlockView(new NumeralExpression() { RawText = "1" }, "Num_1"));
        graph.AddFreeBlock(new StatementBlockView(new WhileStatement(), "While"));
        graph.AddFreeBlock(new ExpressionBlockView(new VariableExpression(i), "i_expr_0"));
        graph.AddFreeBlock(new ExpressionBlockView(new VariableExpression(n), "n_expr_0"));
        graph.AddFreeBlock(new ExpressionBlockView(new BinaryExpression(leSystem), "Le"));
        graph.AddFreeBlock(new StatementBlockView(new PrintStatement(), "Print"));
        graph.AddFreeBlock(new ExpressionBlockView(new VariableExpression(i), "i_expr_1"));
        graph.AddFreeBlock(new StatementBlockView(new AssignStatement(i), "Assign_2"));
        graph.AddFreeBlock(new ExpressionBlockView(new BinaryExpression(addSystem), "add"));
        graph.AddFreeBlock(new ExpressionBlockView(new VariableExpression(i), "i_expr_1"));
        graph.AddFreeBlock(new ExpressionBlockView(new NumeralExpression() { RawText = "1" }, "Num_2"));
    }

    async void EvaluateSelected(Label resultLabel)
    {
        var controller = ExpressionGraphController.Instance;

        if (controller.SelectedBlock == null)
        {
            Debug.LogWarning("No block selected");
            return;
        }

        if (controller.SelectedBlock is ExpressionBlockView exprBlock)
        {
            var result = await exprBlock.Expression.EvaluateAsync();
            resultLabel.text = $"Result: {result}";
            return;
        }

        if (controller.SelectedBlock is StatementBlockView stmtBlock)
        {
            var result = await stmtBlock.Statement.ExecuteAsync();
            resultLabel.text = $"Statement executed (control flow: {result.Kind})";
            return;
        }

        Debug.LogWarning("Selected block is not evaluable");
    }

    void LoadStyles(VisualElement root)
    {
        var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/UI Test/expression-block.uss");

        if (style != null)
            root.styleSheets.Add(style);
    }
}
