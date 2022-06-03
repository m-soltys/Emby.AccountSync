namespace AccountSync
{
    using System.Linq;
    using System.Text;

    public static class Extensions
    {
        public static string PropertiesToString(this object obj)
            => obj.GetType().GetProperties()
                .Select(info => (info.Name, Value: info.GetValue(obj, null) ?? "(null)"))
                .Aggregate(
                    new StringBuilder("\n"),
                    (sb, pair) => sb.AppendLine($"{pair.Name}: {pair.Value}"),
                    sb => sb.ToString());
    }
}