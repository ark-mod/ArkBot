namespace ArkBot.Modules.Application.ViewModel
{
    public abstract class ToolViewModel : PaneViewModel
    {
        public ToolViewModel(string contentId, string name) : base(contentId, name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public bool IsVisible { get; set; }
    }
}
