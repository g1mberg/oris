namespace HttpServer.core.handlers;

public class StaticFilesHandler : Handler
{
    public override void HandleRequest(int condition)
    {
        // некоторая обработка запроса
         
        if (condition==1)
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