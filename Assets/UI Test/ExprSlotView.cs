#nullable enable

using System;
using Script.Core.Expressions;
using UnityEngine.UIElements;
using UnityEngine;

public class ExprSlotView : VisualElement
{
    public IExpressionSlotHost ParentHost { get; }
    public int Index { get; }

    public ExpressionBlockView? ChildBlock { get; private set; }

    public ExprSlotView(IExpressionSlotHost parentHost, int index)
    {
        ParentHost = parentHost;
        Index = index;

        AddToClassList("expr-slot");

        Debug.Log($"[ExprSlotView] Created -> {ParentHost.DebugName}[{Index}]");

        RegisterCallback<ClickEvent>(evt =>
        {
            evt.StopPropagation();
            Debug.Log($"[ExprSlotView] Click -> {ParentHost.DebugName}[{Index}]");
            ExpressionGraphController.Instance.SelectSlot(this);
        });
    }

    public void SetChild(ExpressionBlockView block)
    {
        Debug.Log($"[ExprSlotView] SetChild -> {block.DebugName} -> {ParentHost.DebugName}[{Index}]");

        if (block.ParentSlot != null)
        {
            Debug.LogWarning($"[ExprSlotView] Block already connected -> {block.DebugName}");
        }

        block.RemoveFromHierarchy();
        block.AttachToSlot();

        ClearChild();

        ChildBlock = block;
        block.ParentSlot = this;

        ParentHost.SetExpression(Index, block.Expression);

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

        Debug.Log($"[ExprSlotView] ClearChild -> {ParentHost.DebugName}[{Index}]");

        ChildBlock.ParentSlot = null;
        ChildBlock.Expression.Parent = null;
        ParentHost.SetExpression(Index, null);
        ChildBlock = null;

        Clear();
    }
}
