using CatFactory.CodeFactory;

namespace CatFactory.EfCore
{
    public static class LineHelper
    {
        public static PreprocessorDirectiveLine GetWarning(string message, params string[] args)
            => new PreprocessorDirectiveLine(string.Concat("#warning ", message), args);
    }
}
