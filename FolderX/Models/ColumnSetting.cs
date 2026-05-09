namespace FolderX.Models
{
    public class ColumnSetting
    {
        public string Name { get; set; } = string.Empty;
        public string PropertyKey { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public int Order { get; set; }

        public ColumnSetting(string name, string propertyKey, bool isVisible = true, int order = 0)
        {
            Name = name;
            PropertyKey = propertyKey;
            IsVisible = isVisible;
            Order = order;
        }
    }
}
