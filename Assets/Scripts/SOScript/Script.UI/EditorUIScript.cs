using UnityEngine;
using UnityEngine.UIElements;
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

        root.style.display = DisplayStyle.None;
        var elements = root.Q("Elements");
        var editor = root.Q("Editor");

        BuildEditorField(editor);
        BuildConsole(editor);
        BuildElements(elements, editor);
        BuildToolbar(root);

        // TestConsole();
    }

    void TestConsole()
    {
        List<MessageType> types = Enum.GetValues(typeof(MessageType)).Cast<MessageType>().ToList();

        for (int i = 0; i < 15; i++)
        {
            UIConsole.Instance.Write($"i = {i}", types[i % types.Count]);
        }
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
                    try
                    {
                        var var = ScriptTypeOperations.CreateVariable(type, varName);
                        AddVariable(var, editor);
                    }
                    catch(ArgumentException e)
                    {
                        UIConsole.Instance.WriteError(e.Message);
                    }

                };
            }
            else
            {
                Debug.Log($"Missing container for {typeName} variables");
            }
        }

        var messaging = elements.Q<Foldout>("Messages");
        messaging.Add(new PrintStatementBlockSpawner(UIConsole.Instance, MessageType.Info, editor));
        messaging.Add(new PrintStatementBlockSpawner(UIConsole.Instance, MessageType.Warning, editor));
        messaging.Add(new PrintStatementBlockSpawner(UIConsole.Instance, MessageType.Error, editor));
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

        var deleteButton = root.Q<Button>("DeleteSelection");
        deleteButton.clicked += DeleteSelected;
    }

    void BuildConsole(VisualElement editor)
    {
        editor.Add(UIConsole.Instance);
    }

    void DeleteSelected()
    {
        var controller = ExpressionGraphController.Instance;

        if (controller.SelectedBlock == null)
        {
            Debug.LogWarning("No block selected");
            return;
        }

        if (controller.SelectedBlock is ExpressionBlockView exprBlock)
        {
            controller.DetachBlock(exprBlock);
            controller.SelectedBlock = null;
            GraphRoot.Instance?.Remove(exprBlock);
            return;
        }

        if (controller.SelectedBlock is StatementBlockView stmtBlock)
        {
            controller.DetachBlock(stmtBlock);
            controller.SelectedBlock = null;
            GraphRoot.Instance?.Remove(stmtBlock);
            return;
        }
    }

    async void EvaluateSelected()
    {
        var controller = ExpressionGraphController.Instance;

        if (controller.SelectedBlock == null)
        {
            UIConsole.Instance.WriteWarning("No block selected");
            return;
        }

        if (controller.SelectedBlock is ExpressionBlockView exprBlock)
        {
            try
            {
                var result = await exprBlock.Expression.EvaluateAsync();
                resultLabel.text = $"Result: {result}";
            }
            catch (Exception e)
            {
                UIConsole.Instance.WriteError(e.Message);
            }
            return;
        }

        if (controller.SelectedBlock is StatementBlockView stmtBlock)
        {
            try
            {
                var result = await stmtBlock.Statement.ExecuteAsync();
                resultLabel.text = $"Statement executed (control flow: {result.Kind})";
            }
            catch (Exception e)
            {
                UIConsole.Instance.WriteError(e.Message);
            }
            return;
        }

        UIConsole.Instance.WriteWarning("Selected block is not evaluable");
    }

    public void OnEnter()
    {
        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void OnExit()
    {
        document.rootVisualElement.style.display = DisplayStyle.None;
    }
}
