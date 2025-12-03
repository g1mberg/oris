using System.Text;
using MiniTemplateEngine.Parsing;
using MiniTemplateEngine.Runtime;

namespace MiniTemplateEngine.Rendering;

/// <summary>Обход AST и генерация строки</summary>
internal static class Render
{
    /// <summary>Render list of nodes into string</summary>
    public static string Run(List<Node> nodes, object model)
    {
        var context = new Context(model);
        context.Set("root", model);
        
        var sb = new StringBuilder(); // result
        
        foreach (var n in nodes) // working with nodes
            WriteNode(n, context, sb);
        
        return sb.ToString();
    }

    // Render of node
    private static void WriteNode(Node n, Context context, StringBuilder sb)
    {
        switch (n)
        {
            case TextNode t:
                sb.Append(t.Text);
                break;

            case VarNode v:
                var val = Eval.ResolvePath(v.Expr, context);
                sb.Append(val?.ToString() ?? string.Empty);
                break;

            case IfNode i:
                foreach (var child in Eval.EvaluateCondition(i.Condition, context) ? i.Then : i.Else ?? [])
                    WriteNode(child, context, sb);
                break;

            case ForEachNode f:
                var en = Eval.ResolveEnumerable(f.SourceExpr, context);
                if (en == null) break;
                foreach (var item in en)
                {
                    context.PushScope();
                    context.Set("this", item); 
                    context.Set(f.ItemVar, item); 
                    foreach (var child in f.Body)
                        WriteNode(child, context, sb);
                    context.PopScope();
                }

                break;

            default:
                throw new NotSupportedException($"Unknown node: {n.GetType().Name}");
        }
    }
}