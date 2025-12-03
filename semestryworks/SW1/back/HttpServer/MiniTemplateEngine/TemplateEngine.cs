using MiniTemplateEngine.Abstraction;
using MiniTemplateEngine.Parsing;
using MiniTemplateEngine.Rendering;

namespace MiniTemplateEngine;

/// <summary>реализация IHtmlTemplateRenderer на базе Parser</summary>
public sealed class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    /// <inheritdoc />
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        var ast = Parser.Parse(htmlTemplate); // template parsing
        return Render.Run(ast, dataModel); // render
        
    }

    /// <inheritdoc />
    public string RenderFromFile(string filePath, object dataModel)
    {
        var text = File.ReadAllText(filePath); // file to string
        return RenderFromString(text, dataModel); // render from string
    }

    /// <inheritdoc />
    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        var result = RenderFromFile(inputFilePath, dataModel); // get result
        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? "."); // create directory if not exist
        File.WriteAllText(outputFilePath, result); // write into file
        return result; // returns string
    }
}