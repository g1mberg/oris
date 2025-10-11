namespace HttpServer.core.handlers;

public class FindMethodsHandler : Handler
{
    public override void HandleRequest(int condition)
    {
        // некоторая обработка запроса
         
        if (condition==2)
        {
            // завершение выполнения запроса;
        }
        // передача запроса дальше по цепи при наличии в ней обработчиков
        else if (Successor != null)
        {
            Successor.HandleRequest(condition);
        }
    }
}