using CatFactory.CodeFactory;

namespace CatFactory.EfCore
{
    public static class LineHelper
    {
        public static PreprocessorDirectiveLine GetWarning(string message, params string[] args)
            => new PreprocessorDirectiveLine(string.Concat("warning ", message), args);

        public static PreprocessorDirectiveLine GetWarning(int indent, string message, params string[] args)
            => new PreprocessorDirectiveLine(indent, string.Concat("warning ", message), args);

        public static PreprocessorDirectiveLine Region(string message, params string[] args)
            => new PreprocessorDirectiveLine(string.Concat("region ", message), args);

        public static PreprocessorDirectiveLine EndRegion(string message, params string[] args)
            => new PreprocessorDirectiveLine("endregion");
    }
}
