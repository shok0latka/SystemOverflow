#nullable enable

using System;
using Script.Core.Expressions;
using Script.Core.Statements;
using Script.UI.Views;
using UnityEngine;
using UnityEngine.UIElements;


namespace Script.UI.Controllers 
{
    public class ExpressionGraphController
    {
        public static ExpressionGraphController Instance = new();

        public VisualElement? SelectedBlock;
        public VisualElement? SelectedSlot;

        void ToggleSelection(VisualElement element, ref VisualElement? selected)
        {
            if (selected == element)
            {
                element.RemoveFromClassList("expr-selected");
                selected = null;
                return;
            }

            selected?.RemoveFromClassList("expr-selected");

            selected = element;
            element.AddToClassList("expr-selected");
        }

        public void SelectBlock(ExpressionBlockView block)
        {
            Debug.Log($"[GraphController] SelectExpressionBlock -> {block.DebugName}");

            ToggleSelection(block, ref SelectedBlock);
            TryConnect();
        }

        public void SelectBlock(StatementBlockView block)
        {
            Debug.Log($"[GraphController] SelectStatementBlock -> {block.DebugName}");

            ToggleSelection(block, ref SelectedBlock);
            TryConnect();
        }

        public void SelectSlot(ExprSlotView slot)
        {
            Debug.Log($"[GraphController] SelectExprSlot -> {slot.ParentHost.DebugName}[{slot.Index}]");

            ToggleSelection(slot, ref SelectedSlot);
            TryConnect();
        }

        public void SelectSlot(StmtSlotView slot)
        {
            Debug.Log($"[GraphController] SelectStmtSlot -> {slot.ParentBlock.DebugName}:{slot.Kind}");

            ToggleSelection(slot, ref SelectedSlot);
            TryConnect();
        }

        public void ClickOnEmptySpace(Vector2 mousePosition)
        {
            if (SelectedBlock == null)
                return;

            if (SelectedBlock is ExpressionBlockView exprBlock)
            {
                Debug.Log($"[GraphController] Place expression block '{exprBlock.DebugName}' on canvas");
                DetachBlock(exprBlock);
                exprBlock.MakeFree(mousePosition);
                exprBlock.RemoveFromClassList("expr-selected");
            }
            else if (SelectedBlock is StatementBlockView stmtBlock)
            {
                Debug.Log($"[GraphController] Place statement block '{stmtBlock.DebugName}' on canvas");
                DetachBlock(stmtBlock);
                stmtBlock.MakeFree(mousePosition);
                stmtBlock.RemoveFromClassList("expr-selected");
            }

            SelectedBlock = null;
        }

        public void DetachBlock(ExpressionBlockView block)
        {
            Debug.Log($"[GraphController] DetachExpressionBlock -> {block.DebugName}");

            block.ParentSlot?.ClearChild();

            GraphRoot.Instance?.AddFreeBlock(block);
        }

        public void DetachBlock(StatementBlockView block)
        {
            Debug.Log($"[GraphController] DetachStatementBlock -> {block.DebugName}");

            block.ParentSlot?.ClearChild();

            GraphRoot.Instance?.AddFreeBlock(block);
        }

        void TryConnect()
        {
            if (SelectedBlock == null || SelectedSlot == null)
                return;
            
            if (SelectedBlock is ExpressionBlockView exprBlock && SelectedSlot is ExprSlotView exprSlot)
            {
                Connect(exprBlock, exprSlot);
                ClearSelection();
                return;
            }

            if (SelectedBlock is ExpressionBlockView exprItem && SelectedSlot is ExprSlotView exprSlot2)
            {
                Connect(exprItem, exprSlot2);
                ClearSelection();
                return;
            }

            if (SelectedBlock is StatementBlockView stmtBlock && SelectedSlot is StmtSlotView stmtSlot)
            {
                Connect(stmtBlock, stmtSlot);
                ClearSelection();
                return;
            }

            ClearSelection();
        }

        public void ClearSelection()
        {
            SelectedSlot?.RemoveFromClassList("expr-selected");
            SelectedBlock?.RemoveFromClassList("expr-selected");
            SelectedBlock = null;
            SelectedSlot = null;
        }

