using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

using Script.Core.Expressions.BinaryExpressions;
using Script.Core.Expressions.BinaryExpressions.Arithmetic;
using Script.Core.Expressions.BinaryExpressions.Comparison;
using Script.Core.Expressions.LiteralExpressions;
using Script.Core.Expressions.LiteralExpressions.Implementations;
using Script.Core.Statements;
using Script.Core.Utils;
using Script.Core.Variables.Implementations;

public class ScriptEditorUI : MonoBehaviour
{
    [SerializeField] UIDocument document;
    [SerializeField] StyleSheet style;
    [SerializeField] OverloadSystem system;

    [Header("Runtime Hack UI")]
    [SerializeField] private bool openOnStart = true;
    [SerializeField] private bool pauseWhileEditing = true;
    [SerializeField] private PlayerInteractor playerInteractor;
    [SerializeField] private bool autoFindPlayerInteractor = true;

    Label resultLabel;
    GraphRoot graph;
    ProgramRootStatement programRootStatement;
    StatementBlockView programRootBlock;
    EnemyHackController boundTarget;
    PlayerInteractor subscribedInteractor;

    Vector3 startMouse;
    float startLeft;
    float startTop;

    float minZoom = 0.25f;
    float zoom = 1f;
    float maxZoom = 2f;

    bool timePausedForEditing;
    float previousTimeScale = 1f;

    void Start()
    {
        if (document == null)
        {
            document = GetComponent<UIDocument>();
        }

        if (document == null)
        {
            Debug.LogError($"[{nameof(ScriptEditorUI)}] Missing UIDocument reference.", this);
            enabled = false;
            return;
        }

        var root = document.rootVisualElement;

        if (style != null)
            root.styleSheets.Add(style);

        var elements = root.Q("Elements");
        var editor = root.Q("Editor");

        BuildEditorField(editor);
        BuildElements(elements, editor);
        BuildToolbar(root);
        SubscribeToHackEvents();

        SetEditorVisible(openOnStart);
    }

    private void OnDestroy()
    {
        UnsubscribeFromHackEvents();
        RestoreTimeAfterEditing();
        EnemyCommandScriptContext.Clear(boundTarget);
    }

