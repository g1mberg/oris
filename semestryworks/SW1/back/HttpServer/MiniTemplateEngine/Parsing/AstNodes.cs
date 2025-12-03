namespace MiniTemplateEngine.Parsing;

internal abstract class Node
{
}

internal sealed class TextNode(string text) : Node
{
    public readonly string Text = text; // сырой текст
}

internal sealed class VarNode(string expr) : Node
{
    public readonly string Expr = expr; 
}

internal sealed class IfNode(string condition, List<Node> thenPart, List<Node>? elsePart)
    : Node
{
    public readonly string Condition = condition; 
    public readonly List<Node>? Else = elsePart; 
    public readonly List<Node> Then = thenPart; 
}

internal sealed class ForEachNode(string itemVar, string sourceExpr, List<Node> body) : Node
{
    public readonly List<Node> Body = body; 
    public readonly string ItemVar = itemVar; 
    public readonly string SourceExpr = sourceExpr; 
}