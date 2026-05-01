#nullable enable

using System;
using System.Collections.Generic;
using Script.Core.Expressions;
using Script.Core.Statements;
using UnityEngine.UIElements;
using UnityEngine;
using System.Threading.Tasks;
using Script.UI.Controllers;

namespace Script.UI.Views 
{
    public class StatementBlockView : VisualElement, IExpressionSlotHost
    {
        public IStatement Statement { get; }
        public string DebugName { get; }

        public List<ExprSlotView> ExprSlots { get; } = new();
        public List<StmtSlotView> StmtSlots { get; } = new();

        public StmtSlotView? ParentSlot = null;

        readonly VisualElement content;

        public StatementBlockView(IStatement statement, string? debugName = null)
        {
            Statement = statement;
            DebugName = debugName ?? Guid.NewGuid().ToString();

            content = new VisualElement();
            content.AddToClassList("stmt-block");

            Add(content);

            RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                ExpressionGraphController.Instance.SelectBlock(this);
            });

            Statement.RegisterExecutionCallback(async () => await PlayExecutePulse(100));
            BuildTitle(statement.Name);
            BuildExpressionArguments();
        }

        public StatementBlockView(PrintStatement stmt, string? debugName = null) : this((IStatement)stmt, debugName)
        {
            BuildNextSlot();
        }

        public StatementBlockView(AssignStatement stmt, string? debugName = null) : this((IStatement)stmt, debugName)
        {
            BuildNextSlot();
        }

        public StatementBlockView(IfStatement stmt, string? debugName = null) : this((IStatement)stmt, debugName)
        {
            BuildStatementSlot("Do:", StmtSlotKind.Do);
            BuildStatementSlot("Else:", StmtSlotKind.Else);

            BuildNextSlot();
        }

        public StatementBlockView(WhileStatement stmt, string? debugName = null) : this((IStatement)stmt, debugName)
        {
            BuildStatementSlot("Body:", StmtSlotKind.Body);

            BuildNextSlot();
        }

        void BuildTitle(string text)
        {
            var title = new Label(text);
            title.AddToClassList("stmt-title");
            content.Add(title);
        }

        void BuildExpressionArguments()
        {
            for (int i = 0; i < Statement.Arguments.Count; i++)
            {
                var arg = Statement.Arguments[i];

                var label = new Label(arg.Name + ":");
                label.AddToClassList("stmt-arg-label");
                content.Add(label);

                var slot = new ExprSlotView(this, i);
                ExprSlots.Add(slot);
                content.Add(slot);
            }
        }

        void BuildStatementSlot(string labelText, StmtSlotKind kind)
        {
            var label = new Label(labelText);
            label.AddToClassList("stmt-arg-label");
            content.Add(label);

            var slot = new StmtSlotView(this, kind);
            StmtSlots.Add(slot);
            content.Add(slot);
        }

        void BuildNextSlot()
        {
            var label = new Label("Next:");
            label.AddToClassList("stmt-arg-label");

            var slot = new StmtSlotView(this, StmtSlotKind.Next);

            StmtSlots.Add(slot);

            content.Add(label);
            Add(slot);
        }

        public async Task PlayExecutePulse(int delayMs)
        {
            content.AddToClassList("stmt-eval");

            await Task.Delay(delayMs);

            schedule.Execute(() => { content.RemoveFromClassList("stmt-eval"); }).ExecuteLater(200);
        }

        public void SetExpression(int index, Expression? expression)
        {
            if (index < 0 || index >= Statement.Arguments.Count)
                return;

            Statement.Arguments[index].Attached = expression;
        }

        public Expression? GetExpression(int index)
        {
            if (index < 0 || index >= Statement.Arguments.Count)
                return null;

            return Statement.Arguments[index].Attached;
        }

        public void SetStatementSlot(StmtSlotKind kind, StatementBlockView block)
        {
            switch (kind)
            {
                case StmtSlotKind.Next:
                    Statement.Next = block.Statement;
                    break;
                case StmtSlotKind.Do:
                    if (Statement is IfStatement ifStmt)
                        ifStmt.Do = block.Statement;
                    break;
                case StmtSlotKind.Else:
                    if (Statement is IfStatement ifStmt2)
                        ifStmt2.Else = block.Statement;
                    break;
                case StmtSlotKind.Body:
                    if (Statement is WhileStatement whileStmt)
                        whileStmt.Body = block.Statement;
                    break;
            }
        }

        public void ClearStatementSlot(StmtSlotKind kind)
        {
            switch (kind)
            {
                case StmtSlotKind.Next:
                    Statement.Next = null;
                    break;
                case StmtSlotKind.Do:
                    if (Statement is IfStatement ifStmt)
                        ifStmt.Do = null;
                    break;
                case StmtSlotKind.Else:
                    if (Statement is IfStatement ifStmt2)
                        ifStmt2.Else = null;
                    break;
                case StmtSlotKind.Body:
                    if (Statement is WhileStatement whileStmt)
                        whileStmt.Body = null;
                    break;
            }
        }

        public void AttachToSlot()
        {
            style.position = Position.Relative;
            style.left = StyleKeyword.Null;
            style.top = StyleKeyword.Null;
        }

        public void MakeFree(Vector2 pos)
        {
            style.position = Position.Absolute;

            var size = content.resolvedStyle;

            style.left = pos.x - size.width / 2;
            style.top = pos.y - size.height / 2;
        }
    }
}