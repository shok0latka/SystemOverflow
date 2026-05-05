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
    static readonly HackCommand[] EnemyCommandPaletteOrder =
    {
        HackCommand.MoveUp,
        HackCommand.MoveDown,
        HackCommand.MoveLeft,
        HackCommand.MoveRight,
        HackCommand.RotateCounterClockwise,
        HackCommand.RotateClockwise,
        HackCommand.Interact,
        HackCommand.Attack
    };

    [SerializeField] UIDocument document;
    [SerializeField] StyleSheet style;
    [SerializeField] OverloadSystem system;
    [SerializeField] bool openOnStart;
    [SerializeField] bool pauseWhileEditing = true;

    Label resultLabel;
    VisualElement editorRoot;
    VisualElement toolbar;
    HackedRobotAccessButton hackedRobotAccessButton;
    Vector3 startMouse;
    float startLeft;
    float startTop;
    EnemyHackController boundTarget;
    float previousTimeScale = 1f;
    bool timePausedForEditing;
    bool editorVisible;

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

        root.style.display = DisplayStyle.Flex;
        root.pickingMode = PickingMode.Ignore;
        editorRoot = root.Q("SO_Script_Editor");
        toolbar = root.Q("Toolbar");

        var elements = root.Q("Elements");
        var editor = root.Q("Editor");

        BuildEditorField(editor);
        BuildConsole(editor);
        BuildElements(elements, editor);
        BuildToolbar(root);
        hackedRobotAccessButton = new HackedRobotAccessButton(root, OpenForHackTarget);
        SetEditorVisible(openOnStart);

        // TestConsole();
    }

    void Update()
    {
        if (boundTarget != null && !boundTarget.GetHackStatus().IsActive)
        {
            CloseEditor(clearQueuedCommands: false, cancelHack: false);
        }

        hackedRobotAccessButton?.Refresh(editorVisible);
    }

    void OnDestroy()
    {
        CloseEditor(clearQueuedCommands: false, cancelHack: false);
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

        var enemyCommands = GetOrCreateEnemyCommandsFoldout(elements);
        foreach (var command in EnemyCommandPaletteOrder)
        {
            enemyCommands.Add(new EnemyCommandStatementSpawner(command, editor));
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

    Foldout GetOrCreateEnemyCommandsFoldout(VisualElement elements)
    {
        var enemyCommands = elements.Q<Foldout>("EnemyCommands");
        if (enemyCommands != null)
        {
            return enemyCommands;
        }

        enemyCommands = new Foldout
        {
            name = "EnemyCommands",
            text = "Enemy Commands",
            value = true
        };
        enemyCommands.style.marginRight = 20;

        var scrollView = elements.Q<ScrollView>();
        if (scrollView != null)
        {
            scrollView.Add(enemyCommands);
        }
        else
        {
            elements.Add(enemyCommands);
        }

        return enemyCommands;
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

        var cancelButton = root.Q<Button>("Cancel");
        if (cancelButton != null)
        {
            cancelButton.clicked += CancelEditor;
        }

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
                if (boundTarget != null)
                {
                    CloseEditor(clearQueuedCommands: false, cancelHack: false);
                }
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
        SetEditorVisible(true);
    }

    public void OnExit()
    {
        CloseEditor(clearQueuedCommands: false, cancelHack: false);
    }

    void OpenForHackTarget(EnemyHackController target)
    {
        if (target == null)
        {
            return;
        }

        if (!target.GetHackStatus().IsActive)
        {
            UIConsole.Instance.WriteWarning("Enemy is no longer hacked.");
            return;
        }

        boundTarget = target;
        boundTarget.ClearCommands();
        EnemyCommandScriptContext.Bind(boundTarget);
        PauseGameplay();
        SetEditorVisible(true);
        resultLabel.text = $"Result: Target {boundTarget.name}";
        UIConsole.Instance.Write($"Command target: {boundTarget.name}", MessageType.Info);
    }

    void CancelEditor()
    {
        CloseEditor(clearQueuedCommands: true, cancelHack: true);
    }

    void CloseEditor(bool clearQueuedCommands, bool cancelHack)
    {
        if (clearQueuedCommands)
        {
            boundTarget?.ClearCommands();
        }

        if (cancelHack)
        {
            boundTarget?.TryCancelHack();
        }

        if (boundTarget != null)
        {
            EnemyCommandScriptContext.Clear(boundTarget);
        }
        else if (!EnemyCommandScriptContext.HasTarget)
        {
            EnemyCommandScriptContext.Clear();
        }

        boundTarget = null;
        RestoreGameplay();
        SetEditorVisible(false);
        ExpressionGraphController.Instance.ClearSelection();
    }

    void PauseGameplay()
    {
        if (!pauseWhileEditing || timePausedForEditing)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        timePausedForEditing = true;
    }

    void RestoreGameplay()
    {
        if (!timePausedForEditing)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
        timePausedForEditing = false;
    }

    void SetEditorVisible(bool visible)
    {
        if (document == null || document.rootVisualElement == null)
        {
            return;
        }

        editorVisible = visible;

        VisualElement root = document.rootVisualElement;
        root.style.display = DisplayStyle.Flex;
        root.pickingMode = PickingMode.Ignore;

        editorRoot ??= root.Q("SO_Script_Editor");
        toolbar ??= root.Q("Toolbar");

        SetElementVisible(editorRoot, visible);
        SetElementVisible(toolbar, visible);
        hackedRobotAccessButton?.Refresh(editorVisible);
    }

    void SetElementVisible(VisualElement element, bool visible)
    {
        if (element == null)
        {
            return;
        }

        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        element.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
    }

}
