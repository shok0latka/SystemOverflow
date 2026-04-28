#nullable enable

using System;
using Script.Core.Statements;
using UnityEngine.UIElements;
using UnityEngine;

public enum StmtSlotKind
{
    Next,
    Do,
    Else,
    Body
}

public class StmtSlotView : VisualElement
{
    public StatementBlockView ParentBlock { get; }
    public StmtSlotKind Kind { get; }

    public StatementBlockView? ChildBlock { get; private set; }

    readonly VisualElement placeholder;

    public StmtSlotView(StatementBlockView parent, StmtSlotKind kind)
    {
        ParentBlock = parent;
        Kind = kind;

        AddToClassList("stmt-slot");

        placeholder = new VisualElement();
        placeholder.AddToClassList("stmt-slot-placeholder");

        Add(placeholder);

        Debug.Log($"[StmtSlotView] Created -> {ParentBlock.DebugName}:{Kind}");

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            Debug.Log($"[StmtSlotView] Click -> {ParentBlock.DebugName}:{Kind}");
            ExpressionGraphController.Instance.SelectSlot(this);
        });
    }

    public void SetChild(StatementBlockView block)
    {
        Debug.Log($"[StmtSlotView] SetChild -> {block.DebugName} -> {ParentBlock.DebugName}:{Kind}");

        if (block.ParentSlot != null)
        {
            Debug.LogWarning($"[StmtSlotView] Block already connected -> {block.DebugName}");
        }

        block.RemoveFromHierarchy();
        block.AttachToSlot();

        ClearChild();

        ChildBlock = block;
        block.ParentSlot = this;

        ParentBlock.SetStatementSlot(Kind, block);

        Add(block);

        placeholder.style.display = DisplayStyle.None;
    }

    public StatementBlockView? ReplaceChild(StatementBlockView newBlock)
    {
        var old = ChildBlock;

        ClearChild();
        SetChild(newBlock);

        return old;
    }

    public void ClearChild()
    {
        if (ChildBlock is null)
            return;

        Debug.Log($"[StmtSlotView] ClearChild -> {ParentBlock.DebugName}:{Kind}");

        ChildBlock.ParentSlot = null;
        ParentBlock.ClearStatementSlot(Kind);

        ChildBlock = null;

        Clear();
        Add(placeholder);

        placeholder.style.display = DisplayStyle.Flex;
    }
}
