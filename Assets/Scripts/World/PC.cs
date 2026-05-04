using UnityEngine;

public class PC : Interactable
{
    [TextArea(3, 10)]
    public string terminalMessage = "Добро пожаловать в консоль System Overflow. Взлом роботов - ваша главная задача. Будьте осторожны.";

    public override void Interact()
    {
        Debug.Log("Взаимодействие с терминалом");
        
        DialogManager.Instance.ShowDialog(terminalMessage);
    }
}