        void Connect(ExpressionBlockView block, ExprSlotView slot)
        {
            var parentExpr = slot.ParentHost.GetExpression(slot.Index);
            var childExpr = block.Expression;

            Debug.Log($"[GraphController] Connect -> {block.DebugName} -> {slot.ParentHost.DebugName}[{slot.Index}]");

            if (slot.ParentHost is ExpressionBlockView eqHost && eqHost == block)
            {
                UIConsole.Instance.WriteWarning($"Connection rejected: cycle risk");
                return;
            }

            if (parentExpr != null && WouldCreateCycle(parentExpr, childExpr))
            {
                UIConsole.Instance.WriteWarning($"Connection rejected: cycle risk");
                return;
            }

            if (block.ParentSlot != null && block.ParentSlot != slot)
            {
                Debug.Log($"[GraphController] Detach {block.DebugName} from previous slot");
                block.ParentSlot.ClearChild();
            }

            var pos = block.ChangeCoordinatesTo(GraphRoot.Instance, new Vector2(0, 0));
            GraphRoot.Instance?.AddFreeBlock(block, pos);

            try
            {
                var replaced = slot.ReplaceChild(block);
                if (replaced != null)
                {
                    Debug.Log($"[GraphController] Slot occupied, freeing block {replaced.DebugName}");
                    GraphRoot.Instance?.AddFreeBlock(replaced);
                }

                Debug.Log($"[GraphController] Connect success: {block.DebugName} -> {slot.ParentHost.DebugName}[{slot.Index}]");
            }
            catch (Exception e)
            {
                GraphRoot.Instance?.AddFreeBlock(block, pos);
                UIConsole.Instance.WriteError(e.Message);
            }
        }

        void Connect(StatementBlockView block, StmtSlotView slot)
        {
            Debug.Log($"[GraphController] Connect statement -> {block.DebugName} -> {slot.ParentBlock.DebugName}:{slot.Kind}");

            if (slot.ParentBlock == block || WouldCreateStatementCycle(slot.ParentBlock.Statement, block.Statement))
            {
                UIConsole.Instance.WriteWarning($"[GraphController] Connection rejected: cycle risk");
                return;
            }

            if (block.ParentSlot != null && block.ParentSlot != slot)
            {
                Debug.Log($"[GraphController] Detach {block.DebugName} from previous slot");
                block.ParentSlot.ClearChild();
            }

            var pos = block.ChangeCoordinatesTo(GraphRoot.Instance, new Vector2(0, 0));
            GraphRoot.Instance?.AddFreeBlock(block, pos);

            try
            {
                var replaced = slot.ReplaceChild(block);
                if (replaced != null)
                {
                    Debug.Log($"[GraphController] Slot occupied, freeing block {replaced.DebugName}");
                    GraphRoot.Instance?.AddFreeBlock(replaced);
                }

                Debug.Log($"[GraphController] Connect success: {block.DebugName} -> {slot.ParentBlock.DebugName}:{slot.Kind}");
            }
            catch (Exception e)
            {
                GraphRoot.Instance?.AddFreeBlock(block, pos);
                UIConsole.Instance.WriteError(e.Message);
            }
        }

        bool WouldCreateCycle(Expression parent, Expression child)
        {
            if (parent == child)
                return true;

            int arity = child.Arity();

            for (int i = 0; i < arity; i++)
            {
                var input = child[i];

                if (input != null && WouldCreateCycle(parent, input))
                    return true;
            }

            return false;
        }

        bool WouldCreateStatementCycle(IStatement parent, IStatement child)
        {
            if (ReferenceEquals(parent, child))
                return true;

            if (child.Next != null && WouldCreateStatementCycle(parent, child.Next))
                return true;

            if (child is IfStatement ifStmt)
            {
                if (ifStmt.Do != null && WouldCreateStatementCycle(parent, ifStmt.Do))
                    return true;
                if (ifStmt.Else != null && WouldCreateStatementCycle(parent, ifStmt.Else))
                    return true;
            }
            else if (child is WhileStatement whileStmt)
            {
                if (whileStmt.Body != null && WouldCreateStatementCycle(parent, whileStmt.Body))
                    return true;
            }

            return false;
        }
    }
}
