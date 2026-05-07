using System;
using UnityEngine;
using UnityEngine.UIElements;

public class HackedRobotAccessButton
{
    private const string ButtonName = "HackedRobotAccess";
    private const string ButtonText = "Робот";
    private const string ButtonTooltip = "Open hacked robot command menu";

    private readonly Action<EnemyHackController> _openTarget;
    private readonly Button _button;
    private bool _editorVisible;

    public HackedRobotAccessButton(VisualElement root, Action<EnemyHackController> openTarget)
    {
        _openTarget = openTarget;
        _button = root.Q<Button>(ButtonName);
        if (_button == null)
        {
            _button = new Button(OnClicked)
            {
                name = ButtonName
            };

            root.Add(_button);
        }
        else
        {
            _button.clicked += OnClicked;
        }

        ApplyStyle();
        Refresh(editorVisible: false);
    }

    public void Refresh(bool editorVisible)
    {
        _editorVisible = editorVisible;

        bool showButton = !editorVisible && IsActiveHackTarget(EnemyHackController.ActiveHack);
        _button.style.display = showButton ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnClicked()
    {
        EnemyHackController target = EnemyHackController.ActiveHack;
        if (!IsActiveHackTarget(target))
        {
            Refresh(_editorVisible);
            return;
        }

        _openTarget?.Invoke(target);
    }

    private void ApplyStyle()
    {
        _button.text = ButtonText;
        _button.tooltip = ButtonTooltip;
        _button.pickingMode = PickingMode.Position;
        _button.style.position = Position.Absolute;
        _button.style.top = 12;
        _button.style.right = 12;
        _button.style.width = 88;
        _button.style.height = 36;
        _button.style.fontSize = 16;
        _button.style.unityFontStyleAndWeight = FontStyle.Bold;
        _button.style.borderTopLeftRadius = 6;
        _button.style.borderTopRightRadius = 6;
        _button.style.borderBottomLeftRadius = 6;
        _button.style.borderBottomRightRadius = 6;
    }

    private static bool IsActiveHackTarget(EnemyHackController target)
    {
        return target != null && target.GetHackStatus().IsActive;
    }
}
