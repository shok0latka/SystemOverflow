using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.Core.Expressions.BinaryExpressions;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Addition;
using Unity.VisualScripting;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Multiplication;

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
        var root = rootVisualElement;

        LoadStyles(root);

        var graph = new GraphRoot();

        root.Add(graph);

        var literal1 = new ExpressionBlockView(new NumeralExpression() { RawText="1" }, "Literal_1");
        var literal2 = new ExpressionBlockView(new NumeralExpression() { RawText="5" }, "Literal_2");
        var addBlock = new ExpressionBlockView(new BinaryExpression(add_system), "Addition_Op");
        var mulBlock = new ExpressionBlockView(new BinaryExpression(mul_system), "Multiplication_Op");

        graph.AddFreeBlock(literal1);
        graph.AddFreeBlock(literal2);
        graph.AddFreeBlock(addBlock);
        graph.AddFreeBlock(mulBlock);
    }

    void LoadStyles(VisualElement root)
    {
        var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            "Assets/UI Test/expression-block.uss");

        if (style != null)
            root.styleSheets.Add(style);
    }
}