    void BuildEditorField(VisualElement editor)
    {
        var field = editor.Q("Field");
        field.visible = true;

        field.RegisterCallback<PointerDownEvent>(evt =>
        {
            startMouse = evt.position;

            startLeft = field.resolvedStyle.left;
            startTop = field.resolvedStyle.top;
        });

        field.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (evt.pressedButtons == 1)
            {
                Vector2 delta = evt.position - startMouse;

                float newLeft = startLeft + delta.x;
                float newTop = startTop + delta.y;

                field.style.left = newLeft;
                field.style.top = newTop;
            }
        });

        field.RegisterCallback<WheelEvent>(evt =>
        {
            float zoomDelta = -evt.delta.y;
            float newZoom = Mathf.Clamp(zoom + zoomDelta, minZoom, maxZoom);

            if (Mathf.Approximately(newZoom, zoom))
                return;

            Vector2 mousePos = editor.WorldToLocal(evt.mousePosition);

            float oldZoom = zoom;
            zoom = newZoom;

            float left = field.resolvedStyle.left;
            float top = field.resolvedStyle.top;

            float scaleFactor = zoom / oldZoom;

            float newLeft = mousePos.x - (mousePos.x - left) * scaleFactor;
            float newTop = mousePos.y - (mousePos.y - top) * scaleFactor;

            field.style.scale = new Scale(new Vector3(zoom, zoom, 1));

            field.style.left = newLeft;
            field.style.top = newTop;

            evt.StopPropagation();
        });

        field.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            minZoom = Mathf.Max(
                editor.layout.width / field.layout.width,
                editor.layout.height / field.layout.height);

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
            newTop = Mathf.Clamp(newTop, minY, maxY);

            field.style.top = newTop;
            field.style.left = newLeft;
        });

        graph = new GraphRoot();
        field.Add(graph);
        BuildProgramRoot();

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
        if (system != null)
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
        }

        var literal = elements.Q<Foldout>("LiteralExpressions");

        foreach (var type in Enum.GetValues(typeof(UserInputExpressionType)).Cast<UserInputExpressionType>())
        {
            var block = new ExpressionElementSpawner(type, editor);
            literal.Add(block);
        }

        var cfStmts = elements.Q<Foldout>("ControlFlowStmts");

        foreach (var type in Enum.GetValues(typeof(CondStatementType)).Cast<CondStatementType>())
        {
            var block = new CondStatementSpawner(type, editor);
            cfStmts.Add(block);
        }

        var enemyCommands = EnsureEnemyCommandsFoldout(elements);
        foreach (var command in Enum.GetValues(typeof(HackCommand))
                     .Cast<HackCommand>())
        {
            if (command == HackCommand.None)
            {
                continue;
            }

            enemyCommands.Add(new EnemyCommandStatementSpawner(command, editor));
        }
    }

    Foldout EnsureEnemyCommandsFoldout(VisualElement elements)
    {
        var existing = elements.Q<Foldout>("EnemyCommands");
        if (existing != null)
        {
            return existing;
        }

        var foldout = new Foldout
        {
            name = "EnemyCommands",
            text = "Enemy Commands",
            value = true
        };

        var scrollView = elements.Q<ScrollView>();
        if (scrollView != null)
        {
            scrollView.Add(foldout);
        }
        else
        {
            elements.Add(foldout);
        }

        return foldout;
    }

    void BuildToolbar(VisualElement root)
    {
        resultLabel = root.Q<Label>("Result");

        var evalButton = root.Q<Button>("Execute");
        if (evalButton != null)
        {
            evalButton.clicked += ExecuteProgram;
        }

        var toolbar = root.Q("Toolbar");
        if (toolbar == null)
        {
            return;
        }

        var cancelButton = root.Q<Button>("Cancel");
        if (cancelButton == null)
        {
            cancelButton = new Button(CancelEditing)
            {
                name = "Cancel",
                text = "Cancel"
            };
            cancelButton.style.width = 160;
            toolbar.Add(cancelButton);
        }
        else
        {
            cancelButton.clicked += CancelEditing;
        }
    }

    void BuildProgramRoot()
    {
        if (graph == null)
        {
            return;
        }

        programRootStatement = new ProgramRootStatement();
        programRootBlock = new StatementBlockView(programRootStatement, "ProgramRoot");
        graph.AddFreeBlock(programRootBlock, new Vector2(32f, 32f));
        programRootBlock.Pin();
    }

    void ResetProgramGraph()
    {
        if (graph == null)
        {
            return;
        }

        ExpressionGraphController.Instance.ClearSelection();
        graph.Clear();
        BuildProgramRoot();
    }

    public void OpenForTarget(EnemyHackController target)
    {
        if (target == null)
        {
            return;
        }

        boundTarget = target;
        boundTarget.ClearCommands();
        EnemyCommandScriptContext.Bind(boundTarget);
        ResetProgramGraph();
        SetEditorVisible(true);
        PauseTimeForEditing();

        if (resultLabel != null)
        {
            resultLabel.text = $"Target: {boundTarget.name}";
        }
    }

    async void ExecuteProgram()
    {
        try
        {
            if (programRootStatement == null || programRootStatement.Body == null)
            {
                await EvaluateSelectedOrWarn();
                return;
            }

            if (boundTarget != null)
            {
                if (!boundTarget.GetHackStatus().IsActive)
                {
                    throw new InvalidOperationException("The hacked enemy is no longer active.");
                }

                boundTarget.ClearCommands();
                EnemyCommandScriptContext.Bind(boundTarget);
            }

            var result = await programRootStatement.ExecuteAsync();
            if (resultLabel != null)
            {
                resultLabel.text = $"Program queued (control flow: {result.Kind})";
            }

            if (boundTarget != null)
            {
                CloseEditing(cancelHack: false, clearQueuedCommands: false);
            }
        }
        catch (Exception exception)
        {
            boundTarget?.ClearCommands();
            if (resultLabel != null)
            {
                resultLabel.text = $"Error: {exception.Message}";
            }

            Debug.LogWarning($"[{nameof(ScriptEditorUI)}] Program execution failed: {exception.Message}", this);
        }
    }

    async Task EvaluateSelectedOrWarn()
    {
        var controller = ExpressionGraphController.Instance;

        if (controller.SelectedBlock == null)
        {
            if (resultLabel != null)
            {
                resultLabel.text = "Program is empty";
            }

            Debug.LogWarning("No program or block selected");
            return;
        }

        if (controller.SelectedBlock is ExpressionBlockView exprBlock)
        {
            var result = await exprBlock.Expression.EvaluateAsync();
            if (resultLabel != null)
            {
                resultLabel.text = $"Result: {result}";
            }

            return;
        }

        if (controller.SelectedBlock is StatementBlockView stmtBlock)
        {
            var result = await stmtBlock.Statement.ExecuteAsync();
            if (resultLabel != null)
            {
                resultLabel.text = $"Statement executed (control flow: {result.Kind})";
            }

            return;
        }

        Debug.LogWarning("Selected block is not evaluable");
    }

    void CancelEditing()
    {
        CloseEditing(cancelHack: true, clearQueuedCommands: true);
    }

    void CloseEditing(bool cancelHack, bool clearQueuedCommands)
    {
        EnemyHackController target = boundTarget;

        if (clearQueuedCommands)
        {
            target?.ClearCommands();
        }

        if (cancelHack)
        {
            target?.TryCancelHack();
        }

        EnemyCommandScriptContext.Clear(target);
        boundTarget = null;
        RestoreTimeAfterEditing();
        SetEditorVisible(false);
    }

    void SetEditorVisible(bool visible)
    {
        if (document == null)
        {
            return;
        }

        document.rootVisualElement.style.display = visible
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }

    void PauseTimeForEditing()
    {
        if (!pauseWhileEditing || timePausedForEditing)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        timePausedForEditing = true;
    }

    void RestoreTimeAfterEditing()
    {
        if (!timePausedForEditing)
        {
            return;
        }

        Time.timeScale = previousTimeScale;
        timePausedForEditing = false;
    }

    void SubscribeToHackEvents()
    {
        if (playerInteractor == null && autoFindPlayerInteractor)
        {
            playerInteractor = FindObjectOfType<PlayerInteractor>();
        }

        if (playerInteractor == null || subscribedInteractor == playerInteractor)
        {
            return;
        }

        UnsubscribeFromHackEvents();
        subscribedInteractor = playerInteractor;
        subscribedInteractor.HackCommandMenuRequested += OpenForTarget;
    }

    void UnsubscribeFromHackEvents()
    {
        if (subscribedInteractor == null)
        {
            return;
        }

        subscribedInteractor.HackCommandMenuRequested -= OpenForTarget;
        subscribedInteractor = null;
    }
}
