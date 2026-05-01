using UnityEngine;
using UnityEngine.UIElements;

using Script.Core.Expressions;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.Core.Expressions.BinaryExpressions;
using Script.Core.Expressions.BinaryExpressions.Arithmetic;
using Script.Core.Expressions.BinaryExpressions.Comparison;
using Script.Core.Statements;
using Script.Core.Variables.Implementations;
using Script.Core.Utils;
using System;
using System.Linq;
using System.Collections.Generic;
using Script.Core.Variables;
using Script.Core.Types;
using Script.UI.Controllers;
using Script.UI.Spawners;
using Script.UI.Views;

public class ScriptEditorUI : MonoBehaviour
{
    [SerializeField] UIDocument document;
    [SerializeField] StyleSheet style;
    [SerializeField] OverloadSystem system;

    Label resultLabel;
    Vector3 startMouse;
    float startLeft;
    float startTop;

    float minZoom;
    float zoom = 1f;
    float maxZoom = 2f;

    List<Variable> scope = new();
    Foldout varList;
    

    void Start()
    {
        var root = document.rootVisualElement;

        if (style != null)
            root.styleSheets.Add(style);

        var elements = root.Q("Elements");
        var editor = root.Q("Editor");

        BuildEditorField(editor);
        BuildElements(elements, editor);

        BuildToolbar(root);
    }

    void BuildEditorField(VisualElement editor)
    {
        var field = editor.Q("Field");
        field.visible = true;

        field.RegisterCallback<PointerDownEvent>(evt =>
        {
            startMouse = evt.position;

            startLeft = field.resolvedStyle.left;
            startTop  = field.resolvedStyle.top;
        });

        field.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (evt.pressedButtons == 1)
            {
                Vector2 delta = evt.position - startMouse;

                float newLeft = startLeft + delta.x;
                float newTop  = startTop + delta.y;

                field.style.left = newLeft;
                field.style.top  = newTop;
            }
        });

        field.RegisterCallback<WheelEvent>(evt =>
        {
            float zoomDelta = -evt.delta.y;
            float newZoom = Mathf.Clamp(zoom + zoomDelta, minZoom, maxZoom);

            Debug.Log($"New zoom: {newZoom}");

            if (Mathf.Approximately(newZoom, zoom))
                return;

            Vector2 mousePos = editor.WorldToLocal(evt.mousePosition);

            float oldZoom = zoom;
            zoom = newZoom;

            float left = field.resolvedStyle.left;
            float top  = field.resolvedStyle.top;

            float scaleFactor = zoom / oldZoom;

            float newLeft = mousePos.x - (mousePos.x - left) * scaleFactor;
            float newTop  = mousePos.y - (mousePos.y - top) * scaleFactor;

            field.style.scale = new Scale(new Vector3(zoom, zoom, 1));

            field.style.left = newLeft;
            field.style.top  = newTop;

            evt.StopPropagation();
        });
        
        field.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            Debug.Log($"Geometry Changed");

            minZoom = Mathf.Max(
                editor.layout.width  / field.layout.width,
                editor.layout.height / field.layout.height
            );

            var fieldRect = field.layout;
            var editorRect = editor.layout;

            float fieldWidth = fieldRect.width * zoom;
            float fieldHeight = fieldRect.height * zoom;
            float newTop = field.resolvedStyle.top;
            float newLeft = field.resolvedStyle.left;

            float editorWidth = editorRect.width;
            float editorHeight = editorRect.height;

            float minX = editorWidth - fieldWidth;
            float maxX = 0;

            float minY = editorHeight - fieldHeight;
            float maxY = 0;

            newLeft = Mathf.Clamp(newLeft, minX, maxX);
            newTop  = Mathf.Clamp(newTop,  minY, maxY);

            field.style.top = newTop;
            field.style.left = newLeft;
        });

        var graph = new GraphRoot();        
        field.Add(graph);

        // BuildTestGraph(graph);

        field.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.target == field)
            {
                ExpressionGraphController.Instance.ClickOnEmptySpace(evt.localPosition);
                evt.StopPropagation();
            }
        });
    }

    void BuildElements(VisualElement elements, VisualElement editor)
    {
        var arithmetics = elements.Q<Foldout>("Arithmetics");

        foreach (var tag in BinaryTagOperations.Arithmetics)
        {
            var block = new ExpressionElementSpawner(system[tag], editor);
            arithmetics.Add(block);
        }

        var comparison = elements.Q<Foldout>("Comparison");

        foreach (var tag in BinaryTagOperations.Comparison)
        {
            var block = new ExpressionElementSpawner(system[tag], editor);
            comparison.Add(block);
        }

        var literal = elements.Q<Foldout>("LiteralExpressions");

        foreach (var type in Enum.GetValues(typeof(UserInputExpressionType)).Cast<UserInputExpressionType>())
        {
            var block = new ExpressionElementSpawner(type, editor);
            literal.Add(block);
        }

        foreach (var constant in new bool[] {true, false})
        {
            var block = new ExpressionElementSpawner(constant, editor);
            literal.Add(block);
        }
        var cfStmts = elements.Q<Foldout>("ControlFlowStmts");

        foreach (var type in Enum.GetValues(typeof(CondStatementType)).Cast<CondStatementType>())
        {
            var block = new CondStatementSpawner(type, editor);
            cfStmts.Add(block);
        }

        varList = elements.Q<Foldout>("VarList");
        Foldout create = elements.Q<Foldout>("VarCreate");

        List<string> varTypesStr = new() {"String", "Float", "Int", "Bool"};
        List<ScriptType> varTypesEnum = new() {ScriptType.String, ScriptType.Float, ScriptType.Integer, ScriptType.Boolean};

        for (int i = 0; i < Math.Min(varTypesStr.Count, varTypesEnum.Count); i++)
        {
            var typeName = varTypesStr[i];
            var type = varTypesEnum[i];
            var container = create.Q($"{typeName}VarCreation");
            if (container is not null)
            {
                var textField = container.Q<TextField>();
                var button = container.Q<Button>();

                button.clicked += () =>
                {
                    var varName = textField.value;
                    var var = ScriptTypeOperations.CreateVariable(type, varName); // TODO exception handling

                    AddVariable(var, editor);
                };
            }
            else
            {
                Debug.Log($"Missing container for {typeName} variables");
            }
        }
    }

    void AddVariable(Variable var, VisualElement editor)
    {
        Debug.Log($"Create Variable(type={var.Type}, name={var.Name})");
        scope.Add(var);
        var section = new VarSpawnSection(var, editor);
        varList.Add(section);
    }

    void BuildToolbar(VisualElement root)
    {
        resultLabel = root.Q<Label>("Result");


        var evalButton = root.Q<Button>("Execute");
        evalButton.clicked += EvaluateSelected;
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
