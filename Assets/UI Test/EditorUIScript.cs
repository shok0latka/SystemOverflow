using UnityEngine;
using UnityEngine.UIElements;
using System.Threading.Tasks;

using Script.Core.Expressions;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.Core.Expressions.BinaryExpressions;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Addition;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Multiplication;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Subtraction;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Modulo;
using Script.Core.Expressions.BinaryExpressions.Implementations.Arithmetic.Division;
using Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.LessOrEqual;
using Script.Core.Expressions.BinaryExpressions.Implementations.Comparison.Equality;

using Script.Core.Statements;
using Script.Core.Variables.Implementations;

public class ScriptEditorUI : MonoBehaviour
{
    [SerializeField] UIDocument document;
    [SerializeField] StyleSheet style;

    Label resultLabel;

    void Start()
    {
        var root = document.rootVisualElement;

        if (style != null)
            root.styleSheets.Add(style);

        var elements = root.Q<ScrollView>("Elements");
        var editorScroll = root.Q<ScrollView>("Editor");

        CreateToolbar(root);

        var graph = new GraphRoot();
        editorScroll.contentViewport.Add(graph);

        BuildTestGraph(graph);

        editorScroll.contentViewport.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.target == editorScroll.contentViewport)
            {
                ExpressionGraphController.Instance.ClickOnEmptySpace(evt.localPosition);
                evt.StopPropagation();
            }
        });
    }

    void CreateToolbar(VisualElement root)
    {
        var toolbar = new VisualElement();
        toolbar.style.flexDirection = FlexDirection.Row;

        resultLabel = new Label("Result:");

        var evalButton = new Button(() => EvaluateSelected())
        {
            text = "Execute"
        };

        toolbar.Add(evalButton);
        toolbar.Add(resultLabel);
        toolbar.AddToClassList("expr-toolbar");

        root.Add(toolbar);
    }

    void BuildTestGraph(GraphRoot graph)
    {
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

        graph.AddFreeBlock(new ExpressionBlockView(new VariableExpression(i), "i_expr_2"));

        graph.AddFreeBlock(new ExpressionBlockView(new NumeralExpression() { RawText = "1" }, "Num_2"));
    }

    async void EvaluateSelected()
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
}