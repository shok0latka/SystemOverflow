#nullable enable

using UnityEngine.UIElements;
using UnityEngine;
using Script.UI.Views;
using System;

namespace Script.UI.Controllers 
{
    public class GraphRoot : VisualElement
    {
        public static GraphRoot? Instance;

        public GraphRoot()
        {
            if (Instance is not null)
            {
                throw new InvalidOperationException($"Second call of {nameof(GraphRoot)} constructor");    
            }

            Instance = this;
            Debug.Log("[GraphRoot] GraphRoot created");

            style.position = Position.Relative;
            style.flexGrow = 1;

            RegisterCallback<ClickEvent>(evt =>
            {
                Debug.Log("[GraphRoot] Empty space click");
                ExpressionGraphController.Instance.ClickOnEmptySpace(evt.localPosition);
            });
        }

        public void AddFreeBlock(ExpressionBlockView block)
        {
            Debug.Log($"Add free block {block.Expression.GetType()}");
            block.RemoveFromHierarchy();

            block.style.position = Position.Absolute;

            Add(block);
        }

        public void AddFreeBlock(ExpressionBlockView block, Vector2 position)
        {
            block.RemoveFromHierarchy();

            block.style.position = Position.Absolute;
            block.style.left = position.x;
            block.style.top = position.y;

            Add(block);
        }

        public void AddFreeBlock(StatementBlockView block)
        {
            Debug.Log($"Add free block {block.Statement.GetType()}");
            block.RemoveFromHierarchy();

            block.style.position = Position.Absolute;

            Add(block);
        }

        public void AddFreeBlock(StatementBlockView block, Vector2 position)
        {
            block.RemoveFromHierarchy();

            block.style.position = Position.Absolute;
            block.style.left = position.x;
            block.style.top = position.y;

            Add(block);
        }
    }
}