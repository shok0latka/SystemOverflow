#nullable enable

using System;
using Script.Core.Expressions;
using UnityEngine.UIElements;
using UnityEngine;

public class SlotView : VisualElement
{
    public ExpressionBlockView ParentBlock { get; }
    public int Index { get; }

    public ExpressionBlockView? ChildBlock { get; private set; }

    public SlotView(ExpressionBlockView parent, int index)
    {
        ParentBlock = parent;
        Index = index;

        AddToClassList("expr-slot");

        Debug.Log($"[SlotView] Created -> {ParentBlock.DebugName}[{Index}]");

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            Debug.Log($"[SlotView] Click -> {ParentBlock.DebugName}[{Index}]");
            ExpressionGraphController.Instance.SelectSlot(this);
        });
    }

    

    public void SetChild(ExpressionBlockView block)
    {
        Debug.Log($"[SlotView] SetChild -> {block.DebugName} -> {ParentBlock.DebugName}[{Index}]");

        if (block.ParentSlot != null)
        {
            Debug.LogWarning($"[SlotView] Block already connected -> {block.DebugName}");
        }

        block.RemoveFromHierarchy();
        block.AttachToSlot();

        ClearChild();

        ChildBlock = block;
        block.ParentSlot = this;

        ParentBlock.Expression[Index] = block.Expression;

        Add(block);
    }

    public ExpressionBlockView? ReplaceChild(ExpressionBlockView newBlock)
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

        Debug.Log($"[SlotView] ClearChild -> {ParentBlock.DebugName}[{Index}]");

        ChildBlock.ParentSlot = null;
        ChildBlock.Expression.Parent = null;
        ParentBlock.Expression[Index] = null;
        ChildBlock = null;

        Clear();
    }
}