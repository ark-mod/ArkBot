namespace ArkBot.ViewModel
{
    public abstract class PaneViewModel : ViewModelBase
    {
        public PaneViewModel(string contentId, string title)
        {
            ContentId = contentId;
            Title = title;
        }

        //public ImageSource IconSource { get; protected set; }

        public string Title { get; set; }

        public string ContentId { get; set; }

        public bool IsSelected { get; set; }

        public bool IsActive { get; set; }
    }
}
