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
        var controller = ExpressionGraphController.Instance;

        var addSystem = new BinaryOperatorOverloadSystem<AdditionOperator>();
        var leSystem = new BinaryOperatorOverloadSystem<LessOrEqualOperator>();

        var n = new IntVariable("n");
        var i = new IntVariable("i");

        var assignI = new StatementBlockView(new AssignStatement(i), "Assign_i");
        var assignN = new StatementBlockView(new AssignStatement(n), "Assign_n");
        var const1a = new ExpressionBlockView(new NumeralExpression() { RawText = "1" }, "Num_1a");
        var const5 = new ExpressionBlockView(new NumeralExpression() { RawText = "5" }, "Num_5");

        var whileBlock = new StatementBlockView(new WhileStatement(), "While");
        var iCondExpr = new ExpressionBlockView(new VariableExpression(i), "i_expr_cond");
        var nCondExpr = new ExpressionBlockView(new VariableExpression(n), "n_expr_cond");
        var leExpr = new ExpressionBlockView(new BinaryExpression(leSystem), "Le");

        var printStmt = new StatementBlockView(new PrintStatement(), "Print");
        var iPrintExpr = new ExpressionBlockView(new VariableExpression(i), "i_expr_print");

        var assignI2 = new StatementBlockView(new AssignStatement(i), "Assign_i_inc");
        var addExpr = new ExpressionBlockView(new BinaryExpression(addSystem), "Add");
        var iIncExpr = new ExpressionBlockView(new VariableExpression(i), "i_expr_inc");
        var const1b = new ExpressionBlockView(new NumeralExpression() { RawText = "1" }, "Num_1b");

        graph.AddFreeBlock(assignI);
        graph.AddFreeBlock(assignN);
        graph.AddFreeBlock(const1a);
        graph.AddFreeBlock(const5);

        graph.AddFreeBlock(whileBlock);
        graph.AddFreeBlock(iCondExpr);
        graph.AddFreeBlock(nCondExpr);
        graph.AddFreeBlock(leExpr);

        graph.AddFreeBlock(printStmt);
        graph.AddFreeBlock(iPrintExpr);

        graph.AddFreeBlock(assignI2);
        graph.AddFreeBlock(addExpr);
        graph.AddFreeBlock(iIncExpr);
        graph.AddFreeBlock(const1b);

       
        StmtSlotView FindSlot(StatementBlockView block, StmtSlotKind kind)
        {
            foreach (var s in block.StmtSlots)
            {
                if (s.Kind == kind) return s;
            }
            throw new System.InvalidOperationException($"Slot {kind} not found on {block.DebugName}");
        }

        controller.SelectBlock(const1a);
        controller.SelectSlot(assignI.ExprSlots[0]);

        controller.SelectBlock(const5);
        controller.SelectSlot(assignN.ExprSlots[0]);

        controller.SelectBlock(assignN);
        controller.SelectSlot(FindSlot(assignI, StmtSlotKind.Next));

        controller.SelectBlock(iCondExpr);
        controller.SelectSlot(leExpr.Slots[0]);

        controller.SelectBlock(nCondExpr);
        controller.SelectSlot(leExpr.Slots[1]);

        controller.SelectBlock(leExpr);
        controller.SelectSlot(whileBlock.ExprSlots[0]);

        controller.SelectBlock(iPrintExpr);
        controller.SelectSlot(printStmt.ExprSlots[0]);

        controller.SelectBlock(printStmt);
        controller.SelectSlot(FindSlot(whileBlock, StmtSlotKind.Body));

        controller.SelectBlock(assignI2);
        controller.SelectSlot(FindSlot(printStmt, StmtSlotKind.Next));

        controller.SelectBlock(iIncExpr);
        controller.SelectSlot(addExpr.Slots[0]);

        controller.SelectBlock(const1b);
        controller.SelectSlot(addExpr.Slots[1]);

        controller.SelectBlock(addExpr);
        controller.SelectSlot(assignI2.ExprSlots[0]);
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