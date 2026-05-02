using UnityEngine.UIElements;

public enum MessageType
{
    Info,
    Warning,
    Error
}

public class UIConsole: Foldout
{
    public static UIConsole Instance = new();
    private readonly ScrollView messages;
    private readonly Button clearButton;

    private UIConsole()
    {
        text = "Console";
        value = false;
        AddToClassList("ui-console");

        messages = new() 
        { 
            verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible,
            mouseWheelScrollSize = 1000,
            mode = ScrollViewMode.VerticalAndHorizontal
        };
        
        messages.style.height = 400;

        clearButton = new() { text = "Clear" };
        clearButton.AddToClassList("toolbar-button");
        clearButton.style.width = StyleKeyword.Auto;
        clearButton.style.marginBottom = 20;
        clearButton.style.marginRight = 30;

        clearButton.clicked += messages.Clear;

        Add(messages);
        Add(clearButton);
    }

    public void Write(string messageText, MessageType type)
    {
        switch(type)
        {
            case MessageType.Info:
                WriteInfo(messageText);
                break;
            case MessageType.Warning:
                WriteWarning(messageText);
                break;
            case MessageType.Error:
                WriteError(messageText);
                break;
        }
    }

    public void WriteInfo(string messageText)
    {
        Write(messageText, "ui-console-info");
    }

    public void WriteWarning(string messageText)
    {
        Write(messageText, "ui-console-warning");
    }

    public void WriteError(string messageText)
    {
        Write(messageText, "ui-console-error");
    }

    private void Write(string messageText, string style)
    {
        var message = new Label($"> {messageText}");
        message.AddToClassList(style);

        messages.Add(message);
    }
